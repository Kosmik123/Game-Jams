using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRule_SkipTurns : PersonalRequest
{
    void Start()
    {
        effect = CardRuleEnum.Skip;
    }
}
