using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleLobbyManager : MonoBehaviour {
    public Image Character;
    public GameObject Item1;
    public GameObject Item2;
    public Text Item1CountText;
    public Text Item2CountText;
    public Sprite[] CharacterImage = new Sprite[3];
    public Sprite[] ItemImage = new Sprite[3];

    private void Awake()
    {
        Setup.Setting();
        Character.sprite = CharacterImage[PlayerPrefs.GetInt("Character")];
        int item1 = PlayerPrefs.GetInt("Item1");
        int item2 = PlayerPrefs.GetInt("Item2");

        if ((item1 == -1) || (PlayerPrefs.GetInt("i" + item1.ToString()) <= 0))
            Item1.SetActive(false);
        else
        {
            Item1.GetComponent<Image>().sprite = ItemImage[item1];
            Item1CountText.text = PlayerPrefs.GetInt("i" + item1.ToString()).ToString();
            Item1.SetActive(true);
        }

        if((item2 == -1) || (PlayerPrefs.GetInt("i" + item2.ToString()) <= 0))
            Item2.SetActive(false);
        else
        {
            Item2.GetComponent<Image>().sprite = ItemImage[item2];
            Item2CountText.text = PlayerPrefs.GetInt("i" + item2.ToString()).ToString();
            Item2.SetActive(true);
        }
    }

    public void ToMyroom()
    {
        SceneManager.LoadScene("MyRoom");
    }

    public void MatchButton_click() //매칭방 이동
    {
        SceneManager.LoadScene("RandomBattleMatching");
    }

    public void MainButton_click()
    {
        SceneManager.LoadScene("Main");
    }
}
