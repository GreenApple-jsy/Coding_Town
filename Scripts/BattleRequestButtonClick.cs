using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleRequestButtonClick : MonoBehaviour
{
    

    void OnMouseDown()
    {
        SquareBattleRequestManager.ToWho = this.GetComponentInParent<PhotonView>().ownerId;
        var d = GameObject.Find("SquareBattleRequestManager").GetComponent<SquareBattleRequestManager>();
        d.ReallyRequestClick(transform.parent.name);
        this.gameObject.SetActive(false);
    }
}
