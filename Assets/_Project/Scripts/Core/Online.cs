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
        private const string KeyPending = "pwp_pending_score";
        private const int TimeoutMs = 10000;

        public struct Row { public int rank; public string name; public int score; public bool mine; }
        public enum Status { Off, Connecting, Ready, Error }

        public static Status State { get; private set; } = Status.Off;
        public static string Message { get; private set; } = "";
        public static int Version { get; private set; }   // rosnie przy kazdej zmianie danych (UI odswieza sie po nim)
        public static readonly List<Row> Top = new List<Row>();
        public static Row? Mine { get; private set; }
        public static bool Busy { get; private set; }
        /// <summary>True, gdy wynik czeka w kolejce na wyslanie.</summary>
        public static bool HasPendingScore => Save.GetInt(KeyPending, 0) > 0;

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
                if (!AuthenticationService.Instance.IsSignedIn)
                    if (!await WithTimeout(AuthenticationService.Instance.SignInAnonymouslyAsync())) throw new TimeoutException("sign-in timeout");
                State = Status.Ready;
                Message = "";
                Changed();
                await SyncName();
                await FlushPending();
                await RefreshInternal();
            }
            catch (Exception e)
            {
                State = Status.Error;
                Message = e.GetType().Name + ": " + e.Message;
                Debug.Log("[online] " + e.Message);
                Changed();
            }
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
            if (State == Status.Ready) { _ = SyncName(); }
        }

        private static async Task SyncName()
        {
            try
            {
                string name = PlayerName;
                if (name.Length == 0) return;
                string current = AuthenticationService.Instance.PlayerName ?? "";
                int hash = current.IndexOf('#');
                if (hash >= 0) current = current.Substring(0, hash);
                if (current != name) await WithTimeout(AuthenticationService.Instance.UpdatePlayerNameAsync(name));
                await RefreshInternal();
            }
            catch (Exception e) { Debug.Log("[online] name: " + e.Message); }
        }

        /// <summary>Wynik po zgonie/wygranej: zapamietany od razu, wysylany gdy siec dziala (zachowany najlepszy).</summary>
        public static void Submit(int score)
        {
            if (score <= 0 || score > MaxSaneScore) return;
            if (Save.GetInt(KeyPending, 0) < score)
            {
                Save.SetInt(KeyPending, score);
                Save.Flush();
            }
            if (State == Status.Off || State == Status.Error) Init();
            else if (State == Status.Ready) _ = FlushAndRefresh();
        }

        private static async Task FlushAndRefresh()
        {
            await FlushPending();
            await RefreshInternal();
        }

        private static async Task FlushPending()
        {
            int pending = Save.GetInt(KeyPending, 0);
            if (pending <= 0 || State != Status.Ready) return;
            try
            {
                if (await WithTimeout(LeaderboardsService.Instance.AddPlayerScoreAsync(BoardId, pending)))
                {
                    Save.SetInt(KeyPending, 0);
                    Save.Flush();
                }
            }
            catch (Exception e) { Fail("submit: " + e.Message); }
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
            Busy = true;
            Changed();
            try
            {
                var pageTask = LeaderboardsService.Instance.GetScoresAsync(BoardId, new GetScoresOptions { Limit = 10 });
                if (!await WithTimeout(pageTask)) throw new TimeoutException("scores timeout");
                var page = pageTask.Result;
                string myId = AuthenticationService.Instance.PlayerId;
                Top.Clear();
                foreach (var e in page.Results)
                    Top.Add(new Row { rank = e.Rank + 1, name = Strip(e.PlayerName), score = (int)e.Score, mine = e.PlayerId == myId });
                HasEverLoaded = true;
                Mine = null;
                foreach (var r in Top) if (r.mine) Mine = r;
                if (Mine == null)
                {
                    try
                    {
                        var meTask = LeaderboardsService.Instance.GetPlayerScoreAsync(BoardId);
                        if (!await WithTimeout(meTask)) throw new TimeoutException("player score timeout");
                        var me = meTask.Result;
                        Mine = new Row { rank = me.Rank + 1, name = Strip(me.PlayerName), score = (int)me.Score, mine = true };
                    }
                    catch (Exception) { /* brak wyniku gracza na tablicy */ }
                }
            }
            catch (Exception e) { Fail("refresh: " + e.Message); }
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
