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
        public Action<Game> now;     // nagroda przy podpisaniu
        public Action<Game> later;   // maly druk
    }

    public static class Contracts
    {
        public const int FineWaves = 2;

        public static readonly List<Contract> All = new List<Contract>
        {
            new Contract { title = "ADVANCE", reward = "+30 COINS NOW", finePrint = "LOSE A RANDOM GAIN",
                now = g => g.coins += 30, later = g => g.ContractLoseGain() },
            new Contract { title = "LOAN SHARK", reward = "+20 COINS NOW", finePrint = "DEBT +25",
                now = g => g.coins += 20, later = g => g.debt += 25 },
            new Contract { title = "BODY WORK", reward = "FULL HEAL +2 HP NOW", finePrint = "A RANDOM PRICE IS ADDED",
                now = g => g.ContractHeal(2), later = g => g.ContractRandomPrice(1) },
            new Contract { title = "LUCKY BREAK", reward = "A RANDOM GAIN LEVEL NOW", finePrint = "TWO RANDOM PRICES ARE ADDED",
                now = g => g.ContractFreeGain(), later = g => g.ContractRandomPrice(2) },
            new Contract { title = "DEBT RELIEF", reward = "DEBT -15 NOW", finePrint = "LOSE HALF YOUR COINS",
                now = g => g.debt = Mathf.Max(0, g.debt - 15), later = g => g.coins /= 2 },
            new Contract { title = "BLOOD MONEY", reward = "+40 COINS NOW", finePrint = "FRAGILITY: -1 MAX HP",
                now = g => g.coins += 40, later = g => g.ContractAddPrice("FRAGILITY") },
            new Contract { title = "INSURANCE", reward = "FULL HEAL NOW", finePrint = "LOSE 25 COINS OR GO INTO DEBT",
                now = g => g.ContractHeal(0), later = g => g.ContractPay(25) },
            new Contract { title = "SPONSOR", reward = "+25 COINS NOW", finePrint = "FOG: A PRICE YOU CAN SEE",
                now = g => g.coins += 25, later = g => g.ContractAddPrice("FOG") },
        };

        public static Contract Roll(Contract avoid = null)
        {
            Contract c;
            int guard = 0;
            do { c = All[UnityEngine.Random.Range(0, All.Count)]; } while (c == avoid && ++guard < 8);
            return c;
        }
    }

    public class ActiveContract
    {
        public Contract contract;
        public int wavesLeft;
    }
}
