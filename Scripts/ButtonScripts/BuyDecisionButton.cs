using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyDecisionButton : MonoBehaviour
{
    public int ans = -1;

    public void BuyButtonClick(int what)
    {
        ans = what;
    }
}
