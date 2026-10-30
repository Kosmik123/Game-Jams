using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Kontrakt: nagroda od razu ("now"), "maly druk" ("later") ujawnia sie dopiero po podpisaniu i wchodzi w zycie po FineWaves falach.
    /// Kazdy los sklada kontrakt z niezaleznie wylosowanej nagrody (nazwa kontraktu = nazwa nagrody) i kary.
    /// </summary>
    public class Contract
    {
        public string title, reward, finePrint;
        public int value;            // "wartosc" w monetach do statystyki Total price paid (z nagrody)
        public Action<Game> now;     // nagroda przy podpisaniu
        public Action<Game> later;   // maly druk
    }

    public class ContractReward
    {
        public string title, text;
        public int value;
        public Action<Game> apply;
    }

    public class ContractPenalty
    {
        public string text;
        public Action<Game> apply;
    }

    public static class Contracts
    {
        public const int FineWaves = 2;

        private static ContractReward R(string title, string text, int value, Action<Game> apply) => 
            new ContractReward { title = title, text = text, value = value, apply = apply };
        
        private static ContractPenalty P(string text, Action<Game> apply) => 
            new ContractPenalty { text = text, apply = apply };

        public static readonly List<ContractReward> Rewards = new List<ContractReward>
        {
            R("ADVANCE", "+30 COINS", 30, g => g.coins += 30),
            R("FAT WALLET", "+1 MAX HP", 20, g => g.ContractMaxHp(1)),
            R("GOLD RUSH", "DOUBLE COINS FOR 2 WAVES", 25, g => g.ContractCoinMul(2f, 2)),
            R("CHECKUP", "FULL HEAL", 20, g => g.ContractHeal(0)),
            R("REGULAR", "FREE REROLLS THIS ZONE", 20, g => g.ContractFreeRerolls()),
            R("OVERCLOCK", "+25% FIRE RATE FOR 3 WAVES", 25, g => g.ContractFireBoost(1.25f, 3)),
            R("BOUNTY", "ENEMIES DROP +1 COIN FOR 2 WAVES", 25, g => g.ContractBonusDrop(2)),
            R("BODYGUARD", "GAIN A RANDOM SHIELD", 25, g => g.ContractShield()),
            R("LOAN SHARK", "+45 COINS", 40, g => g.coins += 45),
            R("FREE SAMPLE", "A RANDOM GAIN FOR FREE", 30, g => g.ContractFreeGain()),
            R("QUICK HANDS", "+30% FIRE RATE FOR 2 WAVES", 25, g => g.ContractFireBoost(1.3f, 2)),
            R("LUCKY STREAK", "ENEMIES DROP +1 COIN FOR 3 WAVES", 25, g => g.ContractBonusDrop(3)),
            R("HEALTHY LIFE", "FULL HEAL AND +1 BONUS HP", 25, g => g.ContractHeal(1)),
            R("QUIET MONEY", "+25 COINS", 20, g => g.coins += 25),
        };

        public static readonly List<ContractPenalty> Penalties = new List<ContractPenalty>
        {
            P("LOSE A RANDOM GAIN", g => g.ContractLoseGain()),
            P("FOG +2 STACKS TILL ZONE END", g => g.ContractTempPrice("FOG", 2, 0, true)),
            P("NEXT BOSS +20% HP", g => g.ContractBossBuff()),
            P("ALL SHOP PRICES +30% NEXT SHOP", g => g.ContractNextShopMarkup(1.3f)),
            P("HEAVY LEGS +2 STACKS FOR 3 WAVES", g => g.ContractTempPrice("HEAVY LEGS", 2, 3, false)),
            P("ENEMIES +20% SPEED NEXT 2 WAVES", g => g.ContractEnemySpeed(1.2f, 2)),
            P("PIXELATION +1 STACK PERMANENT", g => g.ContractAddPrice("PIXELATION")),
            P("DEBT +12", g => g.debt += 12),
            P("PRICEY SHOP +1 STACK FOR 3 WAVES", g => g.ContractTempPrice("PRICEY SHOP", 1, 3, false)),
            P("REVERSED CONTROLS +1 FOR 2 WAVES", g => g.ContractTempPrice("REVERSED CONTROLS", 1, 2, false)),
            P("FLICKER +1 FOR 3 WAVES", g => g.ContractTempPrice("FLICKER", 1, 3, false)),
            P("SLUGGISH AIM +1 STACK PERMANENT", g => g.ContractAddPrice("SLUGGISH AIM")),
            P("SILENCE FOR 3 WAVES", g => g.ContractTempPrice("SILENCE", 1, 3, false)),
        };

        /// <summary>Nagroda i kara losowane osobno.</summary>
        public static Contract Roll()
        {
            var r = Rewards[UnityEngine.Random.Range(0, Rewards.Count)];
            var p = Penalties[UnityEngine.Random.Range(0, Penalties.Count)];
            return new Contract { title = r.title, reward = r.text, finePrint = p.text, value = r.value, now = r.apply, later = p.apply };
        }
    }

    public class ActiveContract
    {
        public Contract contract;
        public int wavesLeft;
    }
}
