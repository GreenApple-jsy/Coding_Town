using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Amazon.DynamoDBv2.DataModel;

public class BuyButton : MonoBehaviour
{
    public bool state;
    public string code;
    public string whatName;
    public static int itemCount = 1;

    StoreManager store;
    BuyDecisionButton buy;

    private void Awake()
    {
        Setup.Setting();
        store = GameObject.Find("StoreManager").GetComponent<StoreManager>();
        buy = GameObject.Find("StoreManager").GetComponent<BuyDecisionButton>();
    }

    public void SaveUserInfo(int ChangeCoin)
    {
        if (PlayerPrefs.GetInt("GuestLogin") == 1) // 게스트 로그인의 경우
        {
            state = false;
            code = PlayerPrefs.GetString("GuestCode");
        }
        else if (PlayerPrefs.GetInt("GooglePlayLogin") == 1) // 구글플레이 로그인의 경우
        {
            state = true;
            code = PlayerPrefs.GetString("GPGSidToken");
        }

        User user1 = new User
        {
            user_no = PlayerPrefs.GetString("UserN"),
            GPlogin_status = state,
            user_code = code,
            user_nick = PlayerPrefs.GetString("Nickname"),
            user_exp = PlayerPrefs.GetInt("Exp"),
            own_coin = ChangeCoin,
            eq_acc = PlayerPrefs.GetInt("Acc"),
            eq_item1 = PlayerPrefs.GetInt("Item1"),
            eq_item2 = PlayerPrefs.GetInt("Item2"),
            present_char = PlayerPrefs.GetInt("Character"),
            login_status = true
        };
        Setup.context.SaveAsync(user1, (result) =>
        {
            if (result.Exception != null)
                Debug.Log(result.Exception);
        });
    }

    public void SaveOwnCharInfo(string ChangeNo)
    {
        OwnChar ownchar1 = new OwnChar
        {
            user_no = PlayerPrefs.GetString("UserN"),
            char_no = ChangeNo
        };
        Setup.context.SaveAsync(ownchar1, (result) =>
        {
            if (result.Exception == null)
                Debug.Log("new ownchar data saved");
            else
                Debug.Log(result.Exception);
        });
    }

    public void SaveOwnItemInfo(string ChangeNo, int ChangeNum)
    {
        OwnItem ownitem1 = new OwnItem
        {
            user_no = PlayerPrefs.GetString("UserN"),
            item_no = ChangeNo,
            item_num = ChangeNum
        };
        Setup.context.SaveAsync(ownitem1, (result) =>
        {
            if (result.Exception != null)
                Debug.Log(result.Exception);
        });
    }

    public void BuyButtonClickListener()
    {
        whatName = this.transform.parent.name;
        if(store.ItemPanel.activeSelf == true)
            StartCoroutine(ShowItemAlarmPanel());
        else
            StartCoroutine(ShowAlarmPanel());
    }

    IEnumerator ShowAlarmPanel()
    {
        store.AlarmPanel.SetActive(true);
        while (store.AlarmPanel.activeSelf == true && buy.ans < 0)
            yield return null;

        for (int i = 0; i < 3; i++)
        {
            if ((buy.ans == 1) && (whatName == "Character" + i))
            {
                if ((PlayerPrefs.GetInt("Coin") - store.CharInfo[i].char_cost) > 0)
                {
                    SaveUserInfo(PlayerPrefs.GetInt("Coin") - store.CharInfo[i].char_cost);
                    SaveOwnCharInfo(i.ToString());
                    PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin") - store.CharInfo[i].char_cost);
                    PlayerPrefs.SetInt("c" + i.ToString(), 1);
                }
                else
                {
                    Debug.Log("금액이 부족합니다.");
                    store.NoMoneyPanel.SetActive(true);
                }
                buy.ans = -1;
            }
        }
        store.AlarmPanel.SetActive(false);
        SceneManager.LoadScene("Store");
    }

    IEnumerator ShowItemAlarmPanel()
    {
        store.ItemAlarmPanel.SetActive(true);
        while (store.ItemAlarmPanel.activeSelf == true && buy.ans < 0)
            yield return null;

        for (int i = 0; i < 3; i++)
        {
            if ((buy.ans == 1) && (whatName == "Item" + i))
            {
                if ((PlayerPrefs.GetInt("Coin") - (store.ItemInfo[i].item_cost) * int.Parse(store.ItemCountText.text)) > 0)
                {
                    SaveUserInfo(PlayerPrefs.GetInt("Coin") - (store.ItemInfo[i].item_cost) * int.Parse(store.ItemCountText.text));
                    SaveOwnItemInfo(i.ToString(), PlayerPrefs.GetInt("i" + i.ToString()) + int.Parse(store.ItemCountText.text));
                    PlayerPrefs.SetInt("Coin", PlayerPrefs.GetInt("Coin") - (store.ItemInfo[i].item_cost) * int.Parse(store.ItemCountText.text));
                    PlayerPrefs.SetInt("i" + i.ToString(), PlayerPrefs.GetInt("i" + i.ToString()) + int.Parse(store.ItemCountText.text));
                }
                else
                {
                    Debug.Log("금액이 부족합니다.");
                    store.NoMoneyPanel.SetActive(true);
                }
                buy.ans = -1;
                itemCount = 1;
            }
        }
        store.ItemAlarmPanel.SetActive(false);
        SceneManager.LoadScene("Store");
    }
    

    public void Up()
    {
        itemCount += 1;
        store.ItemCountText.text = itemCount.ToString();
    }

    public void Down()
    {
        if(itemCount > 1)
        {
            itemCount -= 1;
            store.ItemCountText.text = itemCount.ToString();
        }

        else
        {
            itemCount = 1;
            store.ItemCountText.text = itemCount.ToString();

        }
    }

    public void Okay()
    {
        store.NoMoneyPanel.SetActive(false);
    }

    [DynamoDBTable("user_info")]
    public class User
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBProperty]
        public bool GPlogin_status { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Secondary Hash key.
        public string user_code { get; set; }
        [DynamoDBProperty]
        public string user_nick { get; set; }
        [DynamoDBProperty]
        public int user_exp { get; set; }
        [DynamoDBProperty]
        public int own_coin { get; set; }
        [DynamoDBProperty]
        public int eq_acc { get; set; }
        [DynamoDBProperty]
        public int eq_item1 { get; set; }
        [DynamoDBProperty]
        public int eq_item2 { get; set; }
        [DynamoDBProperty]
        public int present_char { get; set; }
        [DynamoDBProperty]
        public bool login_status { get; set; }
    }

    [DynamoDBTable("own_char")]
    public class OwnChar
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Hash key.
        public string char_no { get; set; }
    }

    [DynamoDBTable("own_item")]
    public class OwnItem
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Hash key.
        public string item_no { get; set; }
        [DynamoDBProperty]
        public int item_num { get; set; }
    }

}
