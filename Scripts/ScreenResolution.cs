using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolution : MonoBehaviour {

	void Start () {
        Screen.SetResolution(Screen.width, Screen.width * 9 / 16, false);
    }
}
