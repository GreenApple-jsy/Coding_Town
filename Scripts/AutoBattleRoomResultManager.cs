using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Amazon.DynamoDBv2.DataModel;

public class AutoBattleRoomResultManager : MonoBehaviour
{
    // 돈     ->   승리(점수 * 20),   무승부 (점수 * 15),     패배(점수 * 10)
    // 경험치 ->   승리(점수 * 0.3),  무승부 (점수 * 0.2),    패배(점수 * 0.1)
    //경험치 구간 단위는 일단 무조건 100으로 설정해둠. 예) exp 1720일 경우, 17레벨의 20퍼채움
    public Text PlayerNameText;
    public Text RobotNameText;
    public Text PlayerScoreText;
    public Text RobotScoreText;
    public Text PlayerCoinText;
    public int Playerexp;
    public int PlayerLevel;
    public int PlayerCoin;
    public Text PlayerLevelText;
    public Slider PlayerexpSlider;
    public GameObject Playercrown;
    public GameObject Robotcrown;
    public Button OKbutton;
    public GameObject WinEffect;
    public GameObject LoseEffect;
    public GameObject WinTextObject;
    public GameObject LoseTextObject;
    public GameObject LevelUpPanel;
    public Text LevelUptext;

    private void Awake()
    {
        WinEffect.SetActive(false);
        LoseEffect.SetActive(false);
        PlayerCoin = PlayerPrefs.GetInt("Coin", 0);
        OKbutton.interactable = false;
        Playercrown.SetActive(false);
        Robotcrown.SetActive(false);
        PlayerNameText.text = PlayerPrefs.GetString("Nickname");
        RobotNameText.text = "코딩 로봇";
        Playerexp = PlayerPrefs.GetInt("Exp", 0);
        PlayerLevel = Playerexp / 100;
        PlayerLevelText.text = "Lv" + PlayerLevel.ToString();
        PlayerexpSlider.value = Playerexp % 100;
        WinEffect.SetActive(false);
        LoseEffect.SetActive(false);
        WinTextObject.SetActive(false);
        LoseTextObject.SetActive(false);
        LevelUpPanel.SetActive(false);
    }

