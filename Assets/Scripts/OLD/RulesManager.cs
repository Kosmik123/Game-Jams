using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Card;

public enum CardRuleEnum
{
    Draw, Skip, Demand, OnAll, AllOn,
    ChangeColor, Reverse, Replace
}

[Serializable]
public class CardRule : MonoBehaviour
{
    public CardRuleEnum effect;
}

public class RulesManager : MonoBehaviour
{
    // Enums
    public enum MultipleCardsOption
    {
        None, OnDemand, Always
    }

    public enum WarIncreaser
    {
        Add, Multiply, MultiplyIfNot1
    }

    public enum ShuffleType
    {
        ShuffleAtStart, RandomAtDraw
    }

    public enum DemandOption
    {
        None, OnDemandingCard, OnAnyCard
    }

    public static RulesManager main;

    [Header("Global Rules")]
    [Range(1,100)] public int numberOfDecks;
    public Rank[] ranksExcluded;
    public ShuffleType randomDrawType;
    public MultipleCardsOption multipleCardsPlacing;

    [Header("War Rules")]
    public bool keepPlacingRules;
    public bool firstDrawCanRescue;
    public WarIncreaser nextCardInWarCauses;

    [Header("Demand Rules")]
    public bool canDemandSpecialEffectCards;
    public bool canDemandNothing;
    public DemandOption respondToDemand;

    void Awake()
    {
        main = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanThrowCardOnStack(Card cardThrown, Card cardOnStack)
    {
        CardRule[] cardThrownRules = CardRules.main.GetCardRules(cardThrown);
        foreach (CardRule rule in cardThrownRules)
            if (rule.effect == CardRuleEnum.Replace)
            {
                cardThrown.rank = Rank._2;
                cardThrown.suit = Suit.Hearts;
                break;
            }

        if (cardThrown.rank == cardOnStack.rank)
            return true;
        if (cardThrown.suit == cardOnStack.suit)
            return true;

        CardRule[] cardOnStackRules = CardRules.main.GetCardRules(cardOnStack);
        if (cardThrownRules == null || cardOnStackRules == null)
            return false;

        foreach(CardRule rule in cardThrownRules)
            if (rule.effect == CardRuleEnum.OnAll)
                 return true;

        foreach (CardRule rule in cardOnStackRules)
            if (rule.effect == CardRuleEnum.AllOn)
                return true;

        return false;
    }
}

[CustomEditor(typeof(RulesManager)), CanEditMultipleObjects]
public class RulesManagerEditor : Editor
{

}
