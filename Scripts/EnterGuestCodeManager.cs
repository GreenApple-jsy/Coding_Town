using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class EnterGuestCodeManager : MonoBehaviour {
    public InputField GuestCodeField;
    public string PlayerNo;
    public string GuestCode;
    public Text NoticeText;
    public Button OK;
    public Button Back;

    private void Awake()
    {
        OK.interactable = true;
        Back.interactable = true;
        Setup.Setting();
    }

    public void BackButton()
    {
        SceneManager.LoadScene("Entrance");
    }

    public void OKButton()
    {
        Setup.LoadingNotice.SetActive(true);
        OK.interactable = false;
        Back.interactable = false;
        GuestCode = GuestCodeField.text;
        var request = new ScanRequest
        {
            TableName = "user_info",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":val", new AttributeValue { S = GuestCode}}},
            FilterExpression = "user_code = :val",
            ProjectionExpression = "user_no"
        };
        Setup.DBclient.ScanAsync(request, (result) => {
            if (result.Exception != null)
            {
                NoticeText.text = "빈 칸 입력은 안됩니다"; //빈 칸이거나 오프라인이거나 등등....
                Setup.LoadingNotice.SetActive(false);
                OK.interactable = true;
                Back.interactable = true;
            }
            else
            {
                if (result.Response.Count == 0) //존재하지 않는 게스트코드일 경우
                {
                    NoticeText.text = "게스트코드를 다시 확인하세요";
                    Setup.LoadingNotice.SetActive(false);
                    OK.interactable = true;
                    Back.interactable = true;
                }
                else //해당하는 게스트코드를 찾은 경우
                {
                    Dictionary<string, AttributeValue> item;
                    item = result.Response.Items[0];
                    AttributeValue value = item["user_no"];
                    PlayerNo = value.S;
                    GuestUserInfoLoad(); //회원 정보 받아오기
                }     
            }
        });
    }

    public void GuestUserInfoLoad()
    {
        User user;
        Setup.context.LoadAsync<User>(PlayerNo, (AmazonDynamoDBResult<User> result) =>
        {
            if (result.Exception != null)
            {
                Debug.LogException(result.Exception);
                return;
            }
            else
            {
                PlayerPrefs.SetInt("GuestLogin", 1);
                PlayerPrefs.SetInt("GooglePlayLogin", 0);
                user = result.Result;
                PlayerPrefs.SetString("UserN", PlayerNo);
                PlayerPrefs.SetString("GuestCode", GuestCode);
                PlayerPrefs.SetString("Nickname", user.user_nick);
                PlayerPrefs.SetInt("Character", user.present_char);
                PlayerPrefs.SetInt("Exp", user.user_exp);
                PlayerPrefs.SetInt("Coin", user.own_coin);
                PlayerPrefs.SetInt("Acc", user.eq_acc);
                PlayerPrefs.SetInt("Item1", user.eq_item1);
                PlayerPrefs.SetInt("Item2", user.eq_item2);

                var request = new ScanRequest //보유중인 캐릭터 목록 받아오기
                {
                    TableName = "own_char",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>{{":val", new AttributeValue { S = PlayerNo } }},
                    FilterExpression = "user_no = :val",
                    ProjectionExpression = "char_no"
                };

                Setup.DBclient.ScanAsync(request, (r1) => {
                    if (r1.Exception != null)
                        Debug.Log("캐릭터 소지 정보 받아오기 실패");
                    else
                    {
                        if (r1.Response.Count == 0)
                        {
                            Debug.Log("가지고 있는 캐릭터 없음");
                        }
                        else
                        {
                            foreach (Dictionary<string, AttributeValue> item in r1.Response.Items) //찾은 값들에 대하여
                            {
                                foreach (var d in item)
                                {
                                    AttributeValue value = d.Value;
                                    PlayerPrefs.SetInt("c" + value.S, 1);
                                }
                            }
                        }
                    }

                    request = new ScanRequest //보유중인 아이템 목록 받아오기
                    {
                        TableName = "own_item",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = PlayerNo } } },
                        FilterExpression = "user_no = :val",
                        ProjectionExpression = "item_no, item_num"
                    };

                    Setup.DBclient.ScanAsync(request, (r2) => {
                        if (r2.Exception != null)
                            Debug.Log("아이템 소지 정보 받아오기 실패");
                        else
                        {
                            if (r2.Response.Count == 0)
                            {
                                Debug.Log("가지고 있는 아이템 없음");
                            }
                            else
                            {
                                foreach (Dictionary<string, AttributeValue> item in r2.Response.Items) //찾은 값들에 대하여
                                {
                                    int item_num = 0;
                                    foreach (var d in item)
                                    {
                                        AttributeValue value = d.Value;
                                        if (value.S != null)
                                        {
                                            if (item_num != 0)
                                            {
                                                PlayerPrefs.SetInt("i" + value.S, item_num);
                                                item_num = 0;
                                            }
                                        }
                                        if (value.N != null)
                                            item_num = int.Parse(value.N);
                                    }
                                }
                            }
                            Setup.LoadingNotice.SetActive(false);
                            NoticeText.text = "로그인 완료!";
                            SceneManager.LoadScene("Square");
                        }
                    });
                });
            }
        }, null);
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
        [DynamoDBHashKey] // Hash key.
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
