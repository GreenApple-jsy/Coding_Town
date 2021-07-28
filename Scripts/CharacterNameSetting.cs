using UnityEngine;

public class CharacterNameSetting : MonoBehaviour
{
    private PhotonView pv;
    private void Start()
    {
        pv = this.GetComponent<PhotonView>();
        pv.RPC("CharacterNameScan", PhotonTargets.All);
    }

    [PunRPC]
    void CharacterNameScan()
    {
        GameObject[] tempobj = GameObject.FindGameObjectsWithTag("Character");
        foreach (GameObject ob in tempobj)
        {
            if (ob.name.Contains("Clone"))
            {
                ob.name = ob.GetComponent<PhotonView>().owner.NickName;
                ob.transform.GetChild(1).GetComponent<TextMesh>().text = ob.name;
            }
        }
    }
}
