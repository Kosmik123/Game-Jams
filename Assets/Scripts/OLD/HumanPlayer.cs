using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : PlayerController
{
    void Start()
    {
        playerType = PlayerType.Human;

        GenerateCardsArray();
        foreach (Card card in cards)
            card.isFaceUp = true;
    }

    void Update()
    {
        PositionHand();
        for (int i = 0; i < cards.Length; i++)
        {
            if (!cards[i].isDragged && cards[i].movementProgress == 1)
                cards[i].faceSpriteRenderer.sortingOrder = -Mathf.Abs(i - activeCardIndex);
        }

        if (!turnEnded)
        {

        }
    }


}
