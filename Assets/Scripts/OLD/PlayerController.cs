using UnityEngine;

public class PlayerController : CardContainer
{
    public enum PlayerType
    {
        AI, Human
    }

    public PlayerType playerType;

    public float maximumCardsAngle, minimumHandAngle, maximumHandAngle, rotationMultiplier;
    public float radius, exposureSize;
    public Vector3 center;

    public bool turnEnded;
    public int turnsToWait = 0;
    public int activeCardIndex;

    public Card chosenCard;

    void Start()
    {
        GenerateCardsArray();
    }

    protected void PositionHand()
    {
        float startAngle = Mathf.Max((maximumHandAngle + minimumHandAngle) / 2 - maximumCardsAngle * (cards.Length - 1) / 2, minimumHandAngle);
        float cardsAngle = Mathf.Min((maximumHandAngle - minimumHandAngle) / (cards.Length - 1), maximumCardsAngle);

        for (int i = 0; i < cards.Length; i++)
        {
            Card card = cards[i];
            if (!card.isDragged)
            {
                float angle = startAngle + i * cardsAngle;
                float cardRadius = radius + (cards[i].isChosen ? exposureSize : 0);
                card.MoveToPosition(CalculateCardPosition(startAngle, cardsAngle, i));
                card.RotateTo(CalculateCardRotation(startAngle, cardsAngle, i));

                if (card.isMouseOver)
                    activeCardIndex = i;
                if (card.isChosen)
                    SetChosen(card);
            }
        }
    }

    public Vector3 CalculateCardPosition(int index, int cardsCount)
    {
        float startAngle = Mathf.Max((maximumHandAngle + minimumHandAngle) / 2 - maximumCardsAngle * (cardsCount - 1) / 2, minimumHandAngle);
        float cardsAngle = Mathf.Min((maximumHandAngle - minimumHandAngle) / (cardsCount - 1), maximumCardsAngle);

        return CalculateCardPosition(startAngle, cardsAngle, index);
    }

    public Vector3 CalculateCardPosition(float startAngle, float cardsAngle, int index)
    {
        float angle = startAngle + index * cardsAngle;
        float cardRadius = radius;// + (cards[index].isChosen ? exposureSize : 0);
        return center + new Vector3(Mathf.Sin(angle) * cardRadius,
            Mathf.Cos(angle) * cardRadius, 0);
    }

    public Quaternion CalculateCardRotation(int index, int cardsCount)
    {
        float startAngle = Mathf.Max((maximumHandAngle + minimumHandAngle) / 2 - maximumCardsAngle * (cardsCount - 1) / 2, minimumHandAngle);
        float cardsAngle = Mathf.Min((maximumHandAngle - minimumHandAngle) / (cardsCount - 1), maximumCardsAngle);

        return CalculateCardRotation(startAngle, cardsAngle, index);
    }

    public Quaternion CalculateCardRotation(float startAngle, float cardsAngle, int index)
    {
        float angle = startAngle + index * cardsAngle;
        return Quaternion.Euler(0, 0, rotationMultiplier * Mathf.Rad2Deg * angle);
    }

    protected void SetChosen(Card newChosenCard)
    {
        if (chosenCard != newChosenCard)
        {
            if(chosenCard != null)
                chosenCard.isChosen = false;
            chosenCard = newChosenCard;
        }
    }
}
