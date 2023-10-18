using UnityEngine;

public class GenerateGame : MonoBehaviour
{
    /*
    Battler生成
    
    
    盤面生成
    ゲームマスター側で盤面生成

    Battlerに初期位置セット
    Battler側で移動処理

    戦闘時
    Battler側で当たり判定を確定

    GameMasterが判定を取得

    その後、相手側のBattlerの場所と比較
    
    判定に相手側がいたらダメージ処理
    
     */

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject canMoveTilePrefab;
    [SerializeField] private Vector3 tileSize;
    [SerializeField] private Battler player;
    [SerializeField] private Battler enemy;
    private GameObject[,] canMoveTiles = new GameObject[6, 3];
    private GameObject[,] tiles = new GameObject[6,3];
    private enum BattlerNum
    {
        None,   // 0
        Player, // 1: プレイヤー
        Enemy   // 2: エネミー
    }
    private int[,] tilesData =
    {
        {0,0,0},
        {0,1,0},
        {0,0,0},
        {0,0,0},
        {0,2,0},
        {0,0,0},
    };

    public GameObject[,] Tiles { get => tiles; }

    // タイルの生成
    public void GenerateTiles()
    {
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                canMoveTiles[i, j] = Instantiate(canMoveTilePrefab, transform);
                Tiles[i, j] = Instantiate(tilePrefab, transform);
                // タイルを生成
                float x = tileSize.x * j;
                float y = tileSize.z * i;
                Vector3 pos = new Vector3(x,0f,y);

                // ここの計算を後で見やすくしたい
                Tiles[i, j].transform.localPosition = pos;
                pos.y += 0.01f;
                canMoveTiles[i, j].transform.localPosition = pos;

                if (tilesData[i, j] == 1) player.BattlerMove.SetPosition(pos);
                else if(tilesData[i, j] == 2) enemy.BattlerMove.SetPosition(pos);
            }
        }
        DeactiveMoveTiles();
    }

    public void RecieveMove(bool isMyClient, Vector2Int pos)
    {
        if (isMyClient)
        {
            player.BattlerMove.Moved(tiles[pos.x,pos.y].transform.position, pos);
        }
        else
        {
            Vector2Int movedPos = Calculator.CalcReflection(pos);
            Vector2Int enemyPos = movedPos;
            movedPos = Calculator.CalcEnemyPosition(movedPos);
            enemy.BattlerMove.Moved(tiles[movedPos.x, movedPos.y].transform.position, pos);
        }
    }

    public void ActiveCanMoveTiles()
    {
        var pos = player.BattlerMove.PiecePos;
        int x = pos.x;
        int y = pos.y;
        //Debug.Log(pos);
        if (x < Tiles.GetLength(1) - 1) canMoveTiles[x + 1, y].SetActive(true);
        if (x > 0) canMoveTiles[x - 1, y].SetActive(true);
        if (y < Tiles.GetLength(1) - 1) canMoveTiles[x, y + 1].SetActive(true);
        if (y > 0) canMoveTiles[x, y - 1].SetActive(true);
    }

    public void DeactiveMoveTiles()
    {
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                canMoveTiles[i, j].SetActive(false);
            }
        }
    }
}
