using System;
using UnityEngine;

[Serializable]
public class RuleArray
{
    public GameObject[] ofSuit;
}

public class CardRules : MonoBehaviour
{
    public static CardRules main;
    public RuleArray[] rules;

    private void Awake()
    {
        main = this;
    }

    public CardRule[] GetCardRules(Card.Rank rank, Card.Suit suit)
    {
        GameObject cardTemplate = rules[(int)rank].ofSuit[(int)suit];
        return cardTemplate.GetComponents<CardRule>();
    }

    public CardRule[] GetCardRules(Card card)
    {
        return GetCardRules(card.rank, card.suit);
    }

}
