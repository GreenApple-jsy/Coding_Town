using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using UnityEngine.UI;

public class TrainingRoomManager : MonoBehaviour
{
    public Text Question_TitleText; //문제 제목
    public Text Question_ContentText; //문제 내용
    public Slider CountdownSlider;
    public Text QuestionNumberText;
    public Image QuestionNumberBg;
    public int PlayerAnswerChoice;
    public int CurrentQuestionType;
    public GameObject ResultPanel;
    public GameObject QuitPanel;
    public bool IsPause;

    public int Ready;
    public Sprite ReadyImage;
    public Sprite StartImage;
    public Image BeforeStartImage;
    public Text PlayerNameText;

    public GameObject TypeAButtons;
    public GameObject TypeBButtons;
    public GameObject TypeCButtons;

    public Text[] TypeAtext = new Text[4];
    public Text[] TypeCtext = new Text[3];

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];

    List<Question> QuestionInfo = new List<Question>(); //문제 정보 저장

    private void Awake()
    {
        Setup.LoadingNotice.SetActive(true);
        PlayerAnswerChoice = 0;
        TypeAButtons.SetActive(false);
        TypeBButtons.SetActive(false);
        TypeCButtons.SetActive(false);
        ResultPanel.SetActive(false);
        QuitPanel.SetActive(false);
        CountdownSlider.gameObject.SetActive(false);
        QuestionNumberText.gameObject.SetActive(false);
        Question_TitleText.gameObject.SetActive(false);
        Question_ContentText.gameObject.SetActive(false);
        QuestionNumberBg.gameObject.SetActive(false);
        BeforeStartImage.gameObject.SetActive(false);
        Ready = 0;
        IsPause = false;

        Setup.Setting();
        
        PlayerNameText.text = PlayerPrefs.GetString("NickName");
        
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape) && !IsPause)
            {
                Time.timeScale = 0;
                IsPause = true;
                QuitPanel.SetActive(true);
            }
        }
    }

    public void TypeA()
    {
        int[] question_Number = new int[10]; //출제할 문제 번호 저장용

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
                int RandomQuestionN = Random.Range(i * PartSize, (i + 1) * PartSize); //각 파트 사이즈 내에서 랜덤하게 문제 고름
                question_Number[i] = RandomQuestionN * 3;
                Debug.Log(question_Number[i]);
            }
            StartCoroutine(QuestionSetting(question_Number));
        }, null);
    }

    public void TypeB()
    {
        int[] question_Number = new int[10]; //출제할 문제 번호 저장용

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
                int RandomQuestionN = Random.Range(i * PartSize, (i + 1) * PartSize); //각 파트 사이즈 내에서 랜덤하게 문제 고름
                question_Number[i] = RandomQuestionN * 3 + 1;
                Debug.Log(question_Number[i]);
            }
            StartCoroutine(QuestionSetting(question_Number));
        }, null);
    }

    public void TypeC()
    {
        int[] question_Number = new int[10]; //출제할 문제 번호 저장용

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
                int RandomQuestionN = Random.Range(i * PartSize, (i + 1) * PartSize); //각 파트 사이즈 내에서 랜덤하게 문제 고름
                question_Number[i] = RandomQuestionN * 3 + 2;
                Debug.Log(question_Number[i]);
            }
            StartCoroutine(QuestionSetting(question_Number));
        }, null);
    }

    public void TypeD()
    {
        int[] question_Number = new int[10]; //출제할 문제 번호 저장용

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
                Debug.Log(question_Number[i]);
            }
            StartCoroutine(QuestionSetting(question_Number));
        }, null);
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
        Setup.LoadingNotice.SetActive(false);
        StartCoroutine(ShowReadySign());
        //StartCoroutine(ShowQuestions());
    }

    IEnumerator ShowReadySign()
    {
        BeforeStartImage.GetComponent<Image>().sprite = ReadyImage;
        BeforeStartImage.gameObject.transform.localPosition = new Vector2(1580, 0);
        BeforeStartImage.gameObject.SetActive(true);
        for (int i = 1580; i >= -1580; i = i - 20)
        {
            BeforeStartImage.gameObject.transform.localPosition = new Vector2(i, 0);
            if (i == 0)
                yield return new WaitForSeconds(0.5f);
            yield return 0.0000001f;
        }
        BeforeStartImage.GetComponent<Image>().sprite = StartImage;
        for (int i = 1580; i >= -1580; i = i - 20)
        {
            BeforeStartImage.gameObject.transform.localPosition = new Vector2(i, 0);
            if (i == 0)
                yield return new WaitForSeconds(0.5f);
            yield return 0.0000001f;
        }
        BeforeStartImage.gameObject.SetActive(false);
        StartCoroutine(ShowQuestions());
    }

    IEnumerator ShowQuestions() //사용자에게 문제 보여주기
    {
        QuestionNumberText.gameObject.SetActive(true);
        Question_TitleText.gameObject.SetActive(true);
        Question_ContentText.gameObject.SetActive(true);
        QuestionNumberBg.gameObject.SetActive(true);

        for (int i = 0; i < 10; i++)
        {
            QuestionNumberText.text = "문제 " + (i + 1).ToString() + " / 10";
            Question_TitleText.text = QuestionInfo[i].ques_title;
            Question_ContentText.text = QuestionInfo[i].ques_content;
            PlayerAnswerChoice = 0;
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
            for (int j = 20; j > 0; j--)
            {
                CountdownSlider.value = j;
                if (PlayerAnswerChoice != 0) //시간 마감 전에 고르기 완료
                    break;
                yield return new WaitForSeconds(1.0f);
            }
            var sr = GameObject.Find("TrainingScoreManager").GetComponent<TrainingRoomScoreManager>();
            StartCoroutine(sr.ScoreReflection(QuestionInfo[i].ques_answer, PlayerAnswerChoice, CurrentQuestionType, QuestionInfo[i].ques_point));
            yield return new WaitForSeconds(3.0f);
            //QuestionInfo[i].ques_answer 랑  PlayerAnswerChoice 비교해서 채점하기. 난 따로 점수 관리 스크립트가 있었음
            /*var sr = GameObject.Find("AutoBattleScoreManager").GetComponent<AutoBattleRoomScoreManager>();
            StartCoroutine(sr.ScoreReflection(QuestionInfo[i].ques_answer, PlayerAnswerChoice, RobotAnswerChoice, CurrentQuestionType, QuestionInfo[i].ques_point));*/
            
            CountdownSlider.gameObject.SetActive(false);
        }
        var RP = GameObject.Find("TrainingResultManager").GetComponent<TrainingRoomResultManager>();
        ResultPanel.SetActive(true);
        StartCoroutine(RP.ShowScore());
        //결과 보여주기
        /*var RP = GameObject.Find("AutoBattleResultManager").GetComponent<AutoBattleRoomResultManager>();
        ResultPanel.SetActive(true);
        StartCoroutine(RP.ShowScore());*/
    }

    public void AnswerChoice1() //1번 버튼에 붙이는 함수임 (A타입의 첫번째 버튼, B타입의 O버튼, C타입의 첫번째 버튼에 해당함)
    {
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

    public void AnswerChoice2() //2번 버튼에 붙이는 함수임(A타입의 두번째 버튼, B타입의 X버튼, C타입의 두번째 버튼에 해당함)
    {
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

    public void AnswerChoice3() //3번 버튼에 붙이는 함수임(A타입의 세번째 버튼, C타입의 세번째 버튼에 해당함)
    {
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

    public void AnswerChoice4() //4번 버튼에 붙이는 함수임(A타입의 네번째 버튼에 해당함)
    {
        PlayerAnswerChoice = 4;
        for (int j = 0; j < 4; j++) //4객관식만 있음
        {
            TypeAButton[j].interactable = false;
            if (j != 3)
                TypeAButton[j].GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        }
    }

    public void QuitGame()
    {
        TrainingLobbyManager Tm = GameObject.Find("TrainingLobbyManager").GetComponent<TrainingLobbyManager>();
        Tm.QuestionPanel.SetActive(false);
    }

    public void CloseQuitPanel()
    {
        QuitPanel.SetActive(false);
        Time.timeScale = 1;
        IsPause = false;
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
}
