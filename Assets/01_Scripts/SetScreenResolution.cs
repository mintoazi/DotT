using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    private void Awake()
    {
        Screen.SetResolution(1920 / 2, 1080 / 2, false);
    }
}
