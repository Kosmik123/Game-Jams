using System;
using UnityEditor;
using UnityEngine;

namespace PayWithPixels.Build
{
    /// <summary>
    /// Kontrola zyskow wzgledem broni (laser nie dostaje PIERCING/RICOCHET) i skalowania wiazki.
    /// Menu: Jam/Run Gain Checks. Z linii polecen (Unity zamkniete):
    /// Unity.exe -batchmode -nographics -quit -projectPath ... -executeMethod PayWithPixels.Build.GainChecks.Run
    /// </summary>
    public static class GainChecks
    {
        private static int failures;

        private static void Check(bool ok, string what)
        {
            if (!ok) { failures++; Debug.LogError("[gain checks] FAIL: " + what); }
        }

        [MenuItem("Jam/Run Gain Checks")]
        public static void Run()
        {
            failures = 0;
            var laserOnly = new[] { "PIERCING", "RICOCHET" };

            foreach (var weapon in new[] { Weapon.Pistol, Weapon.Shotgun, Weapon.Laser })
            {
                var seen = new System.Collections.Generic.HashSet<string>();
                for (int i = 0; i < 4000; i++)
                {
                    var stats = new RunStats(weapon);
                    // losowy stan runu: czesc zyskow posiadana, rozne trudnosci
                    foreach (var g in Catalog.Gains)
                        if (!g.consumable && g.OkFor(weapon) && UnityEngine.Random.value < 0.25f) stats.levels[g.name] = UnityEngine.Random.Range(1, g.maxLevel + 1);
                    stats.Recompute();
                    foreach (var o in Catalog.Roll(3, UnityEngine.Random.Range(1, 20), stats))
                        if (o.gain != null) seen.Add(o.gain.name);
                }
                foreach (var name in laserOnly)
                    Check(weapon == Weapon.Laser ? !seen.Contains(name) : seen.Contains(name),
                        weapon + (weapon == Weapon.Laser ? " must never be offered " : " should be offered ") + name);
                Check(seen.Contains("TRIPLE SHOT") && seen.Contains("FASTER SHOTS"), weapon + " should be offered TRIPLE SHOT and FASTER SHOTS");
            }

            // pula darmowego zysku z kontraktu (taki sam filtr jak w Game.ContractFreeGain)
            var laserStats = new RunStats(Weapon.Laser);
            var freePool = Catalog.Gains.FindAll(g => !g.consumable && laserStats.Level(g.name) < g.maxLevel && g.OkFor(laserStats.weapon) && (g.available == null || g.available(laserStats)));
            foreach (var g in freePool) Check(Array.IndexOf(laserOnly, g.name) < 0, "free contract gain pool for laser contains " + g.name);
            Check(freePool.Count > 0, "free contract gain pool for laser is empty");

            // szerokosc wiazki: TRIPLE SHOT x1 / x1.8 / x2.6
            float[] expected = { 1f, 1.8f, 2.6f };
            for (int lv = 0; lv <= 2; lv++)
            {
                var s = new RunStats(Weapon.Laser);
                if (lv > 0) s.levels["TRIPLE SHOT"] = lv;
                s.Recompute();
                Check(Mathf.Approximately(s.BeamWidthMul, expected[lv]), "BeamWidthMul at TRIPLE SHOT " + lv + " = " + s.BeamWidthMul);
            }

            // FASTER SHOTS: 3 stacki = 1.5^3 szybciej, tick wiazki 0.1 / mnoznik
            var fast = new RunStats(Weapon.Laser);
            fast.levels["FASTER SHOTS"] = 3;
            fast.Recompute();
            Check(Mathf.Approximately(fast.fireRateMul, 3.375f), "laser FASTER SHOTS x3 fireRateMul = " + fast.fireRateMul);

            // inne bronie nie traca na zmianach: strzelba nadal 5 srutow, pistolet 1 pocisk
            Check(new RunStats(Weapon.Shotgun).shotCount == 5 && new RunStats(Weapon.Pistol).shotCount == 1, "base shotCount");

            if (failures == 0) Debug.Log("[gain checks] ALL OK");
            else Debug.LogError("[gain checks] " + failures + " failure(s)");
            if (Application.isBatchMode) EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
