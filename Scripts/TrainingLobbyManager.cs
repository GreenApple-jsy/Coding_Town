using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TrainingLobbyManager : MonoBehaviour
{

    public GameObject QuestionPanel;
    public Image UserImage;

    private void Awake()
    {
        Setup.Setting();
        //버튼들 보유 아이템이랑 캐릭터 이미지로 바꿔주기 필요

        QuestionPanel.SetActive(false);
        
        if(PlayerPrefs.GetInt("Character") == 0)
            UserImage.sprite = Resources.Load("Store,Myroom/cat", typeof(Sprite)) as Sprite;
        else if (PlayerPrefs.GetInt("Character") == 1)
            UserImage.sprite = Resources.Load("Store,Myroom/raccoon", typeof(Sprite)) as Sprite;
        else
            UserImage.sprite = Resources.Load("Store,Myroom/squirrel", typeof(Sprite)) as Sprite;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void TypeButton()
    {
        QuestionPanel.SetActive(true);
    }

    public void ToMyroom()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void MainButton_click()
    {
        SceneManager.LoadScene("Main");
    }

}
