using System.Collections;
using UnityEngine;
using Photon;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2.DataModel;

public class BattleRoomResultManager : PunBehaviour
{
    //현재 임시 설정해둔 기획
    // 돈     ->   승리(점수 * 20),   무승부 (점수 * 15),     패배(점수 * 10)
    // 경험치 ->   승리(점수 * 0.3),  무승부 (점수 * 0.2),    패배(점수 * 0.1)
    //경험치 구간 단위는 일단 무조건 100으로 설정해둠. 예) exp 1720일 경우, 17레벨의 20퍼채움
    private PhotonView pv;
    public Text C1NameText;
    public Text C2NameText;
    public Text C1ScoreText;
    public Text C2ScoreText;
    public Text C1CoinText;
    public Text C2CoinText;
    public int C1exp;
    public static int C2exp;
    public int C1Level;
    public int C2Level;
    public Text C1LevelText;
    public Text C2LevelText;
    public Slider C1expSlider;
    public Slider C2expSlider;
    public GameObject C1crown;
    public GameObject C2crown;
    public Button OKbutton;
    public int WhoAMI;
    public GameObject WinEffect;
    public GameObject LoseEffect;
    public GameObject WinTextObject;
    public GameObject LoseTextObject;
    public GameObject LevelUpPanel;
    public Text LevelUptext;

