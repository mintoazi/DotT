using UnityEngine;

public class GenerateGame : MonoBehaviour
{
    /*
    Battler����
    
    
    �Ֆʐ���
    �Q�[���}�X�^�[���ŔՖʐ���

    Battler�ɏ����ʒu�Z�b�g
    Battler���ňړ�����

    �퓬��
    Battler���œ����蔻����m��

    GameMaster��������擾

    ���̌�A���葤��Battler�̏ꏊ�Ɣ�r
    
    ����ɑ��葤��������_���[�W����
    
     */

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject canMoveTilePrefab;
    [SerializeField] private Vector3 tileSize;
    [SerializeField] private Battler player;
    [SerializeField] private Battler enemy;
    private GameObject[,] canMoveTiles = new GameObject[3, 6];
    private GameObject[,] tiles = new GameObject[3,6];
    private enum BattlerNum
    {
        None,   // 0
        Player, // 1: �v���C���[
        Enemy   // 2: �G�l�~�[
    }
    private int[,] tilesData =
    {
        {0,0,0,0,0,0 },
        {0,1,0,0,2,0 },
        {0,0,0,0,0,0 },
    };

    public GameObject[,] Tiles { get => tiles; }

    // �^�C���̐���
    public void GenerateTiles()
    {
        for (int i = 0; i < Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < Tiles.GetLength(1); j++)
            {
                canMoveTiles[i, j] = Instantiate(canMoveTilePrefab, transform);
                Tiles[i, j] = Instantiate(tilePrefab, transform);
                // ���E�Ώ̂Ƀ^�C���𐶐�
                Vector3 pos = new Vector3(tileSize.x * (i - Tiles.GetLength(0) / 2),0f,tileSize.z * (j - Tiles.GetLength(1) / 2) + tileSize.z / 2);

                // �����̌v�Z����Ō��₷��������
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
            player.BattlerMove.Moved(tiles[pos.y,pos.x].transform.position);
        }
        else
        {
            // ���f
            int x = pos.x;
            if (x == 0) x = 2;
            else if (x == 2) x = 0;
            int y = pos.y;
            if (y == 0) y = 2;
            else if (y == 2) y = 0;

            x += tilesData.GetLength(0);
            //y += tilesData.GetLength(0);

            enemy.BattlerMove.Moved(tiles[y, x].transform.position);
        }
    }

    public void ActiveCanMoveTiles()
    {
        var pos = player.BattlerMove.PiecePos;
        int x = pos.x;
        int y = pos.y;

        if (x < Tiles.GetLength(0) - 1) canMoveTiles[x + 1, y].SetActive(true);
        if (x > 0) canMoveTiles[x - 1, y].SetActive(true);
        if (y < Tiles.GetLength(0) - 1) canMoveTiles[x, y + 1].SetActive(true);
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
