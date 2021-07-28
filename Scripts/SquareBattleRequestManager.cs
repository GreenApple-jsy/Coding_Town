using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SquareBattleRequestManager : PunBehaviour
{
    public GameObject ReallyRequest; //대결을 신청하시겠습니까? 물어보는 패널
    public GameObject ShowRequestPanel; //대결 신청 받았을 때 수락할건지 물어보는 패널
    public GameObject WaitingRequestAnswerPanel;
    public Text WaitingRequestAnswerText;
    public Text ShowRequestText;
    public static string MyName;
    public static int ToWho;
    private PhotonView pv;
    public int FromWho;
    public Text ToWhoRequestText;
    public static string RoomName;

    void Start()
    {
        MyName = PlayerPrefs.GetString("Nickname");
        pv = this.GetComponent<PhotonView>();
        WaitingRequestAnswerPanel.SetActive(false);
        ShowRequestPanel.SetActive(false);
        ReallyRequest.SetActive(false);
    }

    public void ReallyRequestClick(string Who)
    {
        
        ToWhoRequestText.text = Who + "님에게\n대결을 신청하시겠습니까?";
        ReallyRequest.SetActive(true);
        
    }

    public void YesRequest()
    {
        ReallyRequest.SetActive(false);
        pv.RPC("ShowRequest",PhotonPlayer.Find(ToWho),MyName, pv.ownerId);
        StartCoroutine(WaitingForAnswer());
    }

    public void NoRequest()
    {
        ReallyRequest.SetActive(false);
    }

    IEnumerator WaitingForAnswer()
    {
        WaitingRequestAnswerText.text = "상대에게 매칭 신청 중입니다";
        WaitingRequestAnswerPanel.SetActive(true);
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1.0f);
        }
        WaitingRequestAnswerText.text = "매칭 실패...";
        yield return new WaitForSeconds(1.5f);
        WaitingRequestAnswerPanel.SetActive(false);
    }

    [PunRPC]
    public void ShowRequest(string name, int from)
    {
        FromWho = from;
        ShowRequestText.text = name + "님이 대결을 신청하셨습니다\n수락하시겠습니까?";
        ShowRequestPanel.SetActive(true);
        StartCoroutine(CountdownForAnswer());
    }

    IEnumerator CountdownForAnswer()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1.0f);
        }
        ShowRequestPanel.SetActive(false);
    }

    public void AgreeRequestClick() //받은 대결 신청 수락
    {
        pv.RPC("GoToMatchingScene", PhotonPlayer.Find(FromWho), MyName);
        DontDestroyOnLoad(this);
        SceneManager.LoadScene("CustomBattleMatching");
    }

    [PunRPC]
    public void GoToMatchingScene(string name)
    {
        StopAllCoroutines();
        RoomName = name;
        DontDestroyOnLoad(this);
        SceneManager.LoadScene("CustomBattleMatching");
    }

    public void RejectRequestClick() //받은 대결 신청 거부
    {
        ShowRequestPanel.SetActive(false);
    }

}
