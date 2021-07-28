using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsInMain : MonoBehaviour {
    public GameObject BearChattingObject;
    public Text BearChattingText;
    public float t = 0f;
    public int index = 0;
    Vector3 InstantiatePosition = new Vector3(-2.3f,0.39f,-1f);

    private void Awake()
    {
        Setup.LoadingNotice.SetActive(true);

        if (PlayerPrefs.GetInt("Character",0) == 0)
            Instantiate(Resources.Load("main menu/CatCharacter_main"), InstantiatePosition, new Quaternion());
        else if (PlayerPrefs.GetInt("Character", 0) == 1)
            Instantiate(Resources.Load("main menu/RacoonCharacter_main"), InstantiatePosition, new Quaternion());
        else if (PlayerPrefs.GetInt("Character", 0) == 2)
            Instantiate(Resources.Load("main menu/SquirrelCharacter_main"), InstantiatePosition, new Quaternion());

        if (PlayerPrefs.GetInt("KeySelection", 1) == 1) // 오른쪽
        {
            GameObject.Find("MovementButtonsRight").SetActive(true);
            GameObject.Find("MovementButtonsLeft").SetActive(false);
        }
        else
        {
            GameObject.Find("MovementButtonsRight").SetActive(false);
            GameObject.Find("MovementButtonsLeft").SetActive(true);
        }

    }
    public void Start()
    {
        Setup.LoadingNotice.SetActive(false);
        StartCoroutine(ShowBearChatting());
    }

    private void Update()
    {
        t += Time.deltaTime;

        if (t > 7f)
        {
            t = 0f;
            StartCoroutine(ShowBearChatting());
        }
    }

    IEnumerator ShowBearChatting()
    {
        if (index == 3)
            index = 0;

        if (index == 0)
            BearChattingText.text = "훈련장에서\n너의 코딩 실력을 키워봐";
        else if (index == 1)
            BearChattingText.text = "광장에서는 원하는 친구에게\n대결을 신청할 수 있어";
        else
            BearChattingText.text = "상점에서 원하는 아이템과\n캐릭터를 구매할 수 있어";

        BearChattingObject.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        BearChattingObject.SetActive(false);
        index++;
    }

    public void MyRoomButton_click()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void SquareButton_click()
    {
        SceneManager.LoadScene("Square");
    }

    public void StoreButton_click()
    {
        SceneManager.LoadScene("Store");
    }

    public void TrainButton_click()
    {
        SceneManager.LoadScene("TrainingLobby");
    }

    public void BattleButton_click()
    {
        SceneManager.LoadScene("BattleLobby");
    }

}
