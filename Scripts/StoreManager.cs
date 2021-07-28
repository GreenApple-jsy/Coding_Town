using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

public class StoreManager : MonoBehaviour
{
    public Text Nick;
    public Text Coin;
    public Text ItemCountText;

    public GameObject Item;
    public GameObject own_Item;
    public GameObject AlarmPanel;
    public GameObject ItemAlarmPanel;
    public GameObject NoMoneyPanel;
    public GameObject CharPanel;
    public GameObject AccPanel;
    public GameObject ItemPanel;
    public GameObject CharButton;
    public GameObject AccButton;
    public GameObject ItemButton;
    public GameObject NotYetTextObject;

    public Image UserImage;

    public Transform c_grid;
    public Transform a_grid;
    public Transform i_grid;

    public List<CharList> CharInfo = new List<CharList>();
    public List<ItemList> ItemInfo = new List<ItemList>();

    private void Awake()
    {
        Setup.LoadingNotice.SetActive(true);
        Setup.Setting();
        Nick.text = PlayerPrefs.GetString("Nickname");
        Coin.text = PlayerPrefs.GetInt("Coin").ToString();
        AlarmPanel.SetActive(false);
        ItemAlarmPanel.SetActive(false);
        NoMoneyPanel.SetActive(false);
        AccPanel.SetActive(false);
        ItemPanel.SetActive(false);
        NotYetTextObject.SetActive(false);
        StartCoroutine(CharacterSetting());
        StartCoroutine(ItemSetting());
    }

    IEnumerator CharacterSetting()
    {
        for (int i = 0; i < 3; i++)
        {
            Setup.context.LoadAsync<CharList>(i.ToString(), (AmazonDynamoDBResult<CharList> result) => //CharList테이블의 PK인 char_no가 num인 값을 찾는 경우
            {
                if (result.Exception != null)
                {
                    Debug.LogException(result.Exception);
                    return;
                }
                CharInfo.Add(result.Result);
            }, null);
            while (CharInfo.Count < i + 1)
                yield return null;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject character;
            if (PlayerPrefs.GetInt("c" + i.ToString()) == 1) //소지한 캐릭터일 경우
                character = Instantiate(own_Item);
            else //소지하지 않은 캐릭터일 경우
            {
                character = Instantiate(Item);
                character.transform.Find("CountText").gameObject.SetActive(false);
                character.transform.Find("CostText").GetComponent<Text>().text = CharInfo[i].char_cost.ToString();
            }
            character.name = "Character" + i.ToString();
            character.transform.SetParent(c_grid);
            character.transform.localScale = new Vector3(1f, 1f, 1f);

            character.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + CharInfo[i].char_img, typeof(Sprite)) as Sprite; //버튼에 이미지 입히기

            if (PlayerPrefs.GetInt("Character") == i) //왼쪽의 유저 이미지도 변경 (현재 장착중인 캐릭터)
                UserImage.sprite = Resources.Load("Store,Myroom/" + CharInfo[i].char_img, typeof(Sprite)) as Sprite;
        }
        Setup.LoadingNotice.SetActive(false);
    }

    IEnumerator ItemSetting()
    {
        for (int i = 0; i < 3; i++)
        {
            Setup.context.LoadAsync<ItemList>(i.ToString(), (AmazonDynamoDBResult<ItemList> result) => //ItemList테이블의 PK인 item_no가 num인 값을 찾는 경우
            {
                if (result.Exception != null)
                {
                    Debug.LogException(result.Exception);
                    return;
                }
                ItemInfo.Add(result.Result);
            }, null);
            while (ItemInfo.Count < i + 1)
                yield return null;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject item = Instantiate(Item);
            item.name = "Item" + i.ToString();
            item.transform.SetParent(i_grid);
            item.transform.localScale = new Vector3(1f, 1f, 1f);
            item.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + ItemInfo[i].item_img, typeof(Sprite)) as Sprite;
            item.transform.Find("CostText").GetComponent<Text>().text = ItemInfo[i].item_cost.ToString();

            if (PlayerPrefs.GetInt("i" + i.ToString()) > 0)
                item.transform.Find("CountText").GetComponent<Text>().text = PlayerPrefs.GetInt("i" + i.ToString()).ToString();
        }
        Setup.LoadingNotice.SetActive(false);
    }

    public void BackButton() // 광장으로 돌아감
    {
        SceneManager.LoadScene("Main");
    }

    public void CharButtonClick()
    {
        CharPanel.SetActive(true);
        NotYetTextObject.SetActive(false);
        AccPanel.SetActive(false);
        ItemPanel.SetActive(false);
        CharButton.transform.localScale = new Vector2(1.2f, 1.2f);
        AccButton.transform.localScale = new Vector2(1.0f, 1.0f);
        ItemButton.transform.localScale = new Vector2(1.0f, 1.0f);
    }

    public void ItemButtonClick()
    {
        NotYetTextObject.SetActive(false);
        CharPanel.SetActive(false);
        AccPanel.SetActive(false);
        ItemPanel.SetActive(true);
        CharButton.transform.localScale = new Vector2(1.0f, 1.0f);
        AccButton.transform.localScale = new Vector2(1.0f, 1.0f);
        ItemButton.transform.localScale = new Vector2(1.2f, 1.2f);
    }

    public void DecoButtonClick()
    {
        NotYetTextObject.SetActive(true);
        CharPanel.SetActive(false);
        AccPanel.SetActive(true);
        ItemPanel.SetActive(false);
        CharButton.transform.localScale = new Vector2(1.0f, 1.0f);
        AccButton.transform.localScale = new Vector2(1.2f, 1.2f);
        ItemButton.transform.localScale = new Vector2(1.0f, 1.0f);
    }

    [DynamoDBTable("char_list")]
    public class CharList
    {
        [DynamoDBHashKey] // Hash key.
        public string char_no { get; set; }
        [DynamoDBProperty]
        public int char_cost { get; set; }
        [DynamoDBProperty]
        public string char_img { get; set; }
        [DynamoDBProperty]
        public string char_name { get; set; }
    }

    [DynamoDBTable("item_list")]
    public class ItemList
    {
        [DynamoDBHashKey] // Hash key.
        public string item_no { get; set; }
        [DynamoDBProperty]
        public int item_cost { get; set; }
        [DynamoDBProperty]
        public string item_effect { get; set; }
        [DynamoDBProperty]
        public string item_img { get; set; }
        [DynamoDBProperty]
        public string item_name { get; set; }
    }
}
