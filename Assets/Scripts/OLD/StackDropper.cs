using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackDropper : MonoBehaviour
{
    public bool isMouseOverStack;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter()
    {
        isMouseOverStack = true;
    }

    void OnMouseExit()
    {
        isMouseOverStack = false;
    }


}
