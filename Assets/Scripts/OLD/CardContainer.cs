using UnityEditor;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    public Card[] cards;
 
    // Start is called before the first frame update
    void Start()
    {
        GenerateCardsArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int Size()
    {
        return cards.Length;
    }

    public void GenerateCardsArray()
    {
        cards = GetComponentsInChildren<Card>();
        if(cards != null)
            for (int i = 0; i < cards.Length; i++)
                if(!cards[i].isOnPlayer)
                    cards[i].faceSpriteRenderer.sortingOrder = cards.Length - i;
    }

    public void ShuffleCards(int shuffleCount)
    {
        if (cards != null)
            for (int i = 0; i < shuffleCount; i++)
                cards[Random.Range(0, cards.Length)].transform.SetSiblingIndex(Random.Range(0, cards.Length));
    }

    public void ShuffleCards()
    {
        ShuffleCards(cards.Length);
    }


    public void OrderCards()
    {
        GenerateCardsArray();

        int siblingIndex = 0;
        for (Card.Rank r = Card.Rank._2; r <= Card.Rank.Joker; r++)
        {
            for (Card.Suit s = Card.Suit.Clubs; s <= Card.Suit.Spades; s++)
            {
                foreach(Card card in cards)
                {
                    if(card.rank == r && card.suit == s)
                    {
                        card.transform.SetSiblingIndex(siblingIndex);
                        siblingIndex++;
                    }
                }
            }
        }
    }
#if UNITY_EDITOR


    private void OnDrawGizmosSelected()
    {

    }

    [CustomEditor(typeof(CardContainer))]
    public class CardContainerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CardContainer container = target as CardContainer;
            DrawDefaultInspector();

            if (GUILayout.Button("Shuffle"))
            {
                container.ShuffleCards();
            }
        }
    }
#endif



}
