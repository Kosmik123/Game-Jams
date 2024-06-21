using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SpritesArray 
{
    public Sprite[] ofSuit;
}

public class CardSprites : MonoBehaviour
{
    public static CardSprites list;
    public SpritesArray[] cards;
    public Sprite backSprite;


    void Start()
    {
        list = this;
    }
}