    private void Awake()
    {
        C2exp = -1;
        OKbutton.interactable = false;
        C1crown.SetActive(false);
        C2crown.SetActive(false);
        WinEffect.SetActive(false);
        LoseEffect.SetActive(false);
        WinTextObject.SetActive(false);
        LoseTextObject.SetActive(false);
        LevelUpPanel.SetActive(false);
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.isMasterClient)
        {
            C1NameText.text = PhotonNetwork.playerList[1].NickName;
            C2NameText.text = PhotonNetwork.playerList[0].NickName;
            WhoAMI = 1;
            StartCoroutine(CheckOtherPlayer());
        }
        else
        {
            C1NameText.text = PhotonNetwork.playerList[0].NickName;
            C2NameText.text = PhotonNetwork.playerList[1].NickName;
            pv.RPC("SendExp", PhotonTargets.All, 2, PlayerPrefs.GetInt("Exp", 0)); //늦게들어온 사용자2가 자신의 exp를 알림
            WhoAMI = 2;
        }
    }

    IEnumerator CheckOtherPlayer()
    {
        while (BattleRoomManager.Player2InTheBattleRoom == false)
            yield return null;
        pv.RPC("SendExp", PhotonTargets.All, 1, PlayerPrefs.GetInt("Exp", 0));
    }

    [PunRPC]
    public void SendExp(int player, int exp)
    {
        if (player == 1)
        {
            C1expSlider.value = exp % 100;
            C1exp = exp;
            C1Level = exp / 100;
            C1LevelText.text = "Lv" + C1Level.ToString();
        }
        else
        {
            C2expSlider.value = exp % 100;
            C2exp = exp;
            C2Level = exp / 100;
            C2LevelText.text = "Lv" + C2Level.ToString();
        }
    }

    private void ResultReflection() //자신의 게임 결과(돈, 경험치) 내부 및 외부 디비에 반영하기
    {
        int exp;
        int money;
        int c1score = BattleRoomScoreManager.Player1Score;
        int c2score = BattleRoomScoreManager.Player2Score;
        if (WhoAMI == 1) //플레이어1
        {
            if (c1score > c2score) //승리
            {
                StartCoroutine(WinTextEffect());
                WinEffect.SetActive(true);
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.3f * c1score);
                money = PlayerPrefs.GetInt("Coin",0) + (20 * c1score);
            } 
            else if (c1score < c2score) //패배
            {
                StartCoroutine(LoseTextEffect());
                LoseEffect.SetActive(true);
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.1f * c1score);
                money = PlayerPrefs.GetInt("Coin",0) + (10 * c1score);
            }  
            else //무승부
            {
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.2f * c1score);
                money = PlayerPrefs.GetInt("Coin",0) + (15 * c1score);
            }
        }
        else //플레이어2
        {
            if (c1score < c2score) //승리
            {
                StartCoroutine(WinTextEffect());
                WinEffect.SetActive(true);
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.3f * c2score);
                money = PlayerPrefs.GetInt("Coin",0) + (20 * c2score);
            }
            else if (c1score > c2score) //패배
            {
                StartCoroutine(LoseTextEffect());
                LoseEffect.SetActive(true);
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.1f * c2score);
                money = PlayerPrefs.GetInt("Coin",0) + (10 * c2score);
            }
            else //무승부
            {
                exp = PlayerPrefs.GetInt("Exp",0) + (int)(0.2f * c2score);
                money = PlayerPrefs.GetInt("Coin",0) + (15 * c2score);
            }
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
            
        else{
            Glogin = false;
            code = PlayerPrefs.GetString("GuestCode");
        }
            
        User Me = new User //업로드 할 내용을 미리 선언해둠(예시)
        {
            user_no = PlayerPrefs.GetString("UserN"),
            GPlogin_status = Glogin,
            user_code = code,
            user_nick = PlayerPrefs.GetString("Nickname"),
            user_exp =  exp,
            own_coin = money,
            eq_acc = PlayerPrefs.GetInt("Acc"),
            eq_item1 = PlayerPrefs.GetInt("Item1",0),
            eq_item2 = PlayerPrefs.GetInt("Item2",0),
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
        int C1Score = BattleRoomScoreManager.Player1Score;
        int C2Score = BattleRoomScoreManager.Player2Score;
        if (C1Score > C2Score) //플레이어1 이김
        {
            for(int i = 0; i <= C2Score; i++)
            {
                C1ScoreText.text = i.ToString();
                C2ScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = C2Score; i <= C1Score; i++)
            {
                C1ScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            C1crown.SetActive(true);
            for (float j = 1.8f; j >= 1f; j -= 0.05f){
                C1crown.transform.localScale = new Vector2(j, j);
                yield return 0.5f;
            }
            StartCoroutine(Money(1, 1, C1Score));
            StartCoroutine(Money(2, 2, C2Score));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EXP(1, 1, C1Score));
            StartCoroutine(EXP(2, 2, C2Score));
        }
        else if (C1Score < C2Score) //플레이어2 이김
        {
            for (int i = 0; i <= C1Score; i++)
            {
                C1ScoreText.text = i.ToString();
                C2ScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            for (int i = C1Score; i <= C2Score; i++)
            {
                C2ScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            C2crown.SetActive(true);
            for (float j = 1.8f; j >= 1f; j -= 0.05f)
            {
                C2crown.transform.localScale = new Vector2(j, j);
                yield return 0.5f;
            }
            StartCoroutine(Money(1, 2, C1Score));
            StartCoroutine(Money(2, 1, C2Score));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EXP(1, 2, C1Score));
            StartCoroutine(EXP(2, 1, C2Score));
        }
        else // 동점
        {
            for (int i = 0; i <= C1Score; i++)
            {
                C1ScoreText.text = i.ToString();
                C2ScoreText.text = i.ToString();
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(Money(1, 3, C1Score));
            StartCoroutine(Money(2, 3, C2Score));
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(EXP(1, 3, C1Score));
            StartCoroutine(EXP(2, 3, C2Score));
        }
    }

    public IEnumerator Money(int who, int result, int score) //result  1:승리, 2:패배, 3:무승부
    {
        if (who == 1) //플레이어1
        {
            if(result == 1) //승리 (돈 두배)
            {
                int val = 20 * score;
                for (int i = 0; i <= val; i += val/20 )
                {
                    C1CoinText.text = i.ToString();
                    yield return 0.004f;
                }
            }
            else if (result == 2) //패배
            {
                int val = 10 * score;
                for (int i = 0; i <= val; i += val / 20)
                {
                    C1CoinText.text = i.ToString();
                    yield return 0.004f;
                }
            }
            else //무승부 (돈 1.5배)
            {
                int val = 15 * score;
                for (int i = 0; i <= val; i += val / 20)
                {
                    C1CoinText.text = i.ToString();
                    yield return 0.004f;
                }
            }
        }
        else //플레이어2
        {
            if (result == 1) //승리
            {
                int val = 20 * score;
                for (int i = 0; i <= val; i += val / 20)
                {
                    C2CoinText.text = i.ToString();
                    yield return 0.0004f;
                }
            }
            else if (result == 2) //패배
            {
                int val = 10 * score;
                for (int i = 0; i <= val; i += val / 20)
                {
                    C2CoinText.text = i.ToString();
                    yield return 0.0004f;
                }
            }
            else //무승부
            {
                int val = 15 * score;
                for (int i = 0; i <= val; i += val / 20)
                {
                    C2CoinText.text = i.ToString();
                    yield return 0.0004f;
                }
            }
        }

    }

    public IEnumerator EXP(int who, int result, int score) //result  1:승리, 2:패배, 3:무승부
    {
        if (who == 1) //플레이어1
        {
            if (result == 1) //승리
            {
                for (int i = C1exp; i <= C1exp + (0.3f * score); i++)
                {
                    C1expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C1Level++;
                        C1LevelText.text = "Lv" + C1Level.ToString();
                        if (PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C1Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
            else if (result == 2) //패배
            {
                for (int i = C1exp; i <= C1exp + (0.1f * score); i++)
                {
                    C1expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C1Level++;
                        C1LevelText.text = "Lv" + C1Level.ToString();
                        if (PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C1Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
            else //무승부
            {
                for (int i = C1exp; i <= C1exp + (0.2f * score); i++)
                {
                    C1expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C1Level++;
                        C1LevelText.text = "Lv" + C1Level.ToString();
                        if (PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C1Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
        }
        else //플레이어2
        {
            if (result == 1) //승리
            {
                for (int i = C2exp; i <= C2exp + (0.3f * score); i++)
                {
                    C2expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C2Level++;
                        C2LevelText.text = "Lv" + C2Level.ToString();
                        if (!PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C2Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
            else if (result == 2) //패배
            {
                for (int i = C2exp; i <= C2exp + (0.1f * score); i++)
                {
                    C2expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C2Level++;
                        C2LevelText.text = "Lv" + C2Level.ToString();
                        if (!PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C2Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
            else //무승부
            {
                for (int i = C2exp; i <= C2exp + (0.2f * score); i++)
                {
                    C2expSlider.value = i % 100;
                    if ((i % 100 == 0) && (i != 0)) //레벨업
                    {
                        C2Level++;
                        C2LevelText.text = "Lv" + C2Level.ToString();
                        if (!PhotonNetwork.isMasterClient)
                            StartCoroutine(LevelUpEffect(C2Level));
                    }
                    yield return new WaitForSeconds(0.07f);
                }
            }
        }
    }

    public void OKButtonClick()
    {
        PhotonNetwork.Disconnect();
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
