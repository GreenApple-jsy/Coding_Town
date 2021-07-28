using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class SquareChatting : PunBehaviour
{
    public InputField ChattingInput;
    private PhotonView pv;
    public static GameObject MyCharacter;

    void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    public void SendChatting()
    {
        string content = ChattingInput.text;
        pv.RPC("ShowChatting", PhotonTargets.All, content, MyCharacter.name);
        ChattingInput.text = null;
    }

    [PunRPC]
    public void ShowChatting(string Content, string CharacterName)
    {
        StartCoroutine(ShowTemp(Content, CharacterName));
    }

    IEnumerator ShowTemp(string Content, string CharacterName)
    {
        GameObject.Find(CharacterName).transform.GetChild(0).GetComponent<TextMesh>().text = Content;
        yield return new WaitForSeconds(2.5f);
        GameObject.Find(CharacterName).transform.GetChild(0).GetComponent<TextMesh>().text = null;
    }
}
