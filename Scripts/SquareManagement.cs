using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;
using UnityEngine.UI;

public class SquareManagement : PunBehaviour{
    public Text Nick;
    public Text Exp;
    public Text Money;
    public Slider expSlider;
    public Image UserImage;
    public GameObject Camera;
    public GameObject LeftMovementKeys;
    public GameObject RightMovementKeys;
    public static GameObject Me;
    
    private void Awake()
    {
        if (PlayerPrefs.GetInt("KeySelection",1) == 1) // 오른쪽
        {
            RightMovementKeys.SetActive(true);
            LeftMovementKeys.SetActive(false);
        }
        else
        {
            RightMovementKeys.SetActive(false);
            LeftMovementKeys.SetActive(true);
        }
        Camera.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        //접속중으로 처리 필요
        //동시 접속 금지 시켜야 함(해당 계정 접속 확인 필요)
        Setup.LoadingNotice.SetActive(true);
        Nick.text = PlayerPrefs.GetString("Nickname");
        Exp.text = (PlayerPrefs.GetInt("Exp") / 100).ToString();
        Money.text = PlayerPrefs.GetInt("Coin").ToString();
        expSlider.value = PlayerPrefs.GetInt("Exp") % 100;
        if (PlayerPrefs.GetInt("Character", 0) == 0)
            UserImage.sprite = Resources.Load("Store,Myroom/cat_round", typeof(Sprite)) as Sprite;
        else if (PlayerPrefs.GetInt("Character", 0) == 1)
            UserImage.sprite = Resources.Load("Store,Myroom/racoon_round", typeof(Sprite)) as Sprite;
        else if (PlayerPrefs.GetInt("Character", 0) == 2)
            UserImage.sprite = Resources.Load("Store,Myroom/squirrel_round", typeof(Sprite)) as Sprite;


        //상이 버전 처리 필요
        PhotonNetwork.ConnectUsingSettings("1.0");
        PhotonNetwork.isMessageQueueRunning = true;
        PhotonNetwork.playerName = PlayerPrefs.GetString("Nickname");
    }

    public override void OnJoinedLobby()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.JoinOrCreateRoom("Square", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom() //광장 룸 접속 완료
    {
        CreateCharacter();
    }

    public void OnJoinRoomFailed() //광장 룸 접속 실패 경우(인원이 꽉 찼거나 네트워크 문제)
    {
        Debug.Log("광장 룸 접속 실패 / 인원이 꽉 찼거나 네트워크 문제");
    }

    public void CreateCharacter()
    {
        Vector3 StartLocation = new Vector3(Random.Range(-3f, 3f), Random.Range(-2.4f, 2.4f),-0.2f);
        if (PlayerPrefs.GetInt("Character", 0) == 0)
            Me = PhotonNetwork.Instantiate("CatCharacter_square", StartLocation, Quaternion.Euler(0, 0, 0), 0);
        else if (PlayerPrefs.GetInt("Character", 0) == 1)
            Me = PhotonNetwork.Instantiate("RacoonCharacter_square", StartLocation, Quaternion.Euler(0, 0, 0), 0);
        else if (PlayerPrefs.GetInt("Character", 0) == 2)
            Me = PhotonNetwork.Instantiate("SquirrelCharacter_square", StartLocation, Quaternion.Euler(0, 0, 0), 0);
        
        CharacterMovement.Character = Me;
        SquareChatting.MyCharacter = Me;
        Camera.transform.SetParent(Me.transform);
        Camera.transform.localPosition = new Vector3(0, 0, -70);
        Setup.LoadingNotice.SetActive(false);
    }
}
