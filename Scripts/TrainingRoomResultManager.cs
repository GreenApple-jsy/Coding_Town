using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon.DynamoDBv2.DataModel;

public class TrainingRoomResultManager : MonoBehaviour
{
    public Text ScoreText;
    public Text CoinText;
    public int PlayerExp;
    public int Level;
    public Text LevelText;
    public Slider expSlider;
    public Button OKbutton;

    public void Awake()
    {
        OKbutton.interactable = false;
        SendExp(PlayerPrefs.GetInt("Exp", 0));
    }

    public void SendExp(int expToChange)
    {
        expSlider.value = expToChange % 100;
        PlayerExp = expToChange;
        Level = expToChange / 100;
        LevelText.text = "Lv" + Level.ToString();

    }

    private void ResultReflection() //자신의 게임 결과(돈, 경험치) 내부 및 외부 디비에 반영하기
    {
        int exp;
        int Playerscore = TrainingRoomScoreManager.PlayerScore;

        exp = PlayerPrefs.GetInt("Exp", 0) + (int)(0.2f * Playerscore);

        PlayerPrefs.SetInt("Exp", exp);

        bool Glogin;
        string code;

        if (PlayerPrefs.GetInt("GooglePlayLogin") == 1)
        {
            Glogin = true;
            code = PlayerPrefs.GetString("GPGSidToken");
        }

        else
        {
            Glogin = false;
            code = PlayerPrefs.GetString("GuestCode");
        }

        User Me = new User //업로드 할 내용을 미리 선언해둠(예시)
        {
            user_no = PlayerPrefs.GetString("UserN"),
            GPlogin_status = Glogin,
            user_code = code,
            user_nick = PlayerPrefs.GetString("Nickname"),
            user_exp = exp,
            own_coin = PlayerPrefs.GetInt("Coin"),
            eq_acc = PlayerPrefs.GetInt("Acc"),
            eq_item1 = PlayerPrefs.GetInt("Item1", 0),
            eq_item2 = PlayerPrefs.GetInt("Item2", 0),
            present_char = PlayerPrefs.GetInt("Character"),
            login_status = true
        };
        Setup.context.SaveAsync(Me, (result) =>
        {
            if (result.Exception == null)
            {
                Debug.Log("게임 결과 반영 완료");
                OKbutton.interactable = true;
            }
            else
                Debug.Log(result.Exception);
        });
    }

    public IEnumerator ShowScore()
    {
        ResultReflection();
        int Score = TrainingRoomScoreManager.PlayerScore;
        int Coin = Score / 2;
        ScoreText.text = Score.ToString();

        for (int i = 0; i <= Coin; i++)
        {
            CoinText.text = i.ToString();
            yield return new WaitForSeconds(0.0015f);
        }
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(EXP(Score));

    }
    
    public IEnumerator EXP(int score) 
    {
        for (int i = PlayerExp; i <= PlayerExp + (0.2f * score); i++)
        {
            expSlider.value = i % 100;
            if ((i % 100 == 0) && (i != 0)) //레벨업
            {
                Level++;
                LevelText.text = "Lv" + Level.ToString();
            }
            yield return new WaitForSeconds(0.07f);
        }
    }
        

    public void OKButtonClick()
    {
        TrainingRoomManager Tr = GameObject.Find("TrainingLobbyManager").GetComponent<TrainingRoomManager>();
        TrainingLobbyManager Tm = GameObject.Find("TrainingLobbyManager").GetComponent<TrainingLobbyManager>();
        Tr.ResultPanel.SetActive(false);
        Tm.QuestionPanel.SetActive(false);
    }

    [DynamoDBTable("user_info")]
    public class User
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBProperty]
        public bool GPlogin_status { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Secondary Hash key.
        public string user_code { get; set; }
        [DynamoDBProperty]
        public string user_nick { get; set; }
        [DynamoDBProperty]
        public int user_exp { get; set; }
        [DynamoDBProperty]
        public int own_coin { get; set; }
        [DynamoDBProperty]
        public int eq_acc { get; set; }
        [DynamoDBProperty]
        public int eq_item1 { get; set; }
        [DynamoDBProperty]
        public int eq_item2 { get; set; }
        [DynamoDBProperty]
        public int present_char { get; set; }
        [DynamoDBProperty]
        public bool login_status { get; set; }
    }
}
