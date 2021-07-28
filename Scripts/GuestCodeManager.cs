using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class GuestCodeManager : MonoBehaviour {
    public string GuestCode = "";
    public Text GuestCodeMent;
    public bool Checking;
    public bool Overlapping;

    public Button CheckButton;
    public Button OkayButton;

    private int num;

    private void Awake()
    {
        Checking = false;
        Overlapping = true;
        OkayButton.interactable = false;
        Setup.Setting();
        StartCoroutine(Check());
    }

    IEnumerator Check()
    {
        Checking = true;
        GuestCodeGenerator();
        CheckOverlapping();
        while (Checking == true)
            yield return null;
        if (Overlapping == false)
            GuestCodeMent.text = "회원님의 Guest Code는  " + GuestCode + "  입니다";
        else
            StartCoroutine(Check2());
        //stackoverflow 오류 해결하기 & 중복 코드 찾기
        /*if (Overlapping == false)
            GuestCodeMent.text = "회원님의 Guest Code는  " + GuestCode + "  입니다";
        else
            StartCoroutine(Check());*/
    }

    IEnumerator Check2()
    {
        Checking = true;
        GuestCodeGenerator();
        CheckOverlapping();
        while (Checking == true)
            yield return null;
        GuestCodeMent.text = "회원님의 Guest Code는  " + GuestCode + "  입니다";
    }

    public void GuestCodeGenerator()
    {
        GuestCode = "";
        int point = 0;
        string input = "abcdefghijklmnopqrstuvwxyz0123456789";
        for (point = 0; point < 7; point++)
        {
            int random = Random.Range(0, 36);
            GuestCode = GuestCode + input.Substring(random, 1);
        }
    }

    public void CheckOverlapping() //중복이 아닐 경우 overlapping 이진수를 false로 바꿈
    {
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
                Debug.Log("정보 받아오기 실패"); //오프라인이거나 등등....
                Overlapping = true;
            }  
            else
            {
                if (result.Response.Count == 0) //중복이 아닌 게스트코드일 경우
                    Overlapping = false;
                else //이미 게스트코드가 존재하는 경우
                    Overlapping = true;
            }
        });
        Checking = false;
    }

    public void CheckButtonClick()
    {
        if (num == 0)
        {
            num += 1;
            OkayButton.interactable = true;
            CheckButton.GetComponent<Image>().sprite = Resources.Load("check", typeof(Sprite)) as Sprite;

        }
        else
        {
            num -= 1;
            OkayButton.interactable = false;
            CheckButton.GetComponent<Image>().sprite = Resources.Load("check_off", typeof(Sprite)) as Sprite;
        }
    }

    public void OkButtonClick()
    {
        //동의 버튼 체크 확인 필요
        PlayerPrefs.SetString("GuestCode", GuestCode);
        SceneManager.LoadScene("GuestNewSetup");
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

    public void BackButton()
    {
        SceneManager.LoadScene("Entrance");
    }
}
