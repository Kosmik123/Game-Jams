using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager main;
    public RulesManager rulesManager;

    [Header("Card Containers")]
    public CardContainer deck;
    public CardContainer stack;
    public StackDropper stackDropPoint;

    [Header("Properties")]
    public PlayerController[] players;

    [Header("States")]
    public int currentPlayerIndex;
    public int direction = +1;
    public Card stackTopCard;

    void Awake()
    {
        main = this;
    }

    private void Start()
    {
        stackDropPoint = stack.GetComponentInChildren<StackDropper>();
        rulesManager = GetComponentInChildren<RulesManager>();
        if(rulesManager.randomDrawType == RulesManager.ShuffleType.ShuffleAtStart)
            ShuffleDeck();
    }

    // Update is called once per frame
    void Update()
    {
        if(players[currentPlayerIndex].turnEnded)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
            players[currentPlayerIndex].turnEnded = false;
        }
    }

    public void AddCardFromDeckToStack()
    {
        Card card = GetCardFromDeck();
        if (card == null)
            return;
        AddCardToStack(card);
        card.RotateTo(Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
    }


    public void AddCardToPlayer(int playerIndex)
    {
        PlayerController player = players[playerIndex];
        Card card = GetCardFromDeck();
        if (card == null)
            return;

        card.faceSpriteRenderer.sortingOrder = 1000;
        card.MoveToPlayer(player);
        AddCardToTransform(card, player.transform, true);

        card.isOnPlayer = true;
        player.OrderCards();
    }

    private Card GetCardFromDeck()
    {
        if (deck.Size() > 0)
        {
            Card card = null;
            if (rulesManager.randomDrawType == RulesManager.ShuffleType.ShuffleAtStart)
            {
                card = deck.GetComponentInChildren<Card>();
            }
            else
            {
                card = deck.cards[Random.Range(0, deck.cards.Length)];
            }
            return card;
        }
        return null;
    }


    private void AddCardToTransform(Card card, Transform transform, bool faceUp)
    {
        card.transform.parent = transform;
        //card.transform.localPosition = Vector3.zero;
        PrepareDeck();
        if(faceUp)
            card.Reveal();
    }

    public void ShuffleDeck()
    {
        deck.ShuffleCards(2 * deck.Size());
    }

    public void ShuffleStackToDeck()
    {

    }

    public void TemporaryThrow()
    {
        Card card = players[currentPlayerIndex].chosenCard;

        if (rulesManager.CanThrowCardOnStack(card, stackTopCard))
        {
            AddCardToStack(card);
            players[currentPlayerIndex].GenerateCardsArray();
        }
        else
        {
            Debug.Log("NIE MOŻNA!");
        }
    }

    public void AddCardToStack(Card card)
    {
        card.MoveToPosition(stack.transform.position);
        AddCardToTransform(card, stack.transform, true);
        stack.GenerateCardsArray();
        card.isOnPlayer = false;
        stackTopCard = card;
    }

    void PrepareDeck()
    {
        deck.GenerateCardsArray();
        if(deck.Size() > 0) 
            deck.cards[0].faceSpriteRenderer.sortingOrder = deck.Size();
    }

}