    private void ResultReflection() //자신의 게임 결과(돈, 경험치) 내부 및 외부 디비에 반영하기
    {
        int exp;
        int money;
        int Playerscore = AutoBattleRoomScoreManager.PlayerScore;
        int Robotscore = AutoBattleRoomScoreManager.RobotScore;
        if (Playerscore > Robotscore) //플레이어 승리
        {
            StartCoroutine(WinTextEffect());
            WinEffect.SetActive(true);
            exp = Playerexp + (int)(0.3f * Playerscore);
            money = PlayerCoin + (20 * Playerscore);
        }
        else if (Playerscore < Robotscore) //패배
        {
            StartCoroutine(LoseTextEffect());
            LoseEffect.SetActive(true);
            exp = Playerexp + (int)(0.1f * Playerscore);
            money = PlayerCoin + (10 * Playerscore);
        }
        else //무승부
        {
            exp = Playerexp + (int)(0.2f * Playerscore);
            money = PlayerCoin + (15 * Playerscore);
        }
        PlayerPrefs.SetInt("Exp", exp);
        PlayerPrefs.SetInt("Coin", money);
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
            own_coin = money,
            eq_acc = PlayerPrefs.GetInt("Acc", 0),
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

    public IEnumerator WinTextEffect()
    {
        WinTextObject.SetActive(true);
        for (float i = 0; i <= 1f; i += 0.1f)
        {
            WinTextObject.transform.localScale = new Vector2(i, i);
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(3.0f);
        WinTextObject.SetActive(false);
    }

    public IEnumerator LoseTextEffect()
    {
        LoseTextObject.SetActive(true);
        for (float i = 0; i <= 1f; i += 0.1f)
        {
            LoseTextObject.transform.localScale = new Vector2(i, i);
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(3.0f);
        LoseTextObject.SetActive(false);
    }

    public IEnumerator LevelUpEffect(int level)
    {
        LevelUptext.text = level.ToString();
        LevelUpPanel.SetActive(true);
        yield return new WaitForSeconds(3.7f);
        LevelUpPanel.SetActive(false);
    }

    public IEnumerator ShowScore()
    {
        ResultReflection();
        int PlayerScore = AutoBattleRoomScoreManager.PlayerScore;
        int RobotScore = AutoBattleRoomScoreManager.RobotScore;
        if (PlayerScore > RobotScore) //플레이어1 이김
        {
            for (int i = 0; i <= RobotScore; i++)
            {
                PlayerScoreText.text = i.ToString();
                RobotScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = RobotScore; i <= PlayerScore; i++)
            {
                PlayerScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            Playercrown.SetActive(true);
            for (float j = 1.8f; j >= 1f; j -= 0.05f)
            {
                Playercrown.transform.localScale = new Vector2(j, j);
                yield return 0.5f;
            }
            StartCoroutine(Money(1, PlayerScore));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EXP(1, PlayerScore));
        }
        else if (PlayerScore < RobotScore) //플레이어2 이김
        {
            for (int i = 0; i <= PlayerScore; i++)
            {
                PlayerScoreText.text = i.ToString();
                RobotScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = PlayerScore; i <= RobotScore; i++)
            {
                RobotScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            Robotcrown.SetActive(true);
            for (float j = 1.8f; j >= 1f; j -= 0.05f)
            {
                Robotcrown.transform.localScale = new Vector2(j, j);
                yield return 0.5f;
            }
            StartCoroutine(Money(2, PlayerScore));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EXP(2, PlayerScore));
        }
        else // 동점
        {
            for (int i = 0; i <= PlayerScore; i++)
            {
                PlayerScoreText.text = i.ToString();
                RobotScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(Money(3, PlayerScore));
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(EXP(3, PlayerScore));
        }
    }

    public IEnumerator Money(int result, int score) //플레이어의 돈 변화.  result  1:승리, 2:패배, 3:무승부
    {
        if (result == 1) //승리 (돈 두배)
        {
            int val = 20 * score;
            for (int i = 0; i <= val; i += val / 20)
            {
                PlayerCoinText.text = i.ToString();
                yield return 0.004f;
            }
        }
        else if (result == 2) //패배
        {
            int val = 10 * score;
            for (int i = 0; i <= val; i += val / 20)
            {
                PlayerCoinText.text = i.ToString();
                yield return 0.004f;
            }
        }
        else //무승부 (돈 1.5배)
        {
            int val = 15 * score;
            for (int i = 0; i <= val; i += val / 20)
            {
                PlayerCoinText.text = i.ToString();
                yield return 0.004f;
            }
        }
    }

    public IEnumerator EXP(int result, int score) //플레이어의 경험치 변화.   result  1:승리, 2:패배, 3:무승부
    {
        if (result == 1) //승리
        {
            for (int i = Playerexp; i <= Playerexp + (0.3f * score); i++)
            {
                PlayerexpSlider.value = i % 100;
                if ((i % 100 == 0) && (i != 0)) //레벨업
                {
                    PlayerLevel++;
                    PlayerLevelText.text = "Lv" + PlayerLevel.ToString();
                    StartCoroutine(LevelUpEffect(PlayerLevel));
                }
                yield return new WaitForSeconds(0.07f);
            }
        }
        else if (result == 2) //패배
        {
            for (int i = Playerexp; i <= Playerexp + (0.1f * score); i++)
            {
                PlayerexpSlider.value = i % 100;
                if ((i % 100 == 0) && (i != 0)) //레벨업
                {
                    PlayerLevel++;
                    PlayerLevelText.text = "Lv" + PlayerLevel.ToString();
                    StartCoroutine(LevelUpEffect(PlayerLevel));
                }
                yield return new WaitForSeconds(0.07f);
            }
        }
        else //무승부
        {
            for (int i = Playerexp; i <= Playerexp + (0.2f * score); i++)
            {
                PlayerexpSlider.value = i % 100;
                if ((i % 100 == 0) && (i != 0)) //레벨업
                {
                    PlayerLevel++;
                    PlayerLevelText.text = "Lv" + PlayerLevel.ToString();
                    StartCoroutine(LevelUpEffect(PlayerLevel));
                }
                yield return new WaitForSeconds(0.07f);
            }
        }
    }

    public void OKButtonClick()
    {
        SceneManager.LoadScene("BattleLobby");
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
