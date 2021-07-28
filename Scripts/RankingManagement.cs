using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class RankingManagement : MonoBehaviour
{
    public GameObject RankingPanel;
    public GameObject RankingContentItem;
    public GameObject RankingContentPanel;
    List<GameObject> Items = new List<GameObject>();
    List<User_short> UserInfo = new List<User_short>();
    public int myRank, UserCount; //나의 순위, 전체 사용자 수

    private void Awake()
    {
        Setup.Setting();
        RankingPanel.SetActive(false);
    }
    
    public void GetInfo()
    {
        var request = new ScanRequest
        {
            TableName = "user_info",
            ProjectionExpression = "user_no,user_nick,user_exp,eq_acc,present_char,login_status "
        };
        Setup.DBclient.ScanAsync(request, (result) => {
            if (result.Exception != null)
                Debug.Log("정보 받아오기 실패");
            else
            {
                StartCoroutine(SetRanking(result.Response.Items.Count));
                UserCount = result.Response.Items.Count;
                foreach (Dictionary<string, AttributeValue> item in result.Response.Items) //찾은 값들에 대하여
                {
                    User_short temp = new User_short();
                    temp.user_no = item["user_no"].S;
                    temp.user_nick = item["user_nick"].S;
                    temp.user_exp = int.Parse(item["user_exp"].N);
                    temp.eq_acc = int.Parse(item["eq_acc"].N);
                    temp.present_char = int.Parse(item["present_char"].N);
                    temp.login_status = item["login_status"].BOOL;
                    UserInfo.Add(temp);
                }
            }
        });
        

    }

    IEnumerator SetRanking(int UserCount)
    {
        while (UserInfo.Count < UserCount) //정보 다 받을때까지 기다림
            yield return null;

        UserInfo.Sort(delegate (User_short A, User_short B) //내림차순 기준 (오름차순 정렬의 경우, return값을 반대로 해주면 된다  1<-> -1)
        {
            if (A.user_exp < B.user_exp) return 1;
            else if (A.user_exp > B.user_exp) return -1; 
            return 0; //같은 값
        });

        for (int i = 0; i < UserCount; i++)
        {
            GameObject u = Instantiate(RankingContentItem);
            Items.Add(u);
            u.transform.SetParent(RankingContentPanel.transform);
            u.GetComponent<RankingItem>().SetPersonalInfo(i+1,UserInfo[i].user_no, UserInfo[i].user_nick, UserInfo[i].user_exp, UserInfo[i].eq_acc, UserInfo[i].present_char, UserInfo[i].login_status);
            
            //자신의 순위는 노란색으로 표시함
            if (UserInfo[i].user_nick == SquareManagement.Me.name)
            {
                myRank = i;
                ColorBlock colorBlock = u.GetComponent<Button>().colors;
                colorBlock.disabledColor = new Color(1f, 1f, 0f, 0.6f);
                u.GetComponent<Button>().colors = colorBlock;
            }
        }

        //랭킹 정보 아이템들이 리스트 안에 다 들어갈 때까지 기다림
        while (RankingContentPanel.GetComponent<RectTransform>().sizeDelta.y < (RankingContentItem.GetComponent<RectTransform>().sizeDelta.y - 14) * UserCount)
            yield return null;

        //자신의 랭킹 위치를 보여주도록 랭킹 리스트 노출 위치 수정
        float RankingPanelHeight = RankingContentPanel.GetComponent<RectTransform>().sizeDelta.y;
        float show_y_pos = (RankingPanelHeight / UserCount) * (myRank + 3) - (RankingPanelHeight / 2);
        RankingContentPanel.GetComponent<RectTransform>().position = new Vector2(RankingContentPanel.GetComponent<RectTransform>().position.x, show_y_pos);
    }

    public void DeleteAllItems()
    {
        foreach(var i in Items)
        {
            Destroy(i);
        }
        Items.Clear();
        UserInfo.Clear();
    }

    public class User_short
    {
        public string user_no;
        public string user_nick;
        public int user_exp;
        public int eq_acc;
        public int present_char;
        public bool login_status;
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
