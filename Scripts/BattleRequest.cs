using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleRequest : MonoBehaviour
{
    public GameObject RequestButton; //캐릭터 오른쪽에 뜨는 대결 신청하기 버튼
    
    void Start()
    {
        RequestButton.SetActive(false);
    }

    void OnMouseDown() //캐릭터 클릭
    {
        if (this.name != PlayerPrefs.GetString("Nickname")) //상대방 캐릭터를 클릭하였을 시
        {
            if (RequestButton.activeSelf)
                RequestButton.SetActive(false);
            else
                RequestButton.SetActive(true);
        }
        else //내 캐릭터일 경우
        {

        }
    }
}
