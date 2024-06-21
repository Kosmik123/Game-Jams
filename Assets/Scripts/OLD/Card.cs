using UnityEditor;
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Rank
    {
        _2, _3, _4, _5, _6, _7, _8, _9, _10,
        Jack, Queen, King, Ace, Joker
    }

    public enum Suit
    {
        Clubs, Diamonds,
        Hearts, Spades
    }

    [Header("To Link")]
    public SpriteRenderer faceSpriteRenderer;
    public SpriteRenderer backSpriteRenderer;

    private new Collider2D collider;
    private Animator animator;

    [Header("Properties")]
    public Rank rank;
    public Suit suit;

    [Header("States")]
    public bool isOnPlayer;
    public bool isMouseOver;
    public bool isChosen;
    public bool isFaceUp;
    public bool isDragged;
    private Vector3 dragOffset;

    [Header("Movement")]
    public float movementProgress;
    public float rotationProgress;
    public float moveSpeed, rotationSpeed;
    public Vector3 targetPosition, oldPosition;
    public Quaternion targetRotation, oldRotation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        movementProgress = rotationProgress = 1f;
        targetPosition = oldPosition = transform.position;
        targetRotation = oldRotation = transform.rotation;
    }

    void OnMouseEnter()
    {
        //Debug.Log("MouseEnter");
        isMouseOver = true;
    }

    void OnMouseExit()
    {
        //Debug.Log("MouseExit");
        isMouseOver = false;
    }

    private void OnMouseDown()
    {
        //Debug.Log("MouseDown");
        if (isOnPlayer)
        {
            isDragged = true;
            dragOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            faceSpriteRenderer.sortingOrder = 32767;
        }
    }

    private void OnMouseUp()
    {
        //Debug.Log("MouseUp");
        if (GameManager.main.stackDropPoint.isMouseOverStack)
        {
            GameManager.main.TemporaryThrow();
        }
        else
        {
            isChosen = isOnPlayer && !isChosen;
            isDragged = false;
        }
    }

    private void Update()
    {
        UpdateSprite(CardSprites.list, faceSpriteRenderer);
        if(movementProgress < 1)
        {
            movementProgress += moveSpeed * Time.deltaTime;
            if (movementProgress >= 1)
            {
                movementProgress = 1;
                transform.position = targetPosition;
            }
            transform.position = Vector3.Lerp(oldPosition, targetPosition, movementProgress);
        }

        if(rotationProgress < 1)
        {
            rotationProgress += rotationSpeed * Time.deltaTime;
            if (rotationProgress >= 1)
            {
                rotationProgress = 1;
                transform.rotation = targetRotation;
            }
            transform.rotation = Quaternion.Lerp(oldRotation, targetRotation, movementProgress);
        }

        if (isOnPlayer && isDragged)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + dragOffset;
        }

    }

    public void UpdateSprite(CardSprites list, SpriteRenderer renderer)
    {
        if (isFaceUp)
        {
            SpritesArray card = list.cards[(int)rank];
            renderer.sprite = card.ofSuit[(int)suit];
        }
        else
        {
            renderer.sprite = list.backSprite;
        }
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        oldPosition = transform.position;
        targetPosition = newPosition;
        movementProgress = 0f;
    }

    public void RotateTo(Quaternion newRotation)
    {
        oldRotation = transform.rotation;
        targetRotation = newRotation;
        rotationProgress = 0f;
    }

    public void MoveToPlayer(PlayerController player)
    {
        MoveToPosition(player.CalculateCardPosition(player.Size(), player.Size() + 1));
        RotateTo(targetRotation = player.CalculateCardRotation(player.Size(), player.Size() + 1));
    }

    public void Reveal()
    {
        animator.SetTrigger("Reveal"); // Reveal
    }


    private void OnDrawGizmos()
    {
        
    }

    [CustomEditor(typeof(Card))]
    public class CardControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var controller = target as Card;
            SpriteRenderer renderer = controller.GetComponentInChildren<SpriteRenderer>();
            CardSprites list = GameObject.FindGameObjectWithTag("Card List").GetComponent<CardSprites>();
            controller.UpdateSprite(list, renderer);
        }
    }

}





