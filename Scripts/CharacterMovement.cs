using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    public bool Up = false, Down = false, Left = false, Right = false;
    public static GameObject Character;
    public static int CurrentAniState;
	void Start () {
        Up = false;
        Down = false;
        Left = false;
        Right = false;
	}

	void Update () {
        if (Character != null)
        {
            if (Up)
            {
                if (Character.transform.localPosition.y <= 2.4f)
                    Character.transform.Translate(0, 0.1f, 0);
            }
            else if (Down)
            {

                if (Character.transform.localPosition.y >= -3.33f)
                    Character.transform.Translate(0, -0.1f, 0);
            }
            else if (Left)
            {
                if (Character.transform.localPosition.x >= -4.43f)
                    Character.transform.Translate(-0.1f, 0, 0);
            }
            else if (Right)
            {
                if (Character.transform.localPosition.x <= 4.4f)
                    Character.transform.Translate(0.1f, 0, 0);
            }
        }
    }

    public void GoUp()
    {
        Up = true;
        Character.GetComponent<Animator>().Play("walk_back");
        CurrentAniState = 3;
    }
    public void StopUp()
    {
        Up = false;
        Character.GetComponent<Animator>().Play("idle_back");
        CurrentAniState = 2;
    }
    public void GoDown()
    {
        Down = true;
        Character.GetComponent<Animator>().Play("walk_front");
        CurrentAniState = 1;
    }
    public void StopDown()
    {
        Down = false;
        Character.GetComponent<Animator>().Play("idle_front");
        CurrentAniState = 0;
    }
    public void GoLeft()
    {
        Left = true;
        Character.GetComponent<Animator>().Play("walk_left");
        CurrentAniState = 5;
    }
    public void StopLeft()
    {
        Left = false;
        Character.GetComponent<Animator>().Play("idle_left");
        CurrentAniState = 4;
    }
    public void GoRight()
    {
        Right = true;
        Character.GetComponent<Animator>().Play("walk_right");
        CurrentAniState = 7;
    }
    public void StopRight()
    {
        Right = false;
        Character.GetComponent<Animator>().Play("idle_right");
        CurrentAniState = 6;
    }
}
