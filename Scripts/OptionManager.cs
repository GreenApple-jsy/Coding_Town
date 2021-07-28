using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SceneManagement;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

public class OptionManager : MonoBehaviour
{
    public GameObject OptionPanel;
    public GameObject Option_AccountPanel;
    public GameObject Option_SoundPanel;
    public GameObject Option_KeyPanel;
    public GameObject Option_HelpPanel;

    public InputField NicknameInputField;
    public Text NicknamePlaceholder;
    public Text NicknameText;
    public Button NicknameChangeButton;
    public GameObject GPGS;
    public Text GPGSText;
    public GameObject Guest;
    public Text GuestText;
    public Text NicknameResultText;
    public Button DeleteAccountButton;
    public Button Back_AccountButton;

    public Slider BGMVolumeSlider;
    public Slider EffectVolumeSlider;

    public GameObject KeyLeftShow;
    public GameObject KeyRightShow;


    private void Start()
    {
        Back_AccountButton.interactable = true;
        DeleteAccountButton.interactable = true;
        NicknameInputField.GetComponent<InputField>().text = "";
        NicknameResultText.text = "";
        Option_AccountPanel.SetActive(false);
        Option_SoundPanel.SetActive(false);
        Option_KeyPanel.SetActive(false);
        Option_HelpPanel.SetActive(false);
    }

    public void AccountButton()
    {
        NicknameInputField.GetComponent<InputField>().text = "";
        NicknameResultText.text = "";
        NicknamePlaceholder.text = PlayerPrefs.GetString("Nickname");
        if (PlayerPrefs.GetInt("GuestLogin", 0) == 0)
        {
            GPGS.SetActive(true);
            GPGSText.text = Social.localUser.userName;
            Guest.SetActive(false);
        }
        else
        {
            GPGS.SetActive(false);
            GuestText.text = PlayerPrefs.GetString("GuestCode");
            Guest.SetActive(true);
        }
        Option_AccountPanel.SetActive(true);
    }

    public void SoundButton()
    {
        Option_SoundPanel.SetActive(true);
        BGMVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        EffectVolumeSlider.value = PlayerPrefs.GetFloat("EffectVolume", 1f);
    }

    public void KeyButton()
    {
        if (PlayerPrefs.GetInt("KeySelection", 1) == 1) //오른쪽
        {
            KeyRightShow.SetActive(true);
            KeyLeftShow.SetActive(false);
        }
        else
        {
            KeyRightShow.SetActive(false);
            KeyLeftShow.SetActive(true);
        }

        Option_KeyPanel.SetActive(true);
    }

    public void HelpButton()
    {
        Option_HelpPanel.SetActive(true);
    }

