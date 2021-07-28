using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;
using UnityEngine.UI;

public class MatchingManager : PunBehaviour
{
    private PhotonView pv;
    public GameObject AskPanel;
    public Button BackButtonobject;
    private void Awake()
    {
        BackButtonobject.interactable = true;
        AskPanel.SetActive(false);
        Setup.Setting();
        PhotonNetwork.ConnectUsingSettings("1.0");
        PhotonNetwork.isMessageQueueRunning = true;
        PhotonNetwork.playerName = PlayerPrefs.GetString("Nickname");
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown() //12초동안 매칭 시도
    {
        for (int i = 10; i > 0; i--)
        {
            yield return new WaitForSeconds(1.0f);
        }
        BackButtonobject.interactable = false;
        Debug.Log("시간 초과");
        PhotonNetwork.Disconnect();
        StopAllCoroutines();
        AskPanel.SetActive(true);
    }

    public void StartAutoBattle()
    {
        SceneManager.LoadScene("AutoBattleRoom");
    }

    public void ReturnToLobby()
    {
        SceneManager.LoadScene("BattleLobby");
    }

    public override void OnJoinedLobby()
    {  
        PhotonNetwork.JoinRandomRoom(); //생성되어있는 랜덤 방 접근 시도
    }

    public void OnPhotonRandomJoinFailed() //랜덤 방 접속 실패 -> 방 직접 생성
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(PhotonNetwork.playerName, roomOptions, TypedLobby.Default); //배틀 방 직접 생성 - 이름은 마스터 닉네임
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg) //방 생성 실패
    {
        Debug.Log("Create Room Failed = " + codeAndMsg[1]);
    }

    public override void OnJoinedRoom() //방 접속 완료
    {
        Debug.Log("배틀 방 접속 완료, 방이름 : " + PhotonNetwork.room.Name);
        StartCoroutine(Headcount());
    }

    public void OnJoinRoomFailed() //네트워크나 기타 등등...
    {
        Debug.Log("방 접속 실패");
    }

    IEnumerator Headcount() //방장이 인원 체크
    {
        if (PhotonNetwork.isMasterClient)
        {
            while (PhotonNetwork.room.PlayerCount != 2)
            {
                yield return null;
            }
            pv.RPC("GameStart", PhotonTargets.All, null); //두명이 모인 경우 게임 시작
        }
    }

    [PunRPC]
    public void GameStart() //배틀방으로 이동
    {
        SceneManager.LoadScene("BattleRoom");
    }

    public void BackButton()
    {
        StopAllCoroutines();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("BattleLobby");
    }
}
