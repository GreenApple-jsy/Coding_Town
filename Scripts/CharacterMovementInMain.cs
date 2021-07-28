using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementInMain : MonoBehaviour
{
    public bool Up = false, Down = false, Left = false, Right = false;
    public GameObject Character;
    void Start()
    {
        Up = false;
        Down = false;
        Left = false;
        Right = false;
    }

    void Update()
    {
        if (Character != null)
        {
            if (Up)
            {
                if (Character.transform.position.y <= 2.18f)
                    Character.transform.Translate(0, 0.1f, 0);
            }
            else if (Down)
            {

                if (Character.transform.position.y >= -2.25f)
                    Character.transform.Translate(0, -0.1f, 0);
            }
            else if (Left)
            {
                if (Character.transform.position.x >= -3.99f)
                    Character.transform.Translate(-0.1f, 0, 0);
            }
            else if (Right)
            {
                if (Character.transform.position.x <= 1.95f)
                    Character.transform.Translate(0.1f, 0, 0);
            }
        }
    }

    public void GoUp()
    {
        Up = true;
        Character.GetComponent<Animator>().Play("walk_back");
    }
    public void StopUp()
    {
        Up = false;
        Character.GetComponent<Animator>().Play("idle_back");
    }
    public void GoDown()
    {
        Down = true;
        Character.GetComponent<Animator>().Play("walk_front");
    }
    public void StopDown()
    {
        Down = false;
        Character.GetComponent<Animator>().Play("idle_front");
    }
    public void GoLeft()
    {
        Left = true;
        Character.GetComponent<Animator>().Play("walk_left");
    }
    public void StopLeft()
    {
        Left = false;
        Character.GetComponent<Animator>().Play("idle_left");
    }
    public void GoRight()
    {
        Right = true;
        Character.GetComponent<Animator>().Play("walk_right");
    }
    public void StopRight()
    {
        Right = false;
        Character.GetComponent<Animator>().Play("idle_right");
    }
}
