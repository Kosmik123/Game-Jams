using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SmokeTest
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static Type T(string name) => Type.GetType("PayWithPixels." + name + ", Assembly-CSharp");
    private static object Get(object o, string f) => o.GetType().GetField(f, All)?.GetValue(o) ?? o.GetType().GetProperty(f, All)?.GetValue(o);
    private static void Set(object o, string f, object v) => o.GetType().GetField(f, All).SetValue(o, v);
    private static object Call(object o, string m, params object[] a) => o.GetType().GetMethod(m, All).Invoke(o, a);

    [UnityTest, Timeout(900000)]
    public IEnumerator PlayThroughRun()
    {
        yield return EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Game/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;
        yield return null;

        if (GameObject.Find("Game") == null) T("Game").GetMethod("Create", All).Invoke(null, null);
        yield return null;
        var gameGo = GameObject.Find("Game");
        Assert.NotNull(gameGo, "Game object not created by Bootstrap");
        var game = gameGo.GetComponent(T("Game"));
        string weaponEnv = Environment.GetEnvironmentVariable("SMOKE_WEAPON");   // 0 pistol, 1 shotgun, 2 laser
        int weaponId = string.IsNullOrEmpty(weaponEnv) ? 0 : int.Parse(weaponEnv);
        if (weaponId > 0)
        {
            T("Meta").GetMethod("Unlock", All).Invoke(null, new object[] { "boss1" });
            T("Meta").GetMethod("Unlock", All).Invoke(null, new object[] { "boss2" });
            T("Meta").GetProperty("Selected", All).SetValue(null, Enum.ToObject(T("Weapon"), weaponId));
        }
        Call(game, "StartRun");
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        T("Game").GetField("DebugTimeScale", All).SetValue(null, 5f);

        // scenariusz ucieczki: Heavy Legs x3 + rozny wrogowie w strefie 3 z maks. mnoznikami nie moga byc szybsi niz gracz
        {
            var stats = Get(game, "stats");
            Set(stats, "legsStacks", 3);
            float playerSpeed = (float)Get(stats, "MoveSpeed");
            Set(game, "zone", 3);
            var parent = ((Component)Get(game, "player")).transform.parent;
            foreach (var name in new[] { "Swarm", "Chaser", "Tank" })
            {
                var e = T("Enemy").GetMethod("Spawn", All).Invoke(null, new object[] { parent, Enum.Parse(T("EnemyType"), name), Vector2.zero, 14 });
                float es = (float)Get(e, "speed");
                Debug.Log($"[smoke] escape {name} speed={es:F2} player(legs x3)={playerSpeed:F2}");
                if (name == "Swarm") Assert.Less(es, playerSpeed, "Swarm catches player with Heavy Legs x3");
                Assert.LessOrEqual(es, 0.8f * 5f * 0.92f + 0.01f, name + " faster than 0.8x player speed");
                UnityEngine.Object.Destroy(((Component)e).gameObject);
            }
            Set(stats, "legsStacks", 0);
            Set(game, "zone", 1);
        }

        var rng = new System.Random(1);
        int lastZone = 0;
        string lastState = "";
        for (int frame = 0; frame < 60000; frame++)
        {
            yield return null;
            string state = Get(game, "state").ToString();
            int zone = (int)Get(game, "zone");
            if (state != lastState || zone != lastZone)
            {
                Debug.Log($"[smoke] frame {frame} state={state} zone={zone} wave={Get(game, "waveInZone")} coins={Get(game, "coins")} debt={Get(game, "debt")}");
                lastState = state; lastZone = zone;
            }

            if (frame % 1500 == 0 && state == "Playing")
            {
                var list = (System.Collections.IList)T("Enemy").GetField("All", All).GetValue(null);
                var sb = new System.Text.StringBuilder();
                foreach (Component e in list) sb.Append(e.name + "@" + e.transform.position.ToString("F1") + " ");
                Debug.Log($"[smoke] frame {frame} playing, enemies: {sb}");
            }

            var player = Get(game, "player");
            if (player != null) Set(player, "hp", 999);

            if (state == "Won" || state == "Dead") break;

            if (state == "Playing" && player != null && frame % 6 == 0)
            {
                // strzel w najblizszego wroga
                var playerTr = ((Component)player).transform;
                var enemies = (System.Collections.IList)T("Enemy").GetField("All", All).GetValue(null);
                Vector2 target = Vector2.zero; float best = 1e9f;
                foreach (Component e in enemies)
                {
                    float d = ((Vector2)e.transform.position - (Vector2)playerTr.position).sqrMagnitude;
                    if (d < best) { best = d; target = e.transform.position; }
                }
                Vector2 dir = best < 1e8f ? (target - (Vector2)playerTr.position).normalized : Vector2.right;
                if (weaponId == 2) Call(player, "UpdateBeam", game, (Vector2)playerTr.position, dir, true);
                else Call(player, "Shoot", (Vector2)playerTr.position, dir);
                if (frame % 120 == 0) playerTr.position = new Vector3((float)(rng.NextDouble() * 12 - 6), (float)(rng.NextDouble() * 6 - 3), 0);
            }

            if (state == "Shop")
            {
                var offers = (IList)Get(game, "shopOffers");
                // czasem reroll / haggle, potem kup losowa oferte (lub pomin)
                int choice = rng.Next(10);
                if ((bool)Get(game, "shopRedeem") && rng.Next(2) == 0) { Call(game, "RemoveRandomPriceStack"); Set(game, "shopRedeem", false); }
                else if (Get(game, "shopContract") != null && rng.Next(3) > 0) Call(game, "SignContract");
                else if (choice == 0) Call(game, "Reroll");
                else if (choice == 1) Call(game, "Haggle");
                else if (offers.Count > 0 && choice < 8)
                {
                    var o = offers[rng.Next(offers.Count)];
                    Call(game, "Buy", o);
                }
                else Call(game, "CloseShop");
            }
        }

        Debug.Log($"[smoke] END state={Get(game, "state")} zone={Get(game, "zone")} wave={Get(game, "wave")}");
        Assert.GreaterOrEqual((int)Get(game, "wave"), 3, "run did not progress");
    }
}