    public void BackButton()
    {
        if (SceneManager.GetActiveScene().name == "Square")
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Square");
        }
        else
            SceneManager.LoadScene("Main");
        OptionPanel.SetActive(false);
    }
    //-----------------------------------------------------------------------------------
    //계정 관리
    public void ChangeNicknameButton()
    {
        if (NicknameText.text == PlayerPrefs.GetString("Nickname"))
        {
            NicknameResultText.text = "현재 닉네임과 동일합니다";
            NicknameText.text = "";
        }
        else if (NicknameText.text == "")
        {
            NicknameResultText.text = "닉네임을 입력하세요";
            NicknameText.text = "";
        }
        else
        {
            NicknameChangeButton.interactable = false;
            NicknameInputField.interactable = false;
            var request = new ScanRequest
            {
                TableName = "user_info",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":val", new AttributeValue { S = NicknameText.text}}},
                FilterExpression = "user_nick = :val",
                ProjectionExpression = "user_no"
            };
            Setup.DBclient.ScanAsync(request, (result) => {
                if (result.Exception != null)
                {
                    NicknameResultText.text = "바꿀 닉네임을 입력해주세요";
                    NicknameChangeButton.interactable = true;
                    NicknameInputField.interactable = true;
                }
                else
                {
                    if (result.Response.Count == 0) //중복이 아닌 닉네임일 경우
                    {
                        bool gplogin; string code;
                        if (PlayerPrefs.GetInt("GooglePlayLogin") == 1)
                        {
                            gplogin = true;
                            code = PlayerPrefs.GetString("GPGSidToken");
                        }
                        else
                        {
                            gplogin = false;
                            code = PlayerPrefs.GetString("GuestCode");
                        }
                        User Me = new User
                        {
                            user_no = PlayerPrefs.GetString("UserN"),
                            GPlogin_status = gplogin,
                            user_code = code,
                            user_nick = NicknameText.text,
                            user_exp = PlayerPrefs.GetInt("Exp"),
                            own_coin = PlayerPrefs.GetInt("Coin"),
                            eq_acc = PlayerPrefs.GetInt("Acc"),
                            eq_item1 = PlayerPrefs.GetInt("Item1"),
                            eq_item2 = PlayerPrefs.GetInt("Item2"),
                            present_char = PlayerPrefs.GetInt("Character"),
                            login_status = true
                        };
                        Setup.context.SaveAsync(Me, (r) =>
                        {
                            if (r.Exception == null)
                            {
                                PlayerPrefs.SetString("Nickname", NicknameText.text);
                                NicknamePlaceholder.text = NicknameText.text;
                                NicknameText.text = "";
                                NicknameResultText.text = "닉네임 변경 성공!";
                            }
                            else
                                NicknameResultText.text = "네트워크 에러";
                            NicknameChangeButton.interactable = true;
                            NicknameInputField.interactable = true;
                        });

                    }
                    else //중복 닉네임인 경우
                    {
                        NicknameResultText.text = "중복인 닉네임입니다";
                        NicknameChangeButton.interactable = true;
                        NicknameInputField.interactable = true;
                    }
                }
            });
        }
    }

    public void GuestCodeCopyButton()
    {
        UniClipboard.SetText(PlayerPrefs.GetString("GuestCode"));
    }

    public void GuestLogoutButton()
    {
        PlayerPrefs.DeleteAll();
        Destroy(GameObject.Find("SetupManager"));
        SceneManager.LoadScene(0);
    }

    public void GPGSLogoutButton()
    {
        //PlayGamesPlatform.Instance.SignOut();
        //((PlayGamesPlatform)Social.Active).SignOut();
        PlayerPrefs.DeleteAll();
        Destroy(GameObject.Find("SetupManager"));
        SceneManager.LoadScene(0);
    }

    public void GuestDeleteAccountButton()
    {
        Back_AccountButton.interactable = false;
        DeleteAccountButton.interactable = false;
        Setup.context.DeleteAsync<User>(PlayerPrefs.GetString("UserN"), (res) =>
        {
            if (res.Exception == null)
            {
                Setup.context.LoadAsync<User>(PlayerPrefs.GetString("UserN"), (result) =>
                {
                    User DeletedUser = result.Result;
                    if (DeletedUser == null) //삭제되었는지 재확인, 삭제한 유저가 검색되지 않을 경우 성공한것임
                    {
                        Debug.Log("유저 삭제 성공");
                        PlayerPrefs.DeleteAll();
                        Destroy(GameObject.Find("SetupManager"));
                        SceneManager.LoadScene(0);
                    }
                });
            }
        });
    }

    public void BackToOption_Account()
    {
        Option_AccountPanel.SetActive(false);
    }

    //-----------------------------------------------------------------------------------
    //사운드 설정
    public void BGMVolumeSliderChange()
    {
        PlayerPrefs.SetFloat("BGMVolume", BGMVolumeSlider.value);
    }

    public void EffectVolumeSliderChange()
    {
        PlayerPrefs.SetFloat("EffectVolume", EffectVolumeSlider.value);
    }

    public void BackToOption_Sound()
    {
        Option_SoundPanel.SetActive(false);
    }

    //-----------------------------------------------------------------------------------
    //방향키 설정
    public void KeyLeftSelect()
    {
        PlayerPrefs.SetInt("KeySelection", 0);
        KeyRightShow.SetActive(false);
        KeyLeftShow.SetActive(true);
    }

    public void KeyRightSelect()
    {
        PlayerPrefs.SetInt("KeySelection", 1);
        KeyRightShow.SetActive(true);
        KeyLeftShow.SetActive(false);
    }

    public void BackToOption_Key()
    {
        Option_KeyPanel.SetActive(false);
    }

    //-----------------------------------------------------------------------------------
    //도움말
    public void BackToOption_Help()
    {
        Option_HelpPanel.SetActive(false);
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