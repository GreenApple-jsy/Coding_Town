using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoomEmoticonManager : MonoBehaviour
{
    private PhotonView pv;
    public Image Player1EmoticonImage;
    public Image Player2EmoticonImage;
    public GameObject Player1EmoticonBackgroundImage;
    public GameObject Player2EmoticonBackgroundImage;
    public Sprite[] EmoticonImages = new Sprite[5];

    private void Awake()
    {
        Player1EmoticonBackgroundImage.SetActive(false);
        Player2EmoticonBackgroundImage.SetActive(false);
        Player1EmoticonImage.gameObject.SetActive(false);
        Player2EmoticonImage.gameObject.SetActive(false);
        pv = GetComponent<PhotonView>();
    }

    public void Emoticon1()
    {
        pv.RPC("ShowEmoticon", PhotonTargets.All, PhotonNetwork.isMasterClient, 0);
    }

    public void Emoticon2()
    {
        pv.RPC("ShowEmoticon", PhotonTargets.All, PhotonNetwork.isMasterClient, 1);
    }

    public void Emoticon3()
    {
        pv.RPC("ShowEmoticon", PhotonTargets.All, PhotonNetwork.isMasterClient, 2);
    }

    public void Emoticon4()
    {
        pv.RPC("ShowEmoticon", PhotonTargets.All, PhotonNetwork.isMasterClient, 3);
    }

    public void Emoticon5()
    {
        pv.RPC("ShowEmoticon", PhotonTargets.All, PhotonNetwork.isMasterClient, 4);
    }


    [PunRPC]
    public void ShowEmoticon(bool Player1, int EmoticonType)
    {
        if (Player1)
            StartCoroutine(Player1Emoticon(EmoticonType));
        else
            StartCoroutine(Player2Emoticon(EmoticonType));
    }

    IEnumerator Player1Emoticon(int EmoticonType)
    {
        Player1EmoticonImage.GetComponent<Image>().sprite = EmoticonImages[EmoticonType];
        Player1EmoticonBackgroundImage.SetActive(true);
        Player1EmoticonImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        Player1EmoticonImage.gameObject.SetActive(false);
        Player1EmoticonBackgroundImage.SetActive(false);
    }

    IEnumerator Player2Emoticon(int EmoticonType)
    {
        Player2EmoticonImage.GetComponent<Image>().sprite = EmoticonImages[EmoticonType];
        Player2EmoticonBackgroundImage.SetActive(true);
        Player2EmoticonImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        Player2EmoticonImage.gameObject.SetActive(false);
        Player2EmoticonBackgroundImage.SetActive(false);
    }
}
