using UnityEngine;
using Amazon.DynamoDBv2.DataModel;
using UnityEngine.SceneManagement;

public class EquipButton : MonoBehaviour
{
    public bool state;
    public string code;

    public static int ItemToChange;

    MyRoomManager myRoom;

    private void Awake()
    {
        Setup.Setting();
        myRoom = GameObject.Find("MyRoomManager").GetComponent<MyRoomManager>();
    }

    public void EquipButtonClickListener() // 장착 버튼 클릭 시 UserInfo에 정보 저장
    {
        for(int i=0; i<3; i++)
        {
            if (this.name == "Character" + i)
            {
                PlayerPrefs.SetInt("Character", i);
                SaveUserInfo(i, PlayerPrefs.GetInt("Item1"), PlayerPrefs.GetInt("Item2"));
                Setup.LoadingNotice.SetActive(true);
            }
            else if(this.name == "Item" + i)
            {
                ItemToChange = i;
                myRoom.ChoicePanel.SetActive(true);
                myRoom.ItemButton1.interactable = true;
                myRoom.ItemButton2.interactable = true;
            }
        }
    }

    public void ChoiceButtonClick(int num)
    {
        myRoom.ItemButton1.interactable = false;
        myRoom.ItemButton2.interactable = false;
        if (num == 1)
        {
            PlayerPrefs.SetInt("Item1", ItemToChange);
            SaveUserInfo(PlayerPrefs.GetInt("Character"), ItemToChange, PlayerPrefs.GetInt("Item2"));
        }
        else if(num == 2)
        {
            PlayerPrefs.SetInt("Item2", ItemToChange);
            SaveUserInfo(PlayerPrefs.GetInt("Character"),PlayerPrefs.GetInt("Item1"), ItemToChange);
        }
        Setup.LoadingNotice.SetActive(true);
    }

    public void SaveUserInfo(int ChangeChar, int ChangeItem1, int ChangeItem2)
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

        User user1 = new User //업로드 할 내용을 미리 선언해둠(예시)
        {
            user_no = PlayerPrefs.GetString("UserN"),
            GPlogin_status = state,
            user_code = code,
            user_nick = PlayerPrefs.GetString("Nickname"),
            user_exp = PlayerPrefs.GetInt("Exp"),
            own_coin = PlayerPrefs.GetInt("Coin"),
            eq_acc = PlayerPrefs.GetInt("Acc"),
            eq_item1 = ChangeItem1,
            eq_item2 = ChangeItem2,
            present_char = ChangeChar,
            login_status = true
        };
        Setup.context.SaveAsync(user1, (result) =>
        {
            if (result.Exception != null)
                Debug.Log(result.Exception);
        });
        SceneManager.LoadScene("MyRoom");
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
}
