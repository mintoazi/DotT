using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    private void Awake()
    {
        Screen.SetResolution(1024, 576, false);
    }
}
