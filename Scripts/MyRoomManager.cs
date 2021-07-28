using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

public class MyRoomManager : MonoBehaviour
{
    public Text UserN;
    public GameObject MyRoom_Item;
    public GameObject Myroom_EquippedItem;
    public Transform c_grid;
    public Transform a_grid;
    public Transform i_grid;

    public GameObject CharPanel;
    public GameObject ItemPanel;
    public GameObject DecoPanel;

    public GameObject ChoicePanel; // 아이템1&2 중 어디에 장착할 건지 묻는 창
    public Image UserImage;

    public GameObject CharButton;
    public GameObject DecoButton;
    public GameObject ItemButton;

    public Button ItemButton1;
    public Button ItemButton2;

    public List<CharList> CharInfo = new List<CharList>();
    public List<ItemList> ItemInfo = new List<ItemList>();

    private void Awake()
    {
        Setup.LoadingNotice.SetActive(true);
        UserN.text = PlayerPrefs.GetString("Nickname");
        ChoicePanel.SetActive(false);
        DecoPanel.SetActive(false);
        ItemPanel.SetActive(false);
        StartCoroutine(CharacterSetting());
        StartCoroutine(ItemSetting());
        ItemButton1.interactable = false;
        ItemButton2.interactable = false;
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
            {
                if (PlayerPrefs.GetInt("Character") == i) //착용 중인 캐릭터일 경우
                    character = Instantiate(Myroom_EquippedItem);
                else
                    character = Instantiate(MyRoom_Item);
                character.name = "Character" + i.ToString();
                character.transform.SetParent(c_grid);
                character.transform.localScale = new Vector3(1f, 1f, 1f);
                character.transform.Find("NameText").GetComponent<Text>().text = CharInfo[i].char_name;
                character.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + CharInfo[i].char_img, typeof(Sprite)) as Sprite; //버튼에 이미지 입히기
                character.transform.Find("CountText").gameObject.SetActive(false);
                if (PlayerPrefs.GetInt("Character") == i) //왼쪽의 유저 이미지도 변경 (현재 장착중인 캐릭터)
                    UserImage.sprite = Resources.Load("Store,Myroom/" + CharInfo[i].char_img, typeof(Sprite)) as Sprite;
            }
        }
        Setup.LoadingNotice.SetActive(false);
    }

    IEnumerator ItemSetting()
    {
        for (int i = 0; i < 3; i++)
        {
            Setup.context.LoadAsync<ItemList>(i.ToString(), (AmazonDynamoDBResult<ItemList> result) =>
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
            GameObject item;
            if (PlayerPrefs.GetInt("i" + i.ToString()) > 0) //1개 이상 소지한 아이템일 경우
            {
                if (PlayerPrefs.GetInt("Item1", 0) == i) //착용 중인 아이템일 경우 (순서 1)
                {
                    ItemButton1.GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + ItemInfo[i].item_img, typeof(Sprite)) as Sprite;
                    item = Instantiate(Myroom_EquippedItem);
                }
                else if (PlayerPrefs.GetInt("Item2", 0) == i) //착용 중인 아이템일 경우 (순서 2)
                {
                    ItemButton2.GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + ItemInfo[i].item_img, typeof(Sprite)) as Sprite;
                    item = Instantiate(Myroom_EquippedItem);
                }
                else
                    item = Instantiate(MyRoom_Item);

                item.name = "Item" + i.ToString();
                item.transform.SetParent(i_grid);
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                item.transform.Find("NameText").GetComponent<Text>().text =ItemInfo[i].item_name;
                item.transform.Find("ItemImage").GetComponent<Image>().sprite = Resources.Load("Store,Myroom/" + ItemInfo[i].item_img, typeof(Sprite)) as Sprite;
                item.transform.Find("CountText").GetComponent<Text>().text = PlayerPrefs.GetInt("i" + i.ToString()).ToString();
            }
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
        DecoPanel.SetActive(false);
        ItemPanel.SetActive(false);
        CharButton.transform.localScale = new Vector2(1.2f, 1.2f);
        DecoButton.transform.localScale = new Vector2(1.0f, 1.0f);
        ItemButton.transform.localScale = new Vector2(1.0f, 1.0f);
    }

    public void AccButtonClick()
    {
        CharPanel.SetActive(false);
        DecoPanel.SetActive(true);
        ItemPanel.SetActive(false);
        CharButton.transform.localScale = new Vector2(1.0f, 1.0f);
        DecoButton.transform.localScale = new Vector2(1.2f, 1.2f);
        ItemButton.transform.localScale = new Vector2(1.0f, 1.0f);
    }

    public void ItemButtonClick()
    {
        CharPanel.SetActive(false);
        DecoPanel.SetActive(false);
        ItemPanel.SetActive(true);
        CharButton.transform.localScale = new Vector2(1.0f, 1.0f);
        DecoButton.transform.localScale = new Vector2(1.0f, 1.0f);
        ItemButton.transform.localScale = new Vector2(1.2f, 1.2f);
    }

    public void CloseChoicePanel()
    {
        ChoicePanel.SetActive(false);
        ItemButton1.interactable = false;
        ItemButton2.interactable = false;
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
