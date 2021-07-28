using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class EntranceManager : MonoBehaviour
{
    string GPGSid;
    string PlayerNo;
    public Button GPGSlogin;
    public Button Guestlogin;
    public Button Guestjoin;

    private void Awake()
    {
        ButtonInteraction(true);
        Setup.Setting();
    }

    public void ButtonInteraction(bool status)
    {
        GPGSlogin.interactable = status;
        Guestlogin.interactable = status;
        Guestjoin.interactable = status;
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

    public void GooglePlayGameLoginButton()
    {
        ButtonInteraction(false);
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestIdToken().RequestServerAuthCode(false).Build();
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        if (! Social.localUser.authenticated)
        {
            Social.localUser.Authenticate((bool Success) =>
            {
                if (Success)
                    StartCoroutine(GPGSprocedure());
                else
                {
                    Debug.Log("구글 플레이 게임 로그인 실패(구글 플레이 게임 설치 확인 필요)");
                    ButtonInteraction(true);
                }
            });
        }
        else
        {
            StartCoroutine(GPGSprocedure());
        }
    }

    IEnumerator GPGSprocedure()
    {
        GPGSid = Social.localUser.id;
        yield return null;
        while (String.IsNullOrEmpty(GPGSid) || GPGSid == "1000")
        {
            GPGSid = Social.localUser.id;
            Debug.Log("계속 도는 중  : " + GPGSid);
            yield return null;
        }
        Debug.Log(GPGSid);
        var request = new ScanRequest
        {
            TableName = "user_info",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        { ":val", new AttributeValue {S = GPGSid}}},
            FilterExpression = "user_code = :val",
            ProjectionExpression = "user_no"
        };
        Debug.Log("스캔 시작 전");
        Setup.DBclient.ScanAsync(request, (result) => {
            if (result.Exception != null)
            {
                ButtonInteraction(true);
                Debug.Log("정보 받아오기 실패");
            }
            else
            {
                Debug.Log("스캔 else 들어옴");
                if (result.Response.Count == 0) //처음 가입하는 이메일일 경우
                {
                    Debug.Log("처음 가입 id토큰");
                    PlayerPrefs.SetString("GPGSidToken", GPGSid);
                    SceneManager.LoadScene("GPGSNewSetup");
                }
                else //이미 해당하는 id토큰의 회원정보가 존재할 경우
                {
                    Debug.Log("이미 존재하는 id토큰");
                    Dictionary<string, AttributeValue> item;
                    item = result.Response.Items[0];
                    AttributeValue value = item["user_no"];
                    PlayerNo = value.S;
                    GPGSUserInfoLoad(); //회원 정보 받아오기
                }
            }
        });
    }

    public void GPGSUserInfoLoad()
    {
        Debug.Log("유저인포 로드 들어옴");
        User user;
        Setup.context.LoadAsync<User>(PlayerNo, (AmazonDynamoDBResult<User> r) =>
        {
            if (r.Exception != null)
            {
                Debug.LogException(r.Exception);
                return;
            }
            PlayerPrefs.SetInt("GooglePlayLogin", 1);
            PlayerPrefs.SetInt("GuestLogin", 0);
            user = r.Result;
            PlayerPrefs.SetString("UserN", PlayerNo);
            PlayerPrefs.SetString("GPGSidToken", GPGSid);
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
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = PlayerNo } } },
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
                        Debug.Log("유저인포 로드 끝");
                        SceneManager.LoadScene("Square");
                    }
                });
            });
        }, null);
        ButtonInteraction(false);
    }

    public void EnterGuestCodeButton()
    {
        SceneManager.LoadScene("EnterGuestCode");
    }

    public void GuestJoinButton()
    {
        SceneManager.LoadScene("GuestCodeConfirm");
    }
}
