using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonsInSquare : MonoBehaviour {
    private WebViewObject webViewObject;
    public GameObject RankingPanel;
    public GameObject RankingContentPanel;
    public GameObject SquareManagement;
    
    void Update()
    {
        if (GameObject.Find("WebViewObject") != null)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Destroy(GameObject.Find("WebViewObject"));
                if (GameObject.Find("QuitPanel") != null)
                    GameObject.Find("QuitPanel").SetActive(false);
                return;
            }
        }
    }

    public void OptionButton_click()
    {
        Setup.OptionPanel.SetActive(true);
    }

    public void MainButton_click()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Main");
    }

    public void NoticeButton_click()
    {
        string strUrl = "https://greenapple16.tistory.com/40";

        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init((msg) => {Debug.Log(string.Format("CallFromJS[{0}]", msg));});
        webViewObject.LoadURL(strUrl);
        webViewObject.SetVisibility(true);
        webViewObject.SetMargins(0, 0, 0, 0);
    }

    public void OpenRanking()
    {
        var r = SquareManagement.GetComponent<RankingManagement>();
        r.GetInfo();
        RankingPanel.SetActive(true);
    }

    public void CloseRanking()
    {
        var r = SquareManagement.GetComponent<RankingManagement>();
        r.DeleteAllItems();
        RankingPanel.SetActive(false);
    }
}

