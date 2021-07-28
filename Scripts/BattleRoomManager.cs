using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Photon;
using UnityEngine.UI;

public class BattleRoomManager : PunBehaviour
{   //Player1은 방장(왼쪽), Player2는 게스트(오른쪽)
    public Text Question_TitleText;
    public Text Question_ContentText;
    public static bool Player2InTheBattleRoom;
    public int Ready;
    private PhotonView pv;
    public Sprite ReadyImage;
    public Sprite StartImage;
    public Image BeforeStartImage;
    public Slider CountdownSlider;
    public Image Player1CharacterImage;
    public Image Player2CharacterImage;
    public Image ResultPanelPlayer1CharacterImage;
    public Image ResultPanelPlayer2CharacterImage;
    public Text QuestionNumberText;
    public Text Player1NameText;
    public Text Player2NameText;
    public int Player1AnswerChoice;
    public int Player2AnswerChoice;
    public int CurrentQuestionType;
    public int CurrentQuestionAnswer;
    public GameObject ResultPanel;
    public GameObject Player1CharacterCloud;
    public GameObject Player2CharacterCloud;
    public GameObject QuestionCloud;

    public GameObject TypeAButtons;
    public GameObject TypeBButtons;
    public GameObject TypeCButtons;

    public Text[] TypeAtext = new Text[4];
    public Text[] TypeCtext = new Text[3];

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];

    public int Player1Item1;
    public int Player1Item2;
    public int Player2Item1;
    public int Player2Item2;

    public Button Player1Item1Button;
    public Button Player1Item2Button;
    public Button Player2Item1Button;
    public Button Player2Item2Button;

    public GameObject Player1Item1Lock;
    public GameObject Player1Item2Lock;
    public GameObject Player2Item1Lock;
    public GameObject Player2Item2Lock;

    public Sprite[] ItemImage = new Sprite[3];
    public Sprite[] CharacterImage = new Sprite[3];

    List<Question> QuestionInfo = new List<Question>(); //문제 정보 저장

    public AudioSource ma;
    public AudioSource ButtonSound;

    private void Awake()
    {
        Setup.LoadingNotice.SetActive(true);
        Player2InTheBattleRoom = false;
        ResultPanel.SetActive(false);
        Player1AnswerChoice = 0;
        Player2AnswerChoice = 0;
        Player1CharacterCloud.SetActive(false);
        Player2CharacterCloud.SetActive(false);
        QuestionCloud.SetActive(false);
        BeforeStartImage.gameObject.SetActive(false);
        Ready = 0;
        TypeAButtons.SetActive(false);
        TypeBButtons.SetActive(false);
        TypeCButtons.SetActive(false);
        CountdownSlider.value = 15;
        QuestionNumberText.gameObject.SetActive(false);
        Question_TitleText.gameObject.SetActive(false);
        Question_ContentText.gameObject.SetActive(false);
        Player1Item1Button.gameObject.SetActive(true);
        Player1Item2Button.gameObject.SetActive(true);
        Player2Item1Button.gameObject.SetActive(true);
        Player2Item2Button.gameObject.SetActive(true);
        Player1Item1Lock.SetActive(false);
        Player1Item2Lock.SetActive(false);
        Player2Item1Lock.SetActive(false);
        Player2Item2Lock.SetActive(false);
        ma = this.GetComponent<AudioSource>();
        ButtonSound = GameObject.Find("ButtonSound").GetComponent<AudioSource>();

        Setup.Setting();
        pv = GetComponent<PhotonView>();

        if (PhotonNetwork.isMasterClient)
        {
            Player1Item1 = -1;
            Player1Item2 = -1;
            Player2Item1Button.interactable = false;
            Player2Item2Button.interactable = false;
            Player1NameText.text = PhotonNetwork.playerList[1].NickName;
            Player2NameText.text = PhotonNetwork.playerList[0].NickName;
            if (PlayerPrefs.GetInt("i" + PlayerPrefs.GetInt("Item1").ToString()) > 0)
                Player1Item1 = PlayerPrefs.GetInt("Item1");
            if (PlayerPrefs.GetInt("i" + PlayerPrefs.GetInt("Item2").ToString()) > 0)
                Player1Item2 = PlayerPrefs.GetInt("Item2");
            Player1CharacterImage.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
            ResultPanelPlayer1CharacterImage.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
            StartCoroutine(CheckOtherPlayer());
        }
        else
        {
            Player2Item1 = -1;
            Player2Item2 = -1;
            Player1Item1Button.interactable = false;
            Player1Item2Button.interactable = false;
            Player1NameText.text = PhotonNetwork.playerList[0].NickName;
            Player2NameText.text = PhotonNetwork.playerList[1].NickName;
            if (PlayerPrefs.GetInt("i" + PlayerPrefs.GetInt("Item1").ToString()) > 0)
                Player2Item1 = PlayerPrefs.GetInt("Item1");
            if (PlayerPrefs.GetInt("i" + PlayerPrefs.GetInt("Item2").ToString()) > 0)
                Player2Item2 = PlayerPrefs.GetInt("Item2");
            pv.RPC("Player2Arrive", PhotonTargets.Others, null);
            pv.RPC("ItemSetting", PhotonTargets.All, 2, Player2Item1, Player2Item2);
            pv.RPC("CharacterImageSetting", PhotonTargets.Others, PlayerPrefs.GetInt("Character"));
            Player2CharacterImage.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
            ResultPanelPlayer2CharacterImage.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
        }
    }

    [PunRPC]
    public void CharacterImageSetting(int character)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Player2CharacterImage.sprite = CharacterImage[character];
            ResultPanelPlayer2CharacterImage.sprite = CharacterImage[character];
        }

        else
        {
            Player1CharacterImage.sprite = CharacterImage[character];
            ResultPanelPlayer1CharacterImage.sprite = CharacterImage[character];
        }
            
    }


    [PunRPC]
    public void ItemSetting(int who, int item1, int item2)
    {
        if (who == 1)
        {
            if (item1 == -1)
                Player1Item1Button.gameObject.SetActive(false);
            else
                Player1Item1Button.GetComponent<Image>().sprite = ItemImage[item1];

            if (item2 == -1)
                Player1Item2Button.gameObject.SetActive(false);
            else
                Player1Item2Button.GetComponent<Image>().sprite = ItemImage[item2];
        }
        else
        {
            if (item1 == -1)
                Player2Item1Button.gameObject.SetActive(false);
            else
                Player2Item1Button.GetComponent<Image>().sprite = ItemImage[item1];

            if (item2 == -1)
                Player2Item2Button.gameObject.SetActive(false);
            else
                Player2Item2Button.GetComponent<Image>().sprite = ItemImage[item2];
        }
    }

    IEnumerator CheckOtherPlayer()
    {
        int[] question_Number = new int[10];
        while (Player2InTheBattleRoom == false) //다른 사용자가 씬에 들어와서 exp를 보낼때까지 기다림
            yield return null;
        pv.RPC("ItemSetting", PhotonTargets.All, 1, Player1Item1, Player1Item2);
        pv.RPC("CharacterImageSetting", PhotonTargets.Others, PlayerPrefs.GetInt("Character"));
        
        Management temp;
        Setup.context.LoadAsync<Management>("questionGroup_count", (AmazonDynamoDBResult<Management> r) =>
        {
            if (r.Exception != null)
            {
                Debug.LogException(r.Exception);
                return;
            }
            temp = r.Result; //현재 db에 올라가있는 문제 그룹의 개수를 받아옴
            int PartSize = temp.value / 10;
            for (int i = 0; i < 10; i++) //출제할 문제 10개 고르기
            {
                int RandomQuestionN = Random.Range(i * 3 * PartSize, (i + 1) * 3 * PartSize); //각 파트 사이즈 내에서 랜덤하게 문제 고름
                question_Number[i] = RandomQuestionN;
            }
            StartCoroutine(QuestionSetting(question_Number));
            pv.RPC("QuestionList", PhotonTargets.Others, question_Number[0], question_Number[1], question_Number[2], question_Number[3], question_Number[4], question_Number[5], question_Number[6], question_Number[7], question_Number[8], question_Number[9]);
        }, null);
    }

    [PunRPC]
    public void Player2Arrive()
    {
        Player2InTheBattleRoom = true;
    }

    [PunRPC]
    public void QuestionList(int q0, int q1, int q2, int q3, int q4, int q5, int q6, int q7, int q8, int q9) //포톤에서 list 타입은 전송 안됨, 문제수만큼 변수 전달 필요
    {
        int[] question_Number = new int[10];
        question_Number[0] = q0; question_Number[1] = q1; question_Number[2] = q2; question_Number[3] = q3; question_Number[4] = q4;
        question_Number[5] = q5; question_Number[6] = q6; question_Number[7] = q7; question_Number[8] = q8; question_Number[9] = q9;
        StartCoroutine(QuestionSetting(question_Number));
    }

    [PunRPC]
    public void SendReady() //문제 세팅 완료
    {
        Ready = Ready + 1;
    }

    IEnumerator QuestionSetting(int[] question_Number)
    {
        for (int i = 0; i < 10; i++)
        {//정해진 문제 번호인 문제들의 정보를 받아옴
            Setup.context.LoadAsync<Question>(question_Number[i].ToString(), (AmazonDynamoDBResult<Question> result) =>
            {
                if (result.Exception != null)
                {
                    Debug.LogException(result.Exception);
                    return;
                }
                QuestionInfo.Add(result.Result);
            }, null);
            while (QuestionInfo.Count < i + 1) //문제마다 받아오는 시간이 다를 수 있음(순차적으로 문제를 저장하기 위함)
                yield return null;
        }
        while (QuestionInfo.Count < 10)
            yield return null;
        pv.RPC("SendReady", PhotonTargets.All, null);
        while (Ready < 2) // + 만약 문제 받기가 지체되는 경우 처리하기
            yield return null;
        Setup.LoadingNotice.SetActive(false);
        StartCoroutine(ShowReadySign());
    }

    IEnumerator ShowReadySign()
    {
        BeforeStartImage.GetComponent<Image>().sprite = ReadyImage;
        BeforeStartImage.gameObject.transform.localPosition = new Vector2(1580, 0);
        BeforeStartImage.gameObject.SetActive(true);
        for (int i = 1580; i >= -1580; i = i - 30)
        {
            BeforeStartImage.gameObject.transform.localPosition = new Vector2(i, 0);
            if (i == -10)
                yield return new WaitForSeconds(0.5f);
            yield return 0.0000001f;
        }
        BeforeStartImage.GetComponent<Image>().sprite = StartImage;
        for (int i = 1580; i >= -1580; i = i - 30)
        {
            BeforeStartImage.gameObject.transform.localPosition = new Vector2(i, 0);
            if (i == -10)
                yield return new WaitForSeconds(0.5f);
            yield return 0.0000001f;
        }
        BeforeStartImage.gameObject.SetActive(false);
        StartCoroutine(ShowQuestions());
    }

    IEnumerator ShowQuestions()
    {
        QuestionNumberText.gameObject.SetActive(true);
        Question_TitleText.gameObject.SetActive(true);
        Question_ContentText.gameObject.SetActive(true);

        for (int i = 0; i < 10; i++)
        {
            ma.Play();
            QuestionNumberText.text = "문제 " + (i + 1).ToString() + " / 10";
            Question_TitleText.text = QuestionInfo[i].ques_title;
            Question_ContentText.text = QuestionInfo[i].ques_content;
            CurrentQuestionAnswer = QuestionInfo[i].ques_answer;
            Player1AnswerChoice = 0;
            Player2AnswerChoice = 0;
            if (QuestionInfo[i].ques_type == "A") //4 짧은 객관식
            {
                CurrentQuestionType = 1;
                for (int j = 0; j < 4; j++)
                {
                    TypeAtext[j].text = QuestionInfo[i].ques_answerList[j];
                    TypeAButton[j].interactable = true;
                    TypeAButton[j].GetComponent<Image>().color = new Color(1, 1, 1);
                }
                TypeAButtons.SetActive(true);
                TypeBButtons.SetActive(false);
                TypeCButtons.SetActive(false);
            }
            else if (QuestionInfo[i].ques_type == "B") //OX
            {
                CurrentQuestionType = 2;
                for (int j = 0; j < 2; j++)
                {
                    TypeBButton[j].interactable = true;
                    TypeBButton[j].GetComponent<Image>().color = new Color(1, 1, 1);
                }
                TypeAButtons.SetActive(false);
                TypeBButtons.SetActive(true);
                TypeCButtons.SetActive(false);
            }
            else if (QuestionInfo[i].ques_type == "C") //3 긴 객관식
            {
                CurrentQuestionType = 3;
                for (int j = 0; j < 3; j++)
                {
                    TypeCtext[j].text = QuestionInfo[i].ques_answerList[j];
                    TypeCButton[j].interactable = true;
                    TypeCButton[j].GetComponent<Image>().color = new Color(1, 1, 1);
                }
                TypeAButtons.SetActive(false);
                TypeBButtons.SetActive(false);
                TypeCButtons.SetActive(true);
            }
            CountdownSlider.gameObject.SetActive(true);
            for (int j = 15; j > 0; j--)
            {
                CountdownSlider.value = j;
                if (Player1AnswerChoice != 0 && Player2AnswerChoice != 0) //시간 마감 전에 둘 다 고르기 완료
                    break;
                yield return new WaitForSeconds(1.0f);
            }
            var sr = GameObject.Find("ScoreManager").GetComponent<BattleRoomScoreManager>();
            StartCoroutine(sr.ScoreReflection(PhotonNetwork.isMasterClient, QuestionInfo[i].ques_answer, Player1AnswerChoice, Player2AnswerChoice, CurrentQuestionType, QuestionInfo[i].ques_point));
            yield return new WaitForSeconds(3.0f);
        }
        var RP = GameObject.Find("BattleResultManager").GetComponent<BattleRoomResultManager>();
        ResultPanel.SetActive(true);
        StartCoroutine(RP.ShowScore());
    }

    public void AnswerChoice1()
    {
        ButtonSound.Play();
        if (CurrentQuestionType == 1) //4 객관식
        {
            for (int j = 0; j < 4; j++)
            {
                TypeAButton[j].interactable = false;
                if (j != 0)
                    TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        else if (CurrentQuestionType == 2) //ox
        {
            for (int j = 0; j < 2; j++)
            {
                TypeBButton[j].interactable = false;
                if (j != 0)
                    TypeBButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        else //3 객관식
        {
            for (int j = 0; j < 3; j++)
            {
                TypeCButton[j].interactable = false;
                if (j != 0)
                    TypeCButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        if (PhotonNetwork.isMasterClient)
            pv.RPC("AnswerChoice", PhotonTargets.All, 1, 1);
        else
            pv.RPC("AnswerChoice", PhotonTargets.All, 2, 1);
    }

    public void AnswerChoice2()
    {
        ButtonSound.Play();
        if (CurrentQuestionType == 1) //4 객관식
        {
            for (int j = 0; j < 4; j++)
            {
                TypeAButton[j].interactable = false;
                if (j != 1)
                    TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        else if (CurrentQuestionType == 2) //ox
        {
            for (int j = 0; j < 2; j++)
            {
                TypeBButton[j].interactable = false;
                if (j != 1)
                    TypeBButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        else //3 객관식
        {
            for (int j = 0; j < 3; j++)
            {
                TypeCButton[j].interactable = false;
                if (j != 1)
                    TypeCButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        if (PhotonNetwork.isMasterClient)
            pv.RPC("AnswerChoice", PhotonTargets.All, 1, 2);
        else
            pv.RPC("AnswerChoice", PhotonTargets.All, 2, 2);
    }

    public void AnswerChoice3()
    {
        ButtonSound.Play();
        if (CurrentQuestionType == 1) //4 객관식
        {
            for (int j = 0; j < 4; j++)
            {
                TypeAButton[j].interactable = false;
                if (j != 2)
                    TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        else //3 객관식
        {
            for (int j = 0; j < 3; j++)
            {
                TypeCButton[j].interactable = false;
                if (j != 2)
                    TypeCButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            }
        }
        if (PhotonNetwork.isMasterClient)
            pv.RPC("AnswerChoice", PhotonTargets.All, 1, 3);
        else
            pv.RPC("AnswerChoice", PhotonTargets.All, 2, 3);
    }

    public void AnswerChoice4()
    {
        ButtonSound.Play();
        for (int j = 0; j < 4; j++) //4객관식만 있음
        {
            TypeAButton[j].interactable = false;
            if (j != 3)
                TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        }
        if (PhotonNetwork.isMasterClient)
            pv.RPC("AnswerChoice", PhotonTargets.All, 1, 4);
        else
            pv.RPC("AnswerChoice", PhotonTargets.All, 2, 4);
    }

    [PunRPC]
    public void AnswerChoice(int player, int answer)
    {
        if (player == 1)
            Player1AnswerChoice = answer;
        else
            Player2AnswerChoice = answer;
    }

    public void Item1Click()
    {
        string item1N = PlayerPrefs.GetInt("Item1").ToString();
        PlayerPrefs.SetInt("i" + item1N, PlayerPrefs.GetInt("i" + item1N) - 1);
        OwnItem me = new OwnItem
        {
            user_no = PlayerPrefs.GetString("UserN"),
            item_no = item1N,
            item_num = PlayerPrefs.GetInt("i" + item1N)
        };
        
        Setup.context.SaveAsync<OwnItem>(me, (res) =>
        {
            if (res.Exception == null)
                Debug.Log("아이템 사용 개수 반영 성공");
        });

        if (PhotonNetwork.isMasterClient)
        {
            Player1Item1Lock.SetActive(true);
            Player1Item1Button.interactable = false;
            if (Player1Item1 == 0)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 1, 0);
                EraserItem();
            }
            else if (Player1Item1 == 1)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 1, 1);
                CloudItem();
            }
            else
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 1, 2);
                GlassItem();
            }
        }
        else
        {
            Player2Item1Lock.SetActive(true);
            Player2Item1Button.interactable = false;
            if (Player2Item1 == 0)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 1, 0);
                EraserItem();
            }
            else if (Player2Item1 == 1)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 1, 1);
                CloudItem();
            }
            else
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 1, 2);
                GlassItem();
            }
        }
    }

    public void Item2Click()
    {
        string item2N = PlayerPrefs.GetInt("Item2").ToString();
        PlayerPrefs.SetInt("i" + item2N, PlayerPrefs.GetInt("i" + item2N) - 1);
        OwnItem me = new OwnItem
        {
        user_no = PlayerPrefs.GetString("UserN"),
        item_no = item2N,
        item_num = PlayerPrefs.GetInt("i" + item2N)
        };
        Setup.context.SaveAsync<OwnItem>(me, (res) =>
        {
            if (res.Exception == null)
                Debug.Log("아이템 사용 개수 반영 성공");
        });

        if (PhotonNetwork.isMasterClient)
        {
            Player1Item2Lock.SetActive(true);
            Player1Item2Button.interactable = false;
            if (Player1Item2 == 0)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 2, 0);
                EraserItem();
            }
            else if (Player1Item2 == 1)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 2, 1);
                CloudItem();
            }
            else
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 1, 2, 2);
                GlassItem();
            }
        }
        else
        {
            Player2Item2Lock.SetActive(true);
            Player2Item2Button.interactable = false;
            if (Player2Item2 == 0)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 2, 0);
                EraserItem();
            }
            else if (Player2Item2 == 1)
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 2, 1);
                CloudItem();
            }
            else
            {
                pv.RPC("ItemUsageNotice", PhotonTargets.Others, 2, 2, 2);
                GlassItem();
            }
        }
    }


    public void EraserItem() //지우개 아이템
    {
        if (CurrentQuestionType == 1) //4 객관식
        {
            int choice = 0;
            choice = Random.Range(1, 5);
            while (choice == CurrentQuestionAnswer)
                choice = Random.Range(1, 5);
            TypeAButton[choice - 1].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            TypeAButton[choice - 1].interactable = false;
        }
        else if (CurrentQuestionType == 2) //ox
        {
            int choice = 0;
            choice = Random.Range(1, 3);
            while (choice == CurrentQuestionAnswer)
                choice = Random.Range(1, 3);
            TypeBButton[choice - 1].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            TypeBButton[choice - 1].interactable = false;
        }
        else //3 객관식
        {
            int choice = 0;
            choice = Random.Range(1, 4);
            while (choice == CurrentQuestionAnswer)
                choice = Random.Range(1, 4);
            TypeCButton[choice - 1].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
            TypeCButton[choice - 1].interactable = false;
        }
    }

    public void CloudItem() //구름 아이템
    {
        StartCoroutine(ShowCharacterCloud());
    }

    IEnumerator ShowCharacterCloud()
    {
        if (PhotonNetwork.isMasterClient)
        {
            Player2CharacterCloud.SetActive(true);
            for (float i = 0f; i <= 1f; i += 0.1f)
            {
                Player2CharacterCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
                yield return 0.1f;
            }
            yield return new WaitForSeconds(3.5f);
            for (float i = 1f; i >= 0f; i -= 0.1f)
            {
                Player2CharacterCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
                yield return 0.1f;
            }
            Player2CharacterCloud.SetActive(false);
        }
        else
        {
            Player1CharacterCloud.SetActive(true);
            for (float i = 0f; i <= 1f; i += 0.1f)
            {
                Player1CharacterCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
                yield return 0.1f;
            }
            yield return new WaitForSeconds(3.5f);
            for (float i = 1f; i >= 0f; i -= 0.1f)
            {
                Player1CharacterCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
                yield return 0.1f;
            }
            Player1CharacterCloud.SetActive(false);
        }
    }

    public void GlassItem() //돋보기 아이템
    {
        if (CurrentQuestionAnswer == 1)
            AnswerChoice1();
        else if (CurrentQuestionAnswer == 2)
            AnswerChoice2();
        else if (CurrentQuestionAnswer == 3)
            AnswerChoice3();
        else
            AnswerChoice4();
    }

    [PunRPC]
    public void ItemUsageNotice(int who, int position, int type)
    {
        if (who == 1)
        {
            if (position == 1)
                Player1Item1Lock.SetActive(true);
            else if(position == 2)
                Player1Item2Lock.SetActive(true);
        }
        else
        {
            if (position == 1)
                Player2Item1Lock.SetActive(true);
            else if (position == 2)
                Player2Item2Lock.SetActive(true);
        }
        if (type == 1) //구름
        {
            StartCoroutine(ShowProblemCloud());
        }
    }

    IEnumerator ShowProblemCloud()
    {
        QuestionCloud.SetActive(true);
        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            QuestionCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
            yield return 0.1f;
        }
        yield return new WaitForSeconds(3.5f);
        for (float i = 1f; i >= 0f; i -= 0.1f)
        {
            QuestionCloud.GetComponent<Transform>().localScale = new Vector2(i, i);
            yield return 0.1f;
        }
        QuestionCloud.SetActive(false);
    }


    [DynamoDBTable("ques_list")]
    public class Question
    {
        [DynamoDBHashKey] // Hash key.
        public string ques_no { get; set; }
        [DynamoDBProperty]
        public int ques_group { get; set; }
        [DynamoDBProperty]
        public string ques_type { get; set; }
        [DynamoDBProperty]
        public string ques_title { get; set; }
        [DynamoDBProperty]
        public string ques_content { get; set; }
        [DynamoDBProperty]
        public List<string> ques_answerList { get; set; }
        [DynamoDBProperty]
        public int ques_answer { get; set; }
        [DynamoDBProperty]
        public int ques_point { get; set; }
    }

    [DynamoDBTable("Management")]
    public class Management
    {
        [DynamoDBHashKey] // Hash key.
        public string type { get; set; }
        [DynamoDBProperty]
        public int value { get; set; }
    }

    [DynamoDBTable("own_item")]
    public class OwnItem
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Hash key.
        public string item_no { get; set; }
        [DynamoDBProperty]
        public int item_num { get; set; }
    }
}