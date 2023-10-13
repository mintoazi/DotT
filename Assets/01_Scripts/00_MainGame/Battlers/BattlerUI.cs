using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class BattlerUI : MonoBehaviour
{
    // ���݂̑����\��
    [SerializeField] private Image typeImage;
    [SerializeField] private Sprite[] typeSprite;

    // ���݂̃R�X�g�\��
    [SerializeField] private Image[] costsImage;
    [SerializeField] private Sprite[] costsType;
    [SerializeField] private Color usedColor;
 
    // �L�����N�^�[�̕\��
    [SerializeField] private Image battlerSkin;
    [SerializeField] private Sprite[] battlerSprite;

    [SerializeField] private Text hpText;


    public void UpdateCharaType(int charaType)
    {
        battlerSkin.sprite = battlerSprite[charaType];
        typeImage.sprite = typeSprite[charaType];
        for (int i = 0; i < costsImage.Length; i++)
        {
            costsImage[i].sprite = costsType[charaType];
        }
    }

    public void UseCost(int cost, bool isUse)
    {
        if (isUse) costsImage[cost].color = usedColor;
        else return;
    }

    public void UpdateType(int type)
    {
        typeImage.sprite = typeSprite[type];
    }

    public void UpdateHP(int hp)
    {
        hpText.text = hp.ToString();
    }
}
