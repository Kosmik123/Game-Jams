using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : PlayerController
{
    public float minimumThinkingTime, maximumThinkingTime;


    void Start()
    {
        playerType = PlayerType.AI;
        GenerateCardsArray();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
