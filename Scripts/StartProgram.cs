using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class StartProgram : MonoBehaviour
{
    public string NextSceneName;
    public Slider LoadingSlider;

    private void Awake()
    {
        Setup.Setting();
    }

    public void Exit()
    {
        /*using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            AndroidJavaObject intent = pm.Call<AndroidJavaObject>("getLaunchIntentForPackage", Application.identifier);
            intent.Call<AndroidJavaObject>("setFlags", 0x20000000);//Intent.FLAG_ACTIVITY_SINGLE_TOP

            AndroidJavaClass pendingIntent = new AndroidJavaClass("android.app.PendingIntent");
            AndroidJavaObject contentIntent = pendingIntent.CallStatic<AndroidJavaObject>("getActivity", currentActivity, 0, intent, 0x8000000); //PendingIntent.FLAG_UPDATE_CURRENT = 134217728 [0x8000000]
            AndroidJavaObject alarmManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "alarm");
            AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
            long currentTime = system.CallStatic<long>("currentTimeMillis");
            alarmManager.Call("set", 1, currentTime + 1000, contentIntent); // android.app.AlarmManager.RTC = 1 [0x1]

            Debug.LogError("alarm_manager set time " + currentTime + 1000);
            currentActivity.Call("finish");

            AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
            int pid = process.CallStatic<int>("myPid");
            process.CallStatic("killProcess", pid);
        }*/
        Application.Quit(0);
    }

    IEnumerator Loading()
    {
        for (float i = 0.0f; i <= 1.0f; i += 0.01f)
        {
            LoadingSlider.value = i;
            yield return 0.025f;
        }
        while (String.IsNullOrEmpty(NextSceneName))
            yield return null;
        SceneManager.LoadScene(NextSceneName);
    }

    public void Start()
    {
        if (PlayerPrefs.GetInt("ConnectionSetup", 0) == 0) //게임 첫 실행 시, cognito 연결을(lock오류) 위해 게임 다시 시작
        {
            var request = new ScanRequest
            {
                TableName = "user_info",
                ProjectionExpression = "user_no"
            };
            Setup.DBclient.ScanAsync(request, (result) => { });
            Debug.Log("앱 재실행");
            PlayerPrefs.SetInt("ConnectionSetup", 1);
            Invoke("Exit", 5f);
        }
        else
        {
            StartCoroutine(Loading());
            if ((PlayerPrefs.GetInt("GuestLogin", 0) == 0) && (PlayerPrefs.GetInt("GooglePlayLogin", 0) == 0)) //첫 실행 유저
                NextSceneName = "Entrance";

            else if ((PlayerPrefs.GetInt("GooglePlayLogin", 0) == 1) && (PlayerPrefs.GetInt("GuestLogin", 0) == 0)) //구글 로그인을 한 적 있는 유저
            {
                PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(false).Build();
                PlayGamesPlatform.InitializeInstance(config);
                PlayGamesPlatform.Activate();

                if (!Social.localUser.authenticated)
                {
                    Social.localUser.Authenticate((bool Success) =>
                    {
                        if (Success) //구글 로그인 성공한 경우
                        {
                            var request = new ScanRequest
                            {
                                TableName = "user_info",
                                ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = PlayerPrefs.GetString("GPGSidToken") } } },
                                FilterExpression = "user_code = :val",
                                ProjectionExpression = "user_no"
                            };
                            Setup.DBclient.ScanAsync(request, (result) =>
                            {
                                if (result.Exception != null)
                                    Debug.Log("정보 받아오기 실패");
                                else
                                {
                                    if (result.Response.Count == 0) //존재하지 않는 id토큰일 경우
                                    {
                                        Debug.Log("해당 id토큰 회원 정보 찾기 실패");
                                        PlayerPrefs.SetInt("GooglePlayLogin", 0); //구글 로그인 기록 초기화
                                        NextSceneName = "Entrance";
                                    }
                                    else //해당 id토큰 회원 있음
                                        NextSceneName = "Square";
                                }
                            });
                        }
                        else //구글 로그인 실패
                        {
                            PlayerPrefs.SetInt("GooglePlayLogin", 0); //구글 로그인 기록 초기화
                            Debug.Log("구글 플레이 게임 로그인 실패");
                            NextSceneName = "Entrance";
                        }
                    });
                }
                else
                {
                    var request = new ScanRequest
                    {
                        TableName = "user_info",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = PlayerPrefs.GetString("GPGSidToken") } } },
                        FilterExpression = "user_code = :val",
                        ProjectionExpression = "user_no"
                    };
                    Setup.DBclient.ScanAsync(request, (result) =>
                    {
                        if (result.Exception != null)
                            Debug.Log("정보 받아오기 실패");
                        else
                        {
                            if (result.Response.Count == 0) //존재하지 않는 id토큰일 경우
                            {
                                Debug.Log("해당 id토큰 회원 정보 찾기 실패");
                                PlayerPrefs.SetInt("GooglePlayLogin", 0); //구글 로그인 기록 초기화
                                NextSceneName = "Entrance";
                            }
                            else //해당 id토큰 회원 있음
                                NextSceneName = "Square";
                        }
                    });
                }
            }
            else if ((PlayerPrefs.GetInt("GuestLogin", 0) == 1) && (PlayerPrefs.GetInt("GooglePlayLogin", 0) == 0)) //게스트로 로그인 했던 유저
            {
                var request = new ScanRequest
                {
                    TableName = "user_info",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> { { ":val", new AttributeValue { S = PlayerPrefs.GetString("GuestCode", "") } } },
                    FilterExpression = "user_code = :val",
                    ProjectionExpression = "user_no"
                };
                Setup.DBclient.ScanAsync(request, (result) =>
                {
                    if (result.Exception != null)
                        Debug.Log("정보 받아오기 실패");
                    else
                    {
                        if (result.Response.Count == 0) //존재하지 않는 게스트코드일 경우
                        {
                            Debug.Log("해당 게스트코드 회원 정보 찾기 실패");
                            PlayerPrefs.SetInt("GuestLogin", 0); //게스트 로그인 기록 초기화
                            NextSceneName = "Entrance";
                        }
                        else //해당 게스트코드 회원 있음
                            NextSceneName = "Square";
                    }
                });
            }
            //위의 모든 조건에 해당 하지 않는 경우(ex.구글 로그인 + 게스트 둘 다 로그인 한 유저 등등의 오류)
            else
            {
                Debug.Log("로그인 기록 오류");
                PlayerPrefs.SetInt("GooglePlayLogin", 0); //구글 로그인 기록 초기화
                PlayerPrefs.SetInt("GuestLogin", 0); //게스트 로그인 기록 초기화
                NextSceneName = "Entrance";
            }
        }
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
