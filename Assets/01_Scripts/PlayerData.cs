using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : SingletonMonoBehaviour<PlayerData>
{
    public static PlayerData instance;
    public int PlayerChara = 0;
    public int EnemyChara = 0;
    public override void CheckSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PushChara(int num)
    {
        PlayerChara = num;
    }
    public void PushEnemyChara(int num)
    {
        EnemyChara = num;
    }
}
