using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoomScoreManager : MonoBehaviour
{
    public Text Player1ScoreText;
    public Text Player2ScoreText;

    public static int Player1Score;
    public static int Player2Score;

    public Image TypeAanswer;
    public Image TypeBanswer;
    public Image TypeCanswer;

    public GameObject Player1Correct;
    public GameObject Player1Wrong;
    public GameObject Player2Correct;
    public GameObject Player2Wrong;

    public Button[] TypeAButton = new Button[4];
    public Button[] TypeBButton = new Button[2];
    public Button[] TypeCButton = new Button[3];

    public AudioClip CorrectSound;
    public AudioClip WrongSound;
    public AudioSource ma;

    public ParticleSystem Player1CorrectEffect;
    public ParticleSystem Player1WrongEffect;
    public ParticleSystem Player2CorrectEffect;
    public ParticleSystem Player2WrongEffect;

    private void Awake()
    {
        Player1Score = 0;
        Player2Score = 0;
        Player1ScoreText.text = "0";
        Player2ScoreText.text = "0";
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
        Player1Correct.SetActive(false);
        Player1Wrong.SetActive(false);
        Player2Correct.SetActive(false);
        Player2Wrong.SetActive(false);
        ma = this.GetComponent<AudioSource>();
    }

    public IEnumerator ScoreReflection(bool Player1, int Answer, int Player1AnswerChoice, int Player2AnswerChoice, int QuestionType, int point)
    {
        if (Player1)
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
        }
        else
        {
            if (Player2AnswerChoice == Answer)
            {
                ma.clip = CorrectSound;
                ma.Play();
            }
            else
            {
                ma.clip = WrongSound;
                ma.Play();
            }
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
            Player1CorrectEffect.Play();
            Player2CorrectEffect.Play();
            Player1Correct.SetActive(true);
            Player2Correct.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                Player1Score += 1;
                Player2Score += 1;
                Player1ScoreText.text = Player1Score.ToString();
                Player2ScoreText.text = Player2Score.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else if (Player1AnswerChoice == Answer) //플레이어1만 맞은 경우
        {
            Player1CorrectEffect.Play();
            Player2WrongEffect.Play();
            Player1Correct.SetActive(true);
            Player2Wrong.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                Player1Score += 1;
                Player1ScoreText.text = Player1Score.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else if (Player2AnswerChoice == Answer) //플레이어2만 맞은 경우
        {
            Player1WrongEffect.Play();
            Player2CorrectEffect.Play();
            Player1Wrong.SetActive(true);
            Player2Correct.SetActive(true);
            for (int i = 0; i < point; i++)
            {
                Player2Score += 1;
                Player2ScoreText.text = Player2Score.ToString();
                yield return new WaitForSeconds(0.0003f);
            }
        }
        else //둘 다 틀린 경우
        {
            Player1WrongEffect.Play();
            Player2WrongEffect.Play();
            Player1Wrong.SetActive(true);
            Player2Wrong.SetActive(true);
        }
        yield return new WaitForSeconds(1.3f);
        Player1Correct.SetActive(false);
        Player1Wrong.SetActive(false);
        Player2Correct.SetActive(false);
        Player2Wrong.SetActive(false);
        TypeAanswer.gameObject.SetActive(false);
        TypeBanswer.gameObject.SetActive(false);
        TypeCanswer.gameObject.SetActive(false);
    }
}
