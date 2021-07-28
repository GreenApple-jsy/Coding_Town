using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingRoomScoreManager : MonoBehaviour
{

    public static int PlayerScore;
    public Text PlayerScoreText;

    public Image TypeAanswer;
    public Image TypeBanswer;
    public Image TypeCanswer;

    public GameObject PlayerCorrect;
    public GameObject PlayerWrong;

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];


    public void Awake()
    {
        PlayerScore = 0;
        PlayerScoreText.text = "0";
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
        PlayerCorrect.SetActive(false);
        PlayerWrong.SetActive(false);
    }

    public IEnumerator ScoreReflection(int Answer, int PlayerAnswerChoice, int QuestionType, int point)
    {
        if (QuestionType == 1) //4객관식
        {
            TypeAanswer.gameObject.transform.position = TypeAButton[Answer - 1].gameObject.transform.position;
            TypeAanswer.gameObject.SetActive(true);
        }
        else if (QuestionType == 2) //ox
        {
            TypeBanswer.gameObject.transform.position = TypeBButton[Answer - 1].gameObject.transform.position;
            TypeBanswer.gameObject.SetActive(true);
        }
        else //3객관식
        {
            TypeCanswer.gameObject.transform.position = TypeCButton[Answer - 1].gameObject.transform.position;
            TypeCanswer.gameObject.SetActive(true);
        }

        if (PlayerAnswerChoice == Answer) //맞은 경우
        {
            PlayerCorrect.SetActive(true);

            for (int i = 0; i < point; i++)
            {
                PlayerScore += 1;
                PlayerScoreText.text = PlayerScore.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else //틀린 경우
        {
            PlayerWrong.SetActive(true);
        }
        yield return new WaitForSeconds(1.3f);
        PlayerCorrect.SetActive(false);
        PlayerWrong.SetActive(false);
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
    }
}
