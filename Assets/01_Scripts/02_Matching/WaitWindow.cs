using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitWindow : MonoBehaviour
{
    [SerializeField] private Text waitDot;
    private float duration = 0.5f;
    private int currentDots = 0;
    private int dots = 3;
    private float time = 0f;

    private void Update()
    {
        time += Time.deltaTime;
        if(time > duration)
        {
            time = 0f;
            if(currentDots < dots)
            {
                currentDots++;
                waitDot.text += "E";
            }
            else
            {
                currentDots = 0;
                waitDot.text = "";
            }
        }
    }
}
