using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

public class BattlerMove : MonoBehaviour
{
    [SerializeField] private GameObject piece;
    [SerializeField] private Camera playerCamera;
    private RaycastHit raycastHit;
    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject canMoveTile;
    [SerializeField] private GameObject selectTile;
    [SerializeField] private Vector3 tileSpace;
    

    private int tileLength = 3; // タイルの量
    private GameObject[,] tiles = new GameObject[3,3]; // タイル
    private GameObject[,] canMoveTiles = new GameObject[3,3]; // ガイド用のタイル

    private Vector2Int piecePos = new Vector2Int(1, 1); // 現在のプレイヤー位置
    public Vector2Int PiecePos { get => piecePos; private set => piecePos = value; }

    // 移動できるフラグ
    bool canMove = false;

    void Start()
    {
        CreateTiles();
        DeactiveMoveTile();

        //Update処理の登録
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }

    /// <summary>
    /// 移動が完了するまで待つ
    /// </summary>
    public async UniTask Move()
    {
        canMove = true;
        ActiveMoveTile();
        await UniTask.WaitUntil(() => !canMove);
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void ManagedUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.Escape)) { Move().Forget(); }

        if (!canMove) return;
        // 移動ができるようになったRayを飛ばす
        Vector3 inflatePos = new Vector3(0f, 0.02f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out raycastHit))
        {
            // 移動先のタイル判定
            if (!raycastHit.collider.CompareTag("CanMove") || raycastHit.collider == null)
            {
                selectTile.SetActive(false);
                return;
            }
            if (!selectTile.activeSelf) selectTile.SetActive(true);
            selectTile.transform.position = raycastHit.transform.position + inflatePos;

            // 移動先のタイルをクリックしたら移動
            if (Input.GetMouseButtonDown(0) && selectTile)
            {
                piece.transform.position = raycastHit.transform.position;
                PiecePos = new Vector2Int((int)piece.transform.localPosition.x, (int)(piece.transform.localPosition.z / tileSpace.z));

                // 移動したガイドを消す
                DeactiveMoveTile();
                selectTile.SetActive(false);
                canMove = false;
            }
        }
    }

    private void CreateTiles()
    {
        for (int x = 0; x < tileLength; x++) 
        {
            for (int y = 0; y < tileLength; y++)
            {
                //Debug.Log(x + ":" + y);
                tiles[x, y] = Instantiate(tile, this.transform);
                tiles[x, y].transform.localPosition = new Vector3(tileSpace.x * x, 0f, tileSpace.z * y);
                canMoveTiles[x, y] = Instantiate(canMoveTile, this.transform);
                canMoveTiles[x, y].transform.localPosition = new Vector3(tileSpace.x * x, tileSpace.y, tileSpace.z * y);
            }
        }
    }

    private void ActiveMoveTile()
    {
        int x = PiecePos.x;
        int y = PiecePos.y;

        if (x < tileLength - 1) canMoveTiles[x + 1, y].SetActive(true);
        if (x > 0) canMoveTiles[x - 1, y].SetActive(true);
        if (y < tileLength - 1) canMoveTiles[x, y + 1].SetActive(true);
        if (y > 0) canMoveTiles[x, y - 1].SetActive(true);
    }

    private void DeactiveMoveTile()
    {
        for (int i = 0; i < tileLength; i++)
        {
            for (int j = 0; j < tileLength; j++)
            {
                canMoveTiles[i, j].SetActive(false);
            }
        }
    }
}
