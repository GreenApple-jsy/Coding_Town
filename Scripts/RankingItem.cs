using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingItem : MonoBehaviour
{
    public int no;
    public string nick;
    public int exp;
    public int acc;
    public int character;
    public bool login;
    public Text RankingText;
    public Text NicknameText;
    public Text LevelText;

    public void SetPersonalInfo(int Ranking, string user_no, string user_nick, int user_exp, int eq_acc, int present_char, bool login_status)
    {
        no = int.Parse(user_no);
        nick = user_nick;
        exp = user_exp;
        acc = eq_acc;
        character = present_char;
        login = login_status;
        RankingText.text = Ranking.ToString() + "위";
        NicknameText.text = nick;
        LevelText.text = "Lv " + (exp / 100).ToString();
    }

    public void Onclick()
    {
        //버튼 누르면 해당 사용자 캐릭터 상태 보여주기
    }
}
