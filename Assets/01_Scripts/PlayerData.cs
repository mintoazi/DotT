using UnityEngine;

public class PlayerData : SingletonMonoBehaviour<PlayerData>
{
    public static PlayerData instance;
    private int playerChara = 0;
    private int enemyChara = 0;
    private int playerDeck = 0;
    private int enemyDeck = 0;
    public int PlayerChara
    {
        get { Debug.Log(playerChara); return playerChara;  }
        set { playerChara = value; }
    }
    public int EnemyChara
    {
        get { Debug.Log(enemyChara); return enemyChara; }
        set { enemyChara = value; Debug.Log(enemyChara); }
    }

    public int PlayerDeck { get => playerDeck; private set => playerDeck = value; }
    public int EnemyDeck { get => enemyDeck; private set => enemyDeck = value; }

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

    public void PushPlayerDeck(int num)
    {
        PlayerDeck = num;
    }

    public void PushEnemyDeck(int num)
    {
        EnemyDeck = num;
    }
}
