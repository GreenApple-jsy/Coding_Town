using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsInTrainingLobby : MonoBehaviour
{
    public GameObject CharacterButton;
    public GameObject FormAButton;
    public GameObject FormBButton;
    public GameObject FormCButton;
    public GameObject Item1Button;
    public GameObject Item2Button;
    public GameObject MainButton;

    public void CharacterButton_click()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void Item1Button_click()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void Item2Button_click()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void FormAButton_click()
    {
        SceneManager.LoadScene("TrainingRoom"); // A형식에 해당하는 문제 불러옴
    }

    public void FormBButton_click()
    {
        SceneManager.LoadScene("TrainingRoom"); // B형식에 해당하는 문제 불러옴
    }

    public void FormCButton_click()
    {
        SceneManager.LoadScene("TrainingRoom"); // C형식에 해당하는 문제 불러옴
    }

    public void MainButton_click()
    {
        SceneManager.LoadScene("Main");
    }

}
