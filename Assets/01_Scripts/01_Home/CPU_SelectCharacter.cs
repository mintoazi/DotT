using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPU_SelectCharacter : MonoBehaviour
{
    [SerializeField] Image playerChara;
    [SerializeField] Image enemyChara;

    [SerializeField] Sprite[] characters;

    public void SelectPlayerChara(int num)
    {
        playerChara.sprite = characters[num];
        PlayerData.Instance.PlayerChara = num;
    }
    public void SelectEnemyChara(int num)
    {
        enemyChara.sprite = characters[num];
        PlayerData.Instance.EnemyChara = num;
    }
}
