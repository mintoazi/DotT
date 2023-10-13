using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;

public class BattlerMove : MonoBehaviour
{
    [SerializeField] private GameObject piece;
    [SerializeField] private Camera playerCamera;
    private RaycastHit raycastHit;
    [SerializeField] private GameObject selectTile;
    [SerializeField] private Vector3 tileSpace;
    
    public Transform[,] TilePosition { get; set; }
    public Vector2Int PiecePos { get; private set; } // ���݂̃v���C���[�ʒu

    // �ړ��ł���t���O
    bool canMove = false;

    void Awake()
    {
        Init();
        //Update�����̓o�^
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }

    public void Init()
    {
        PiecePos = new Vector2Int(1, 1);
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    /// <summary>
    /// �ړ�����������܂ő҂�
    /// </summary>
    public async UniTask<Vector2Int> Move()
    {
        canMove = true;
        await UniTask.WaitUntil(() => !canMove);
        return PiecePos;
    }

    public void Moved(Vector3 pos)
    {
        var diff = pos - transform.position;
        transform.position = pos;
        PiecePos = new Vector2Int(PiecePos.x + (int)diff.x, PiecePos.y + (int)(diff.z / tileSpace.z));
    }

    /// <summary>
    /// �ړ�����
    /// </summary>
    private void ManagedUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.Escape)) { Move().Forget(); }

        if (!canMove) return;
        // �ړ����ł���悤�ɂȂ���Ray���΂�
        Vector3 inflatePos = new Vector3(0f, 0.02f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out raycastHit))
        {
            // �ړ���̃^�C������
            if (!raycastHit.collider.CompareTag("CanMove") || raycastHit.collider == null)
            {
                selectTile.SetActive(false);
                return;
            }
            if (!selectTile.activeSelf) selectTile.SetActive(true);
            selectTile.transform.position = raycastHit.transform.position + inflatePos;

            // �ړ���̃^�C�����N���b�N������ړ�
            if (Input.GetMouseButtonDown(0) && selectTile)
            {
                var diff = raycastHit.transform.position - transform.position;
                transform.position = raycastHit.transform.position;
                PiecePos = new Vector2Int(PiecePos.x + (int)diff.x, PiecePos.y + (int)(diff.z / tileSpace.z));
                // �ړ������K�C�h������
                selectTile.SetActive(false);
                canMove = false;
            }
        }
    }
}
