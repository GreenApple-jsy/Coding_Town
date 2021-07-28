using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using UnityEngine.UI;

public class AutoBattleRoomManager : MonoBehaviour
{   //Player 캐릭터는 왼쪽, 로봇은 오른쪽(답 맞출 확률 일단 70%)
    public Text Question_TitleText;
    public Text Question_ContentText;
    public Sprite ReadyImage;
    public Sprite StartImage;
    public Image BeforeStartImage;
    public Slider CountdownSlider;
    public Image PlayerCharacterImage;
    public Text QuestionNumberText;
    public Text PlayerCharacterNameText;
    public Text RobotNameText;
    public int PlayerAnswerChoice;
    public int RobotAnswerChoice;
    public int CurrentQuestionType;
    public GameObject ResultPanel;

    public GameObject TypeAButtons;
    public GameObject TypeBButtons;
    public GameObject TypeCButtons;

    public Text[] TypeAtext = new Text[4];
    public Text[] TypeCtext = new Text[3];

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];

    public Sprite[] CharacterImage = new Sprite[3];

    List<Question> QuestionInfo = new List<Question>(); //문제 정보 저장

    public AudioSource ma;
    public AudioSource ButtonSound;

    private void Awake()
    {
        PlayerCharacterImage.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
        Setup.LoadingNotice.SetActive(true);
        ResultPanel.SetActive(false);
        PlayerAnswerChoice = 0;
        RobotAnswerChoice = 0;
        BeforeStartImage.gameObject.SetActive(false);
        TypeAButtons.SetActive(false);
        TypeBButtons.SetActive(false);
        TypeCButtons.SetActive(false);
        CountdownSlider.value = 15;
        QuestionNumberText.gameObject.SetActive(false);
        Question_TitleText.gameObject.SetActive(false);
        Question_ContentText.gameObject.SetActive(false);
        Setup.Setting();
        PlayerCharacterNameText.text = PlayerPrefs.GetString("Nickname");
        RobotNameText.text = "코딩 로봇";
        int[] question_Number = new int[10];
        ma = this.GetComponent<AudioSource>();
        ButtonSound = GameObject.Find("ButtonSound").GetComponent<AudioSource>();

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
        }, null);
    }

    IEnumerator QuestionSetting(int[] question_Number)
    {
        for (int i = 0; i < 10; i++) //문제 수대로 수정 필요
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
            PlayerAnswerChoice = 0;
            RobotAnswerChoice = 0;
            if (QuestionInfo[i].ques_type == "A") //4 짧은 객관식
            {
                RobotAnswerChoice = RobotChoiceA(i);
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
                RobotAnswerChoice = RobotChoiceB(i);
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
                RobotAnswerChoice = RobotChoiceC(i);
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
                if (PlayerAnswerChoice != 0 && RobotAnswerChoice != 0) //시간 마감 전에 둘 다 고르기 완료
                    break;
                
                yield return new WaitForSeconds(1.0f);
            }
            var sr = GameObject.Find("AutoBattleScoreManager").GetComponent<AutoBattleRoomScoreManager>();
            StartCoroutine(sr.ScoreReflection(QuestionInfo[i].ques_answer, PlayerAnswerChoice, RobotAnswerChoice, CurrentQuestionType, QuestionInfo[i].ques_point));
            yield return new WaitForSeconds(3.0f);
        }
        var RP = GameObject.Find("AutoBattleResultManager").GetComponent<AutoBattleRoomResultManager>();
        ResultPanel.SetActive(true);
        StartCoroutine(RP.ShowScore());
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

    public int RobotChoiceA(int qnum) //4지선다
    {
        int answer = 0;
        int p = Random.Range(1, 11);
        if (p <= 7) //맞출 확률 70프로
            return QuestionInfo[qnum].ques_answer;
        else
        {
            answer = Random.Range(1, 5);
            while (answer == QuestionInfo[qnum].ques_answer)
                answer = Random.Range(1, 5);
            return answer;
        }
    }

    public int RobotChoiceB(int qnum) //오엑스
    {
        int answer = 0;
        int p = Random.Range(1, 11);
        if (p <= 7) //맞출 확률 70프로
            return QuestionInfo[qnum].ques_answer;
        else
        {
            answer = Random.Range(1, 3);
            while (answer == QuestionInfo[qnum].ques_answer)
                answer = Random.Range(1, 3);
            return answer;
        }
    }

    public int RobotChoiceC(int qnum) //3지선다
    {
        int answer = 0;
        int p = Random.Range(1, 11);
        if (p <= 7) //맞출 확률 70프로
            return QuestionInfo[qnum].ques_answer;
        else
        {
            answer = Random.Range(1, 4);
            while (answer == QuestionInfo[qnum].ques_answer)
                answer = Random.Range(1, 4);
            return answer;
        }
    }

    public void AnswerChoice1()
    {
        ButtonSound.Play();
        PlayerAnswerChoice = 1;
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
    }

    public void AnswerChoice2()
    {
        ButtonSound.Play();
        PlayerAnswerChoice = 2;
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
    }

    public void AnswerChoice3()
    {
        ButtonSound.Play();
        PlayerAnswerChoice = 3;
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
    }

    public void AnswerChoice4()
    {
        ButtonSound.Play();
        PlayerAnswerChoice = 4;
        for (int j = 0; j < 4; j++) //4객관식만 있음
        {
            TypeAButton[j].interactable = false;
            if (j != 3)
                TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        }
    }
}