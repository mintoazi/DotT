using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

public class BattlerMove : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private RaycastHit raycastHit;
    [SerializeField] private GameObject selectTile;
    [SerializeField] private Vector3 tileSpace;
    [SerializeField] private GameObject[] pieces = new GameObject[3];
    [SerializeField] private Material[] materials;
    private Renderer pieceRenderer;
    public Transform[,] TilePosition { get; set; }
    public Vector2Int PiecePos { get; private set; } // 現在のプレイヤー位置

    // 移動できるフラグ
    bool canMove = false;

    void Awake()
    {
        //Init();
        //Update処理の登録
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }

    public void Init(int type)
    {
        GameObject g = Instantiate(pieces[type], transform);
        PiecePos = new Vector2Int(1, 1);
        pieceRenderer = g.transform.GetChild(0).gameObject.GetComponent<Renderer>();
        pieceRenderer.material = materials[type];
    }

    public void UpdatePieceType(int type)
    {
        //pieceRenderer.material = materials[type];
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    /// <summary>
    /// 移動が完了するまで待つ
    /// </summary>
    public async UniTask<Vector2Int> Move()
    {
        canMove = true;
        await UniTask.WaitUntil(() => !canMove);
        return PiecePos;
    }

    public void Moved(Vector3 pos, Vector2Int currentPos)
    {
        //var diff = pos - transform.position;
        transform.position = pos;
        PiecePos = currentPos;
       // Debug.Log(PiecePos + "に移動した");
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    private void ManagedUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.Escape)) { Move().Forget(); }

        if (!canMove) return;
        // 移動ができるようになったらRayを飛ばす
        Vector3 inflatePos = new Vector3(0f, 0.02f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();
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
            var diff = raycastHit.transform.position - transform.position;
            var prePiecePos = new Vector2Int(PiecePos.x + (int)(diff.z / tileSpace.z),
                                             PiecePos.y + (int)diff.x);

            Locator<GameMaster>.Instance.ActiveAttackTiles(prePiecePos);

            // 移動先のタイルをクリックしたら移動
            if (Input.GetMouseButtonDown(0) && selectTile)
            {
                Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();
                PiecePos = prePiecePos;
                // 移動したガイドを消す
                selectTile.SetActive(false);
                canMove = false;
            }
        }
    }
}
