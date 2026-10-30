using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Globalna tablica wynikow (Unity Leaderboards): anonimowe logowanie, nazwa gracza, wysylka SCORE, TOP 10.
    /// Konto anonimowe istnieje tylko na czas operacji (zapis wyniku albo odswiezenie tablicy), potem wylogowanie z wyczyszczeniem sesji.
    /// Kazdy wynik idzie jako nowy gracz (swieze ID), bo tablica trzyma jeden wpis na ID: ten sam gracz moze miec w TOP 10 wiele wpisow.
    /// Podswietlenie "mine" tylko dla wynikow wyslanych zaraz po runie (ID w pamieci, do ClearHighlight).
    /// Gra nigdy nie czeka na siec: kazdy blad konczy sie komunikatem, a wynik czeka w kolejce (Save) do nastepnej proby.
    /// </summary>
    public static class Online
    {
        public const string BoardId = "pay_with_pixels_score";
        /// <summary>Puste = srodowisko domyslne (production); Leaderboards i Authentication nie maja wlasnych srodowisk w panelu.</summary>
        public const string EnvName = "";
        /// <summary>Unity Cloud Project ID; puste, gdy projekt jest polaczony z chmura w Edytorze (Project Settings > Services).</summary>
        public const string ProjectId = "";
        public const int MaxSaneScore = 200000;   // wyniki powyzej to blad albo oszustwo: nie wysylamy

        private const string KeyName = "pwp_player_name";
        private const string KeyPending = "pwp_pending_score";      // stary format: jeden najlepszy wynik (migrowany do kolejki)
        private const string KeyQueue = "pwp_pending_scores";       // kolejka wynikow do wyslania: "120,95,300"
        private static readonly HashSet<string> recentIds = new HashSet<string>();   // ID wynikow z ostatniego runu (tylko pamiec)
        private const int TimeoutMs = 10000;

        public struct Row { public int rank; public string name; public int score; public bool mine; public string id; }
        public enum Status { Off, Connecting, Ready, Error }

        public static Status State { get; private set; } = Status.Off;
        public static string Message { get; private set; } = "";
        public static int Version { get; private set; }   // rosnie przy kazdej zmianie danych (UI odswieza sie po nim)
        public static readonly List<Row> Top = new List<Row>();
        public static Row? Mine { get; private set; }
        public static bool Busy { get; private set; }
        /// <summary>True, gdy ktorys z wynikow ostatniego runu jest w TOP 10 (wazne po zakonczeniu odswiezenia).</summary>
        public static bool InTop10 => Top.Exists(r => r.mine);
        /// <summary>True, gdy wynik czeka w kolejce na wyslanie.</summary>
        public static bool HasPendingScore => LoadQueue().Count > 0;

        public static string PlayerName => Save.GetString(KeyName, "");
        public static bool HasName => PlayerName.Length > 0;

        /// <summary>Wylacznik tablicy wynikow: false = gra w ogole nie laczy sie z Unity Services (build WebGL padal na wyjatku przy Init).</summary>
        public const bool Enabled = true;

        private static bool Disabled => Enabled == false || Application.isBatchMode;   // testy i boty nie lacza sie z siecia

        /// <summary>False w batchmode (testy, boty): wtedy tablica nie jest nigdy pokazywana.</summary>
        public static bool IsAvailable => Disabled == false;

        private static void Changed() => Version++;

        public static bool HasEverLoaded { get; private set; } = false;

        /// <summary>Pasek stanu do UI tablicy.</summary>
        public static string StatusLine()
        {
            if (Disabled) return "OFFLINE MODE";
            switch (State)
            {
                case Status.Connecting: return "CONNECTING...";
                case Status.Error: return "OFFLINE - SCORES ARE SENT LATER";
                case Status.Ready: return (!HasEverLoaded && Busy) ? "LOADING..." : "";
                default: return "OFFLINE";
            }
        }

        public static async void Init()
        {
            if (Disabled || State == Status.Connecting || State == Status.Ready) return;
            State = Status.Connecting;
            Changed();
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions();
                    if (!string.IsNullOrEmpty(EnvName)) options.SetEnvironmentName(EnvName);
                    if (!string.IsNullOrEmpty(ProjectId)) options.SetOption("com.unity.services.core.cloud-project-id", ProjectId);
                    if (!await WithTimeout(UnityServices.InitializeAsync(options))) throw new TimeoutException("init timeout");
                }
                State = Status.Ready;
                Message = "";
                Changed();
                await FlushAndRefresh();
            }
            catch (Exception e)
            {
                State = Status.Error;
                Message = e.GetType().Name + ": " + e.Message;
                Debug.Log("[online] " + e.Message);
                Changed();
            }
        }

        private static bool sessionBusy;

        /// <summary>Jedna operacja sieciowa naraz: logowanie i wylogowanie nie moga sie przeplatac.</summary>
        private static async Task AcquireSession()
        {
            while (sessionBusy) await Task.Yield();
            sessionBusy = true;
        }

        private static async Task SignInFresh()
        {
            EndSession();
            if (!await WithTimeout(AuthenticationService.Instance.SignInAnonymouslyAsync())) throw new TimeoutException("sign-in timeout");
        }

        private static void EndSession()
        {
            if (AuthenticationService.Instance.IsSignedIn) AuthenticationService.Instance.SignOut(true);
        }

        private static bool OwnsRecentEntry => AuthenticationService.Instance.IsSignedIn && recentIds.Contains(AuthenticationService.Instance.PlayerId);

        /// <summary>Wiersz tablicy, ktorego nazwe gracz moze jeszcze wpisac: wynik ostatniego runu z otwartego konta.</summary>
        public static bool CanEdit(Row r) => State == Status.Ready && r.mine && OwnsRecentEntry && r.id == AuthenticationService.Instance.PlayerId;

        private static string nameOverrideId, nameOverride;

        /// <summary>Zatwierdzenie nazwy wpisanej w tablicy: zapis lokalny i zmiana nazwy konta ostatniego wyniku. Pusta nazwa = zostaje losowa z Authentication.</summary>
        public static async void ApplyName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return;
            SetName(raw);
            if (State != Status.Ready) return;
            // nowa nazwa widoczna od razu, jeszcze przed potwierdzeniem przez chmure (odswiezenia ja podtrzymuja do konca zmiany)
            if (OwnsRecentEntry)
            {
                nameOverrideId = AuthenticationService.Instance.PlayerId;
                nameOverride = PlayerName;
                for (int i = 0; i < Top.Count; i++)
                    if (Top[i].id == nameOverrideId) { var row = Top[i]; row.name = nameOverride; Top[i] = row; }
                Changed();
            }
            await AcquireSession();
            try
            {
                if (OwnsRecentEntry) await WithTimeout(AuthenticationService.Instance.UpdatePlayerNameAsync(PlayerName));
            }
            catch (Exception e) { Debug.Log("[online] name: " + e.Message); }
            sessionBusy = false;
            nameOverrideId = null;
            await RefreshInternal();
        }

        /// <summary>Po runie i przy wejsciu do tytulu/nowego runu: wpisy z ostatniego runu przestaja byc podswietlone.</summary>
        public static void ClearHighlight()
        {
            if (recentIds.Count == 0) return;
            recentIds.Clear();
            if (!sessionBusy) try { EndSession(); } catch (Exception) { }
            Mine = null;
            for (int i = 0; i < Top.Count; i++) { var r = Top[i]; r.mine = false; Top[i] = r; }
            Changed();
        }

        /// <summary>Czeka na zadanie najwyzej TimeoutMs. Bez Task.Delay (w WebGL nie ma watkow/timerow): odpytuje w kolejnych klatkach.</summary>
        private static async Task<bool> WithTimeout(Task t)
        {
            float start = Time.realtimeSinceStartup;
            while (!t.IsCompleted)
            {
                if (Time.realtimeSinceStartup - start > TimeoutMs / 1000f)
                {
                    t.ContinueWith(x => { var ignored = x.Exception; }, TaskContinuationOptions.OnlyOnFaulted);   // spozniony blad nie trafia do logu
                    return false;
                }
                await Task.Yield();
            }
            await t;   // przenosi wyjatek (np. brak sieci) do wywolujacego catch
            return true;
        }

        /// <summary>Nazwa gracza: 3-16 znakow A-Z, 0-9, _ (reszta odrzucana).</summary>
        public static string CleanName(string raw)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in (raw ?? "").ToUpperInvariant())
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_') sb.Append(c);
            string s = sb.ToString();
            if (s.Length > 16) s = s.Substring(0, 16);
            return s.Length >= 3 ? s : (s + "PLAYER").Substring(0, Mathf.Max(3, s.Length));
        }

        public static void SetName(string raw)
        {
            Save.SetString(KeyName, CleanName(raw));
            Save.Flush();
            Changed();
        }

        private static List<int> LoadQueue()
        {
            var list = new List<int>();
            int legacy = Save.GetInt(KeyPending, 0);
            if (legacy > 0) list.Add(legacy);
            foreach (var part in Save.GetString(KeyQueue, "").Split(','))
                if (int.TryParse(part, out int v) && v > 0) list.Add(v);
            return list;
        }

        private static void StoreQueue(List<int> list)
        {
            Save.SetInt(KeyPending, 0);
            Save.SetString(KeyQueue, string.Join(",", list));
            Save.Flush();
        }

        /// <summary>Wynik po zgonie/wygranej: dopisany do kolejki od razu, wysylany gdy siec dziala (kazdy wynik osobno).</summary>
        public static void Submit(int score)
        {
            if (score <= 0 || score > MaxSaneScore) return;
            var queue = LoadQueue();
            queue.Add(score);
            StoreQueue(queue);
            if (State == Status.Off || State == Status.Error) Init();
            else if (State == Status.Ready) _ = FlushAndRefresh();
        }

        private static async Task FlushAndRefresh()
        {
            Busy = true;   // od wyslania do konca odswiezenia: UI nie decyduje na starej tablicy
            await FlushPending();
            await RefreshInternal();
            Busy = false;
        }

        private static bool flushing;

        private static async Task FlushPending()
        {
            if (State != Status.Ready) return;
            await AcquireSession();
            try
            {
                while (true)
                {
                    var queue = LoadQueue();
                    if (queue.Count == 0) break;
                    await SignInFresh();   // swieze ID: tablica trzyma jeden wpis na ID
                    if (HasName && !await WithTimeout(AuthenticationService.Instance.UpdatePlayerNameAsync(PlayerName))) throw new TimeoutException("name timeout");   // bez wpisanej nazwy zostaje losowa nazwa z Authentication
                    if (!await WithTimeout(LeaderboardsService.Instance.AddPlayerScoreAsync(BoardId, queue[0]))) throw new TimeoutException("submit timeout");
                    recentIds.Add(AuthenticationService.Instance.PlayerId);
                    queue.RemoveAt(0);
                    StoreQueue(queue);
                }
            }
            catch (Exception e) { Fail("submit: " + e.Message); }
            sessionBusy = false;   // sesja zostaje otwarta dla odswiezenia, ktore ja zamknie
        }

        public static void Refresh()
        {
            if (Disabled) return;
            if (State == Status.Ready) _ = RefreshInternal();
            else if (State != Status.Connecting) Init();
        }

        private static async Task RefreshInternal()
        {
            if (State != Status.Ready) return;
            await AcquireSession();
            Busy = true;
            Changed();
            try
            {
                if (!AuthenticationService.Instance.IsSignedIn) await SignInFresh();
                var pageTask = LeaderboardsService.Instance.GetScoresAsync(BoardId, new GetScoresOptions { Limit = 10 });
                if (!await WithTimeout(pageTask)) throw new TimeoutException("scores timeout");
                var page = pageTask.Result;
                Top.Clear();
                foreach (var e in page.Results)
                    Top.Add(new Row { rank = e.Rank + 1, name = e.PlayerId == nameOverrideId ? nameOverride : Strip(e.PlayerName), score = (int)e.Score, mine = recentIds.Contains(e.PlayerId), id = e.PlayerId });
                HasEverLoaded = true;
                Mine = null;
                foreach (var r in Top) if (r.mine) { Mine = r; break; }
                if (Mine == null && recentIds.Count > 0)
                {
                    try
                    {
                        var meTask = LeaderboardsService.Instance.GetScoresByPlayerIdsAsync(BoardId, new List<string>(recentIds));
                        if (!await WithTimeout(meTask)) throw new TimeoutException("player score timeout");
                        foreach (var me in meTask.Result.Results)   // najlepszy z wynikow ostatniego runu poza TOP 10
                            if (Mine == null || me.Score > Mine.Value.score)
                                Mine = new Row { rank = me.Rank + 1, name = Strip(me.PlayerName), score = (int)me.Score, mine = true, id = me.PlayerId };
                    }
                    catch (Exception) { /* brak wyniku na tablicy */ }
                }
            }
            catch (Exception e) { Fail("refresh: " + e.Message); }
            try { if (!OwnsRecentEntry) EndSession(); } catch (Exception) { }   // konto ostatniego wyniku zostaje do wpisania nazwy
            sessionBusy = false;
            Busy = false;
            Changed();
        }

        /// <summary>Blad uslugi (np. zla konfiguracja tablicy): stan Error, UI nie pokazuje tablicy.</summary>
        private static void Fail(string message)
        {
            State = Status.Error;
            Message = message;
            Debug.Log("[online] " + message);
        }

        private static string Strip(string n)
        {
            if (string.IsNullOrEmpty(n)) return "PLAYER";
            int hash = n.IndexOf('#');
            return hash > 0 ? n.Substring(0, hash) : n;
        }
    }
}
