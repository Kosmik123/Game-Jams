using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Bot do balansu: gra pelne runy "ludzko" (unika, strzela w najblizszego, zbiera monety) z ograniczeniami
/// wynikajacymi z cen (mgla/zoom ograniczaja widocznosc, pikselizacja psuje celnosc, odwrocone sterowanie odwraca ruch).
/// Zapisuje telemetrie do CSV. Parametry z env: BOT_STRATEGY (random|careful|buyall), BOT_RUNS, BOT_OUT, BOT_SPEED, BOT_ASC.
/// </summary>
public class BalanceBot
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Dictionary<string, MemberInfo> cache = new Dictionary<string, MemberInfo>();

    private static Type T(string name) => Type.GetType("PayWithPixels." + name + ", Assembly-CSharp");

    private static object Get(object o, string f)
    {
        var t = o is Type tt ? tt : o.GetType();
        string key = t.FullName + "." + f;
        if (!cache.TryGetValue(key, out var m))
            cache[key] = m = (MemberInfo)t.GetField(f, All) ?? t.GetProperty(f, All);
        object target = o is Type ? null : o;
        return m is FieldInfo fi ? fi.GetValue(target) : ((PropertyInfo)m).GetValue(target);
    }

    private static void Set(object o, string f, object v) => o.GetType().GetField(f, All).SetValue(o, v);

    private static object Call(object o, string m, params object[] a)
    {
        var t = o is Type tt ? tt : o.GetType();
        string key = t.FullName + "()" + m;
        if (!cache.TryGetValue(key, out var mi)) cache[key] = mi = t.GetMethod(m, All);
        return ((MethodInfo)mi).Invoke(o is Type ? null : o, a);
    }

    private static string Env(string name, string def) => Environment.GetEnvironmentVariable(name) is string v && v.Length > 0 ? v : def;

    private struct Obj { public Vector2 pos; public float radius; public Component comp; }

    [UnityTest, Timeout(100000000)]
    public IEnumerator Bot()
    {
        string strategy = Env("BOT_STRATEGY", "careful");
        int runs = int.Parse(Env("BOT_RUNS", "3"));
        string outPath = Env("BOT_OUT", Path.Combine(Path.GetTempPath(), "bot_" + strategy + ".csv"));
        float speed = float.Parse(Env("BOT_SPEED", "8"), System.Globalization.CultureInfo.InvariantCulture);
        int asc = int.Parse(Env("BOT_ASC", "0"));

        if (!File.Exists(outPath))
            File.WriteAllText(outPath, "strategy,run,event,zone,waveInZone,wave,simTime,hp,maxHp,coins,debt,priceStacks,gainLevels,buys,weapon\n");

        yield return EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Game/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;
        if (GameObject.Find("Game") == null) T("Game").GetMethod("Create", All).Invoke(null, null);
        yield return null;
        var game = GameObject.Find("Game").GetComponent(T("Game"));
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        T("Game").GetField("DebugTimeScale", All).SetValue(null, speed);

        var metaT = T("Meta");
        var rng = new System.Random(Environment.TickCount);

        for (int run = 1; run <= runs; run++)
        {
            // ascension ustawiany tylko w tescie (omija odblokowanie)
            metaT.GetField("ascMax", All).SetValue(null, 5);
            metaT.GetField("ascSel", All).SetValue(null, asc);
            Call(game, "StartRun");
            // wymuszone ceny na starcie, np. BOT_FORCE="FOG=3;ZOOM IN=3" (test czytelnosci)
            foreach (var part in Env("BOT_FORCE", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=');
                Call(game, "ApplyStartPrice", kv[0], int.Parse(kv[1]));
            }
            Call(Get(game, "stats"), "Recompute");
            Call(game, "ApplyPrices");
            float simTime = 0;
            int buys = 0;
            string lastState = "";
            float fireTimer = 0;
            int shopSteps = 0;
            string result = "timeout";

            for (int frame = 0; frame < 4000000; frame++)
            {
                yield return null;
                float dt = Time.deltaTime;
                string state = Get(game, "state").ToString();
                if (state == "Playing" || state == "Clearing" || state == "Shop" || state == "ZoneIntro") simTime += dt;

                if (state != lastState)
                {
                    if (state == "Shop") { shopSteps = 0; Row(outPath, strategy, run, "wave_end", game, simTime, buys); }
                    lastState = state;
                }
                if (state == "Dead") { result = "death"; break; }
                if (state == "Won") { result = "win"; break; }
                if (simTime > 2700f) { result = "timeout"; break; }

                var stats = Get(game, "stats");
                var player = Get(game, "player") as Component;
                if (player == null) continue;

                if (state == "Playing" || state == "Clearing")
                {
                    BotMove(game, stats, player, dt, rng);
                    fireTimer -= dt;
                    if (state == "Playing" && fireTimer <= 0 && BotShoot(game, stats, player, rng))
                        fireTimer = (float)Get(stats, "FireInterval");
                }
                else if (state == "Shop")
                {
                    shopSteps++;
                    if (shopSteps % 4 == 0) buys += ShopStep(strategy, game, stats, player, rng);
                    if (shopSteps > 400) Call(game, "CloseShop");
                }
            }
            Row(outPath, strategy, run, result, game, simTime, buys);
            Debug.Log($"[bot] {strategy} run {run}/{runs}: {result} zone={Get(game, "zone")} wave={Get(game, "wave")} time={simTime:F0}s");
            yield return null;
        }
    }

    private static void Row(string path, string strategy, int run, string ev, object game, float simTime, int buys)
    {
        var stats = Get(game, "stats");
        var player = Get(game, "player");
        int hp = player != null ? (int)Get(player, "hp") : 0;
        int priceStacks = 0, gainLevels = 0;
        foreach (var kv in (System.Collections.IDictionary)Get(stats, "priceCounts")) priceStacks += (int)((DictionaryEntry)kv).Value;
        foreach (var kv in (System.Collections.IDictionary)Get(stats, "levels")) gainLevels += (int)((DictionaryEntry)kv).Value;
        File.AppendAllText(path, string.Join(",", strategy, run, ev, Get(game, "zone"), Get(game, "waveInZone"), Get(game, "wave"),
            simTime.ToString("F0"), hp, Get(stats, "maxHp"), Get(game, "coins"), Get(game, "debt"), priceStacks, gainLevels, buys, Get(stats, "weapon")) + "\n");
    }

    // ---------- percepcja ograniczona przez ceny ----------
    private static List<Obj> Visible(IEnumerable list, Vector2 from, float range, bool enemies)
    {
        var res = new List<Obj>();
        foreach (Component c in list)
        {
            if (c == null) continue;
            Vector2 p = c.transform.position;
            if ((p - from).sqrMagnitude > range * range) continue;
            res.Add(new Obj { pos = p, comp = c, radius = enemies ? (float)Get(c, "radius") : 0.15f });
        }
        return res;
    }

    private static float SightRange(object stats)
    {
        float fog = (float)Get(stats, "FogRadius");
        float zoom = (float)Get(stats, "CameraZoom");
        float r = 9.5f * zoom;
        return fog > 0 ? Mathf.Min(r, fog) : r;
    }

    private static void BotMove(object game, object stats, Component player, float dt, System.Random rng)
    {
        Vector2 pos = player.transform.position;
        float sight = SightRange(stats);
        var enemies = Visible((IEnumerable)T("Enemy").GetField("All", All).GetValue(null), pos, sight, true);
        var bullets = Visible((IEnumerable)T("Bullet").GetField("All", All).GetValue(null), pos, Mathf.Min(sight, 4f), false);
        var coins = Visible((IEnumerable)T("Coin").GetField("All", All).GetValue(null), pos, sight, false);

        Vector2 dir = Vector2.zero;
        float nearest = 99f; Vector2 nearestPos = pos;
        foreach (var e in enemies)
        {
            float d = Vector2.Distance(pos, e.pos);
            if (d < nearest) { nearest = d; nearestPos = e.pos; }
            float safe = 3.0f + e.radius;
            if (d < safe) dir += (pos - e.pos).normalized * (safe / Mathf.Max(d, 0.3f));
        }
        foreach (var b in bullets)
        {
            if ((bool)Get(b.comp, "fromPlayer")) continue;
            Vector2 v = (Vector2)Get(b.comp, "velocity");
            Vector2 rel = pos - b.pos;
            // sidestep prostopadle do toru, o ile pocisk leci w nasza strone
            if (Vector2.Dot(v.normalized, rel.normalized) > 0.5f)
            {
                Vector2 perp = new Vector2(-v.y, v.x).normalized;
                if (Vector2.Dot(perp, rel) < 0) perp = -perp;
                dir += perp * (2f / Mathf.Max(rel.magnitude, 0.3f));
            }
        }
        // sciany
        var half = (Vector2)T("Game").GetField("ArenaHalf", All).GetValue(null);
        if (pos.x > half.x - 1.5f) dir += Vector2.left * 1.5f;
        if (pos.x < -half.x + 1.5f) dir += Vector2.right * 1.5f;
        if (pos.y > half.y - 1.2f) dir += Vector2.down * 1.5f;
        if (pos.y < -half.y + 1.2f) dir += Vector2.up * 1.5f;
        // monety, kiedy bezpiecznie
        if (nearest > 4f && coins.Count > 0)
        {
            Obj best = coins[0]; float bd = 99;
            foreach (var c in coins) { float d = Vector2.Distance(pos, c.pos); if (d < bd) { bd = d; best = c; } }
            dir += (best.pos - pos).normalized * 1.2f;
        }
        else if (nearest < 99f && nearest >= 3f)
        {
            // krazy wokol przeciwnika
            Vector2 to = nearestPos - pos;
            dir += new Vector2(-to.y, to.x).normalized * 0.7f;
        }
        dir += -pos.normalized * 0.12f;

        if ((bool)Get(game, "ControlsInverted")) dir = -dir;   // odwrocone sterowanie: czlowiek by tez pomylil kierunek
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

        float spd = (float)Get(stats, "MoveSpeed");
        Vector2 np = pos + dir * spd * dt;
        np.x = Mathf.Clamp(np.x, -half.x, half.x);
        np.y = Mathf.Clamp(np.y, -half.y, half.y);
        player.transform.position = (Vector3)np;
    }

    private static bool BotShoot(object game, object stats, Component player, System.Random rng)
    {
        Vector2 pos = player.transform.position;
        float sight = Mathf.Min(SightRange(stats), 11f);
        var enemies = Visible((IEnumerable)T("Enemy").GetField("All", All).GetValue(null), pos, sight, true);
        if (enemies.Count == 0) return false;
        Obj best = enemies[0]; float bd = 99;
        foreach (var e in enemies) { float d = Vector2.Distance(pos, e.pos); if (d < bd) { bd = d; best = e; } }

        Vector2 dir = (best.pos - pos).normalized;
        // celnosc: baza + pogorszenie od cen (pikselizacja, ociezalosc celowania)
        float err = 4f + 2.5f * (int)Get(stats, "pixelStacks") + 4f * (int)Get(stats, "aimLagStacks");
        float ang = ((float)rng.NextDouble() * 2f - 1f) * err * Mathf.Deg2Rad;
        dir = new Vector2(dir.x * Mathf.Cos(ang) - dir.y * Mathf.Sin(ang), dir.x * Mathf.Sin(ang) + dir.y * Mathf.Cos(ang));
        Call(Get(game, "player"), "Shoot", pos, dir);
        return true;
    }

    // ---------- sklep wg strategii ----------
    private static int ShopStep(string strategy, object game, object stats, Component player, System.Random rng)
    {
        var offers = (IList)Get(game, "shopOffers");
        int coins = (int)Get(game, "coins");
        int debt = (int)Get(game, "debt");
        int hp = (int)Get(player, "hp");
        int maxHp = (int)Get(stats, "maxHp");

        var candidates = new List<object>();
        foreach (var o in offers)
            if (!(bool)Get(o, "sold") && (bool)Call(game, "CanAfford", o)) candidates.Add(o);

        if (strategy == "buyall")
        {
            if (candidates.Count > 0) { Call(game, "Buy", candidates[0]); return 1; }
            Call(game, "CloseShop");
            return 0;
        }

        if (strategy == "random")
        {
            int r = rng.Next(10);
            if (candidates.Count > 0 && r < 4) { Call(game, "Buy", candidates[rng.Next(candidates.Count)]); return 1; }
            Call(game, "CloseShop");
            return 0;
        }

        // careful: najpierw splac dlug, kupuj tylko za monety, preferuj przezycie, unikaj najgorszych cen
        if (debt > 0 && coins > 0) { Call(game, "PayDebt"); return 0; }

        object pick = null; float bestScore = 0;
        foreach (var o in candidates)
        {
            var gain = Get(o, "gain");
            var price = Get(o, "price");
            bool sell = (bool)Get(o, "sell");
            int cost = (int)Call(game, "CostOf", o);
            if (!sell && cost > coins) continue;                       // zadnego kredytu
            string gn = gain != null ? (string)Get(gain, "name") : "SELL";
            string pn = (string)Get(price, "name");

            float score;
            switch (gn)
            {
                case "MORE HEALTH": score = 6; break;
                case "SHIELD": score = 5; break;
                case "HEALING": score = hp < maxHp ? 6 : 0; break;
                case "FASTER SHOTS": score = 5; break;
                case "PIERCING": score = 4; break;
                case "TRIPLE SHOT": score = 4; break;
                case "DASH": score = 2; break;
                case "SELL": score = 0.5f; break;
                default: score = 3; break;
            }
            // cena: unikaj najbardziej bolesnych
            switch (pn)
            {
                case "FOG": case "ZOOM IN": case "REVERSED CONTROLS": case "FRAGILITY": score -= 2.5f; break;
                case "PIXELATION": case "SLUGGISH AIM": case "HUNGER": score -= 1.2f; break;
                default: score -= 0.5f; break;
            }
            if (score > bestScore) { bestScore = score; pick = o; }
        }
        if (pick != null) { Call(game, "Buy", pick); return 1; }
        Call(game, "CloseShop");
        return 0;
    }
}
