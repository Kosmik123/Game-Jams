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

        private static void Near(float actual, float expected, string what)
        {
            Check(Mathf.Abs(actual - expected) < 0.0005f, what + ": expected " + expected + ", got " + actual);
        }

        /// <summary>Wartosci domyslne GameConfig musza dawac dokladnie te liczby, ktore byly wpisane w kodzie przed wprowadzeniem configu.</summary>
        private static void CheckConfigDefaults()
        {
            var pistol = new RunStats(Weapon.Pistol);
            Check(pistol.maxHp == 6 && pistol.shotCount == 1, "pistol base hp/shots");
            Near(pistol.FireInterval, 0.35f, "pistol fire interval");
            Near(pistol.MoveSpeed, 5f, "move speed");
            Near(pistol.CritChance, 0.08f, "pistol crit chance");
            Near(pistol.magnetRange, 1.8f, "magnet base");
            Near(pistol.BulletSpeed, 12f, "pistol bullet speed");
            Near(pistol.BulletLife, 2.5f, "pistol bullet life");
            Near(pistol.SpreadStep, 12f, "pistol spread");

            var shotgun = new RunStats(Weapon.Shotgun);
            Check(shotgun.shotCount == 5, "shotgun shots");
            Near(shotgun.fireRateMul, 0.6f, "shotgun fire rate");
            Near(shotgun.SpreadStep, 7.5f, "shotgun spread step");
            Near(shotgun.ExplosionRadius, 1.375f, "shotgun explosion radius");
            Near(shotgun.BulletLife, 0.55f, "shotgun bullet life");

            var laser = new RunStats(Weapon.Laser);
            Near(laser.BulletSpeed, 24f, "laser bullet speed");
            Near(laser.BulletLife, 1.4f, "laser bullet life");
            Near(laser.CritChance, 0f, "laser crit chance");

            var s = new RunStats(Weapon.Pistol);
            float[] fog = { 0f, 9f, 6.2f, 3.8f, 3.2f };
            for (int n = 0; n < fog.Length; n++) { s.fogStacks = n; Near(s.FogRadius, fog[n], "fog radius " + n); }
            float[] legs = { 5f, 4.6f, 3.8f, 2.9f };
            for (int n = 0; n < legs.Length; n++) { s.legsStacks = n; Near(s.MoveSpeed, legs[n], "move speed legs " + n); }
            float[] dash = { 1.2f, 1.2f, 0.95f, 0.7f };
            for (int n = 0; n < dash.Length; n++) { s.dashLevel = n; Near(s.DashCooldown, dash[n], "dash cooldown " + n); }
            int[] vamp = { 12, 12, 8, 5 };
            for (int n = 0; n < vamp.Length; n++) { s.vampire = n; Check(s.VampireKills == vamp[n], "vampire kills " + n); }
            float[] aura = { 0.75f, 0.75f, 0.62f, 0.5f };
            for (int n = 0; n < aura.Length; n++) { s.slowAura = n; Near(s.SlowAuraFactor, aura[n], "slow aura " + n); }
            s.inertiaStacks = 2; Near(s.InertiaTime, 0.22f, "inertia 2");
            s.inputDelayStacks = 3; Near(s.InputDelay, 0.5f, "input delay 3");
            s.zoomStacks = 2; Near(s.CameraZoom, 0.76f, "zoom 2");
            s.hungerStacks = 1; Near(s.CoinLifetime, 10f, "hunger 1");
            s.flickerStacks = 3; Near(s.FlickerDuration, 2.8f, "flicker 3");
            s.crit = 2; Near(s.CritChance, 2 * 0.12f + 0.06f + 0.08f, "crit chance lv2");
            s.fogStacks = 1; s.tripleCrit = 1; Near(s.CritMultiplier, 7f, "crit multiplier fog + triple crit");
            s.explosion = 3; Check(s.ExplosionDamage == 2, "explosion damage lv3"); Near(s.ExplosionRadius, 2.3f, "explosion radius lv3");
            s.debtWeapon = 2; Check(s.DebtDamageBonus(30) == 2 && s.DebtDamageBonus(5) == 1 && s.DebtDamageBonus(0) == 0, "debt weapon");
            s.shockwave = 2; Near(s.ShockwaveRadius, 3.4f, "shockwave radius lv2"); Check(s.ShockwaveDamage == 2, "shockwave damage lv2");
            s.pixelStacks = 1; s.droneRate = 1; Near(s.DroneInterval, 0.75f * 0.75f, "drone interval pixelation + rate"); Check(s.DroneDamage == 2, "drone damage pixelation");
            s.freeze = 2; Near(s.FreezeDuration, 3.5f, "freeze duration lv2");

            var stats = new RunStats(Weapon.Pistol);
            var fasterShots = Catalog.Gains.Find(g => g.name == "FASTER SHOTS");
            Check(Catalog.GainCost(fasterShots, 1, 1f, stats) == 6, "gain cost FASTER SHOTS d=1");
            Check(Catalog.GainCost(fasterShots, 1, 17f, stats) == 16, "gain cost FASTER SHOTS d=17: " + Catalog.GainCost(fasterShots, 1, 17f, stats));
            var fogPrice = Catalog.Prices.Find(p => p.name == "FOG");
            Check(fogPrice.severity == 2 && Catalog.Prices.Find(p => p.name == "REVERSED CONTROLS").severity == 4 && Catalog.Prices.Find(p => p.name == "FLICKER").severity == 0, "price severities");
            Near(Catalog.PriceDiscount(fogPrice), 0.8f, "fog discount");
            for (int i = 0; i < 50; i++) { int r = Catalog.SellReward(1f, fogPrice); Check(r >= 6 && r <= 8, "sell reward d=1 sev2: " + r); }

            var cfg = GameConfig.Instance;
            Check(cfg.run.wavesPerZone == 5 && cfg.run.maxDebt == 20 && Mathf.Approximately(cfg.run.difficultyPerPurchase, 0.15f), "run config");
            Check(cfg.GetEnemy(EnemyType.Chaser) != null && cfg.GetEnemy(EnemyType.Collector) != null && cfg.GetWeapon(Weapon.Laser).type == Weapon.Laser, "weapon/enemy lists");
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

            CheckConfigDefaults();

            if (failures == 0) Debug.Log("[gain checks] ALL OK");
            else Debug.LogError("[gain checks] " + failures + " failure(s)");
            if (Application.isBatchMode) EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
    }
}
