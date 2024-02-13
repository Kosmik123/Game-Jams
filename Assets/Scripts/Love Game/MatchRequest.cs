using Bipolar.Match3;
using System;
using UnityEngine;

[Serializable]
public struct MatchRequest
{
    [Min(0)]
    public int requestsCount;
    public TokenType type;
    [Min(0)]
    public int size;
    [Min(0)]
    public int horizontalCount;
    [Min(0)]
    public int verticalCount;
}