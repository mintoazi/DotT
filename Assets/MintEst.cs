using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MintEst : MonoBehaviour
{
    [SerializeField] RectTransform rectTrasform;
    private void Start()
    {
        rectTrasform.anchoredPosition = Vector2.zero;
    }
}
