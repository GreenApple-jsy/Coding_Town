using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoBattleRoomScoreManager : MonoBehaviour
{
    public Text PlayerScoreText;
    public Text RobotScoreText;

    public static int PlayerScore;
    public static int RobotScore;

    public Image TypeAanswer;
    public Image TypeBanswer;
    public Image TypeCanswer;

    public GameObject PlayerCorrect;
    public GameObject PlayerWrong;
    public GameObject RobotCorrect;
    public GameObject RobotWrong;

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];

    public AudioClip CorrectSound;
    public AudioClip WrongSound;
    public AudioSource ma;

    public ParticleSystem PlayerCorrectEffect;
    public ParticleSystem PlayerWrongEffect;
    public ParticleSystem RobotCorrectEffect;
    public ParticleSystem RobotWrongEffect;

    private void Awake()
    {
        PlayerScore = 0;
        RobotScore = 0;
        PlayerScoreText.text = "0";
        RobotScoreText.text = "0";
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
        PlayerCorrect.SetActive(false);
        PlayerWrong.SetActive(false);
        RobotCorrect.SetActive(false);
        RobotWrong.SetActive(false);
        ma = this.GetComponent<AudioSource>();
    }

    public IEnumerator ScoreReflection(int Answer, int Player1AnswerChoice, int Player2AnswerChoice, int QuestionType, int point)
    {
        if (Player1AnswerChoice == Answer)
        {
            ma.clip = CorrectSound;
            ma.Play();
        }
        else
        {
            ma.clip = WrongSound;
            ma.Play();
        }

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

        if (Player1AnswerChoice == Answer && Player2AnswerChoice == Answer) //둘 다 맞은 경우
        {
            PlayerCorrectEffect.Play();
            RobotCorrectEffect.Play();
            PlayerCorrect.SetActive(true);
            RobotCorrect.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                PlayerScore += 1;
                RobotScore += 1;
                PlayerScoreText.text = PlayerScore.ToString();
                RobotScoreText.text = RobotScore.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else if (Player1AnswerChoice == Answer) //플레이어만 맞은 경우
        {
            PlayerCorrectEffect.Play();
            RobotWrongEffect.Play();
            PlayerCorrect.SetActive(true);
            RobotWrong.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                PlayerScore += 1;
                PlayerScoreText.text = PlayerScore.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else if (Player2AnswerChoice == Answer) //로봇만 맞은 경우
        {
            PlayerWrongEffect.Play();
            RobotCorrectEffect.Play();
            PlayerWrong.SetActive(true);
            RobotCorrect.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                RobotScore += 1;
                RobotScoreText.text = RobotScore.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else //둘 다 틀린 경우
        {
            PlayerWrongEffect.Play();
            RobotWrongEffect.Play();
            PlayerWrong.SetActive(true);
            RobotWrong.SetActive(true);
        }
        yield return new WaitForSeconds(1.3f);
        PlayerCorrect.SetActive(false);
        PlayerWrong.SetActive(false);
        RobotCorrect.SetActive(false);
        RobotWrong.SetActive(false);
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
    }
}
