using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class CharacterAnimation : Photon.MonoBehaviour, IPunObservable
{
    public Animator MyAnimator;
    public int AnimationState; //CharacterMovement에서 애니메이션 바꿀 때 자동 변경

    private void Start()
    {
        MyAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!photonView.isMine)
        {
            if (AnimationState == 0)
                MyAnimator.Play("idle_front");
            else if (AnimationState == 1)
                MyAnimator.Play("walk_front");
            else if (AnimationState == 2)
                MyAnimator.Play("idle_back");
            else if (AnimationState == 3)
                MyAnimator.Play("walk_back");
            else if (AnimationState == 4)
                MyAnimator.Play("idle_left");
            else if (AnimationState == 5)
                MyAnimator.Play("walk_left");
            else if (AnimationState == 6)
                MyAnimator.Play("idle_right");
            else if (AnimationState == 7)
                MyAnimator.Play("walk_right");
        }
        else
            AnimationState = CharacterMovement.CurrentAniState;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(AnimationState);
        }
        else
        {
            AnimationState = (int)stream.ReceiveNext();
        }
    }
}
