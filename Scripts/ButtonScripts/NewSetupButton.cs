using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class NewSetupButton : MonoBehaviour
{
    public int CharacterNumber = 0;
    public InputField NicknameInput;
    public string Nickname;
    public Text Check;
    public Button Start;
    public Button Character1;
    public Button Character2;
    public Button Back;

    public Image UserImage;

    private void Awake()
    {
        ButtonInteraction(true);
        Setup.Setting();
    }

    public void ButtonInteraction(bool status)
    {
        NicknameInput.interactable = status;
        Character1.interactable = status;
        Character2.interactable = status;
        Start.interactable = status;
        Back.interactable = status;
    }

    public void StartButton()
    {
        ButtonInteraction(false);
        Nickname = NicknameInput.text;
        NickNameCheck(Nickname);
    }

    public void NickNameCheck(string Nick) //닉네임 중복 체크 함수
    {
        if (Nick == "") //닉네임을 입력하지 않은 경우
        {
            Debug.Log("닉네임을 입력하지 않음");
            ButtonInteraction(true);
            return;
        }
        var request = new ScanRequest
        {
            TableName = "user_info",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":val", new AttributeValue { S = Nick}}},
            FilterExpression = "user_nick = :val",
            ProjectionExpression = "user_no"
        };
        Setup.DBclient.ScanAsync(request, (result) => {
            if (result.Exception != null)
            {
                Debug.Log("정보 받아오기 실패"); //오프라인이거나 빈칸이거나 등등....
                ButtonInteraction(true);
            }
            else
            {
                if (result.Response.Count == 0) //중복이 아닌 닉네임일 경우
                {
                    if (SceneManager.GetActiveScene().name == "GPGSNewSetup")
                    {
                        PlayerPrefs.SetInt("GooglePlayLogin", 1);
                        PlayerPrefs.SetInt("GuestLogin", 0);
                        Registration(true, Nickname, CharacterNumber);
                    }
                    else
                    {
                        PlayerPrefs.SetInt("GooglePlayLogin", 0);
                        PlayerPrefs.SetInt("GuestLogin", 1);
                        Registration(false, Nickname, CharacterNumber);
                    }
                }
                else //중복 닉네임인 경우
                {
                    Debug.Log("중복 닉네임입니다. 다른 닉네임을 입력하세요");
                    ButtonInteraction(true);
                }
            }
        });
    }

    public void Registration(bool GPlogin, string Nick, int Character)
    {
        int userNo = -2;
        Management temp;
        Setup.context.LoadAsync<Management>("next_userNo", (AmazonDynamoDBResult<Management> r) =>
        {
            if (r.Exception != null)
            {
                Debug.LogException(r.Exception);
                return;
            }
            temp = r.Result;
            userNo = temp.value;

            string code;
            if (GPlogin)
                code = PlayerPrefs.GetString("GPGSidToken", "");
            else
                code = PlayerPrefs.GetString("GuestCode", "");

            PlayerPrefs.SetString("UserN", userNo.ToString());
            PlayerPrefs.SetString("Nickname", Nick);
            PlayerPrefs.SetInt("Character", Character);
            PlayerPrefs.SetInt("Exp", 0);
            PlayerPrefs.SetInt("Coin", 0);
            PlayerPrefs.SetInt("Pet", 0);
            PlayerPrefs.SetInt("Acc", 0);
            PlayerPrefs.SetInt("Item1", -1);
            PlayerPrefs.SetInt("Item2", -1);
            for (int i = 0; i < 3; i++) //일단 3개해둠
            {
                if (i == Character)
                    PlayerPrefs.SetInt("c" + i.ToString(), 1);
                else
                    PlayerPrefs.SetInt("c" + i.ToString(), 0);
            }
            User user = new User
            {
                user_no = userNo.ToString(),
                GPlogin_status = GPlogin,
                user_code = code,
                user_nick = Nick,
                user_exp = 0,
                own_coin = 0,
                eq_acc = -1,
                eq_item1 = -1,
                eq_item2 = -1,
                present_char = Character,
                login_status = false
            };
            Setup.context.SaveAsync(user, (re) =>
            {
                if (re.Exception == null)
                {
                    Debug.Log("회원 생성 성공");
                    Management update = new Management
                    {
                        type = "next_userNo",
                        value = userNo + 1
                    };
                    Setup.context.SaveAsync<Management>(update, (res) =>
                    {
                        if (res.Exception == null)
                        {
                            Debug.Log("회원번호 증가 업데이트 성공");
                            OwnChar oc = new OwnChar //기본 캐릭터를 캐릭터 소지 테이블에 삽입
                            {
                                user_no = userNo.ToString(),
                                char_no = Character.ToString()
                            };
                            Setup.context.SaveAsync(oc, (r3) =>
                            {
                                if (r3.Exception == null)
                                {
                                    Debug.Log("캐릭터 소유 정보 저장 성공");
                                    SceneManager.LoadScene("Square");
                                }
                                else
                                    Debug.Log(r3.Exception);
                            });
                        }
                    });
                }
                else //캐릭터 생성 오류
                    Debug.Log(re.Exception);
            });
        }, null);
    }

    public void Character1Button()
    {
        CharacterNumber = 0;
        UserImage.sprite = Resources.Load("Store,Myroom/cat", typeof(Sprite)) as Sprite;
        //1캐릭터 보여주기
    }

    public void Character2Button()
    {
        CharacterNumber = 1;
        UserImage.sprite = Resources.Load("Store,Myroom/raccoon", typeof(Sprite)) as Sprite;
        //2캐릭터 보여주기
    }

    public void BackButton()
    {
        SceneManager.LoadScene("Entrance");
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

    [DynamoDBTable("Management")]
    public class Management
    {
        [DynamoDBHashKey] // Hash key.
        public string type { get; set; }
        [DynamoDBProperty]
        public int value { get; set; }
    }

    [DynamoDBTable("own_char")]
    public class OwnChar
    {
        [DynamoDBHashKey] // Hash key.
        public string user_no { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey] // Hash key.
        public string char_no { get; set; }
    }
}