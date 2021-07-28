using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public Text Nick;
    public Text Exp;
    public Text Money;
    public Slider expSlider;
    public Image UserImage;

    private void Awake()
    {
        Nick.text = PlayerPrefs.GetString("Nickname");
        Exp.text = (PlayerPrefs.GetInt("Exp") / 100).ToString();
        Money.text = PlayerPrefs.GetInt("Coin").ToString();
        expSlider.value = PlayerPrefs.GetInt("Exp") % 100;

        if (PlayerPrefs.GetInt("Character", 0) == 0)
            UserImage.sprite = Resources.Load("Store,Myroom/cat_round", typeof(Sprite)) as Sprite;
        else if (PlayerPrefs.GetInt("Character", 0) == 1)
            UserImage.sprite = Resources.Load("Store,Myroom/racoon_round", typeof(Sprite)) as Sprite;
        else if (PlayerPrefs.GetInt("Character", 0) == 2)
            UserImage.sprite = Resources.Load("Store,Myroom/squirrel_round", typeof(Sprite)) as Sprite;
    }

    public void OptionButton_click()
    {
        Setup.OptionPanel.SetActive(true);
    }
}
