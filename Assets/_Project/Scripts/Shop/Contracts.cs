using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Kontrakt: nagroda od razu, "maly druk" ujawnia sie dopiero po podpisaniu i wchodzi w zycie po FineWaves falach.
    /// </summary>
    public class Contract
    {
        public string title, reward, finePrint;
        public int value;            // "wartosc" w monetach do statystyki Total price paid
        public Action<Game> now;     // nagroda przy podpisaniu
        public Action<Game> later;   // maly druk
    }

    public static class Contracts
    {
        public const int FineWaves = 2;

        public static readonly List<Contract> All = new List<Contract>
        {
            new Contract { title = "ADVANCE", reward = "+30 COINS", finePrint = "LOSE A RANDOM GAIN", value = 30,
                now = g => g.coins += 30, later = g => g.ContractLoseGain() },
            new Contract { title = "FAT WALLET", reward = "+1 MAX HP", finePrint = "DEBT +10", value = 20,
                now = g => g.ContractMaxHp(1), later = g => g.debt += 10 },
            new Contract { title = "GOLD RUSH", reward = "DOUBLE COINS FOR 2 WAVES", finePrint = "FOG +2 STACKS TILL ZONE END", value = 25,
                now = g => g.ContractCoinMul(2f, 2), later = g => g.ContractTempPrice("FOG", 2, 0, true) },
            new Contract { title = "CHECKUP", reward = "FULL HEAL", finePrint = "NEXT BOSS +20% HP", value = 20,
                now = g => g.ContractHeal(0), later = g => g.ContractBossBuff() },
            new Contract { title = "REGULAR", reward = "FREE REROLLS THIS ZONE", finePrint = "ALL SHOP PRICES +30% NEXT SHOP", value = 20,
                now = g => g.ContractFreeRerolls(), later = g => g.ContractNextShopMarkup(1.3f) },
            new Contract { title = "OVERCLOCK", reward = "+25% FIRE RATE FOR 3 WAVES", finePrint = "HEAVY LEGS +2 STACKS FOR 3 WAVES", value = 25,
                now = g => g.ContractFireBoost(1.25f, 3), later = g => g.ContractTempPrice("HEAVY LEGS", 2, 3, false) },
            new Contract { title = "BOUNTY", reward = "ENEMIES DROP +1 COIN FOR 2 WAVES", finePrint = "ENEMIES +20% SPEED NEXT 2 WAVES", value = 25,
                now = g => g.ContractBonusDrop(2), later = g => g.ContractEnemySpeed(1.2f, 2) },
            new Contract { title = "BODYGUARD", reward = "GAIN A RANDOM SHIELD", finePrint = "PIXELATION +1 STACK PERMANENT", value = 25,
                now = g => g.ContractShield(), later = g => g.ContractAddPrice("PIXELATION") },
            new Contract { title = "LOAN SHARK", reward = "+45 COINS", finePrint = "DEBT +12", value = 40,
                now = g => g.coins += 45, later = g => g.debt += 12 },
            new Contract { title = "FREE SAMPLE", reward = "A RANDOM GAIN FOR FREE", finePrint = "PRICEY SHOP +1 STACK FOR 3 WAVES", value = 30,
                now = g => g.ContractFreeGain(), later = g => g.ContractTempPrice("PRICEY SHOP", 1, 3, false) },
            new Contract { title = "QUICK HANDS", reward = "+30% FIRE RATE FOR 2 WAVES", finePrint = "REVERSED CONTROLS +1 FOR 2 WAVES", value = 25,
                now = g => g.ContractFireBoost(1.3f, 2), later = g => g.ContractTempPrice("REVERSED CONTROLS", 1, 2, false) },
            new Contract { title = "LUCKY STREAK", reward = "ENEMIES DROP +1 COIN FOR 3 WAVES", finePrint = "FLICKER +1 FOR 3 WAVES", value = 25,
                now = g => g.ContractBonusDrop(3), later = g => g.ContractTempPrice("FLICKER", 1, 3, false) },
            new Contract { title = "HEALTHY LIFE", reward = "FULL HEAL AND +1 BONUS HP", finePrint = "SLUGGISH AIM +1 STACK PERMANENT", value = 25,
                now = g => g.ContractHeal(1), later = g => g.ContractAddPrice("SLUGGISH AIM") },
            new Contract { title = "QUIET MONEY", reward = "+25 COINS", finePrint = "SILENCE FOR 3 WAVES", value = 20,
                now = g => g.coins += 25, later = g => g.ContractTempPrice("SILENCE", 1, 3, false) },
        };

        public static Contract Roll()
        {
            return All[UnityEngine.Random.Range(0, All.Count)];
        }
    }

    public class ActiveContract
    {
        public Contract contract;
        public int wavesLeft;
    }
}
