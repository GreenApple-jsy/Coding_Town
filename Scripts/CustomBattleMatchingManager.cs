using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;

public class CustomBattleMatchingManager : PunBehaviour
{
    private PhotonView pv;
    private void Start()
    {
        pv = this.GetComponent<PhotonView>();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;
        if (SquareBattleRequestManager.RoomName == null)
            PhotonNetwork.JoinOrCreateRoom(SquareBattleRequestManager.MyName, roomOptions, TypedLobby.Default);
        else
            PhotonNetwork.JoinOrCreateRoom(SquareBattleRequestManager.RoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.room.Name);
        StartCoroutine(Headcount());
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
        SquareBattleRequestManager.RoomName = null;
        Destroy(GameObject.Find("SquareBattleRequestManager"));
        SceneManager.LoadScene("BattleRoom");
    }
}
