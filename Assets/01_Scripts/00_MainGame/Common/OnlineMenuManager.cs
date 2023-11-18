using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;
public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    public static int HostCharacter = 0;
    public static int GuestCharacter = 0;
    private int myChara = 0;
    // ボタンを押したらマッチング開始
    // ルームIDでマッチング
    // なければ自分で作る
    // 部屋が二名になればシーン遷移

    private bool inRoom;
    private bool isMatching;

    [SerializeField] private Text roomIdText;
    [SerializeField] private GameObject matchFailedWindow;

    private void Start()
    {
        HostCharacter = 0;
        GuestCharacter = 0;
        //roomIdText.text = "111";
        //OnMatchingButton();
    }
    public void OnMatchingButton()
    {
        if (roomIdText.text == "") return;
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnStartButton()
    {
        if (isMatching) return;
        if (!inRoom) return;
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("あなたはホストではありません。");
            return;
        }
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                HostCharacter = myChara;
                photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, myChara);
            }
            else
            {
                GuestCharacter = myChara;
                photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, myChara);
            }
            isMatching = true;
            //Scene遷移
            photonView.RPC(nameof(LoadScene), RpcTarget.AllViaServer);
        }
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom(roomIdText.text, new RoomOptions() { MaxPlayers = 2 }, TypedLobby.Default);
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        inRoom = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    private void Awake()
    {

        //Update処理の追加
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void ManagedUpdate()
    {
        
    }

    [SerializeField] private Image myImage;
    [SerializeField] private Image enemyImage;
    [SerializeField] private Sprite[] characters;
    public void OnClickType(int type)
    {
        myChara = type;
        myImage.sprite = characters[type];
        if (!inRoom) return;
        if(PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, myChara);
        }
    }

    [PunRPC]
    private void EnemyChangeChara(int type)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GuestCharacter = type;
            HostCharacter = myChara;
        }
        else
        {
            HostCharacter = type;
            GuestCharacter = myChara;
        }
        enemyImage.sprite = characters[type];
    }

    [PunRPC]
    private void LoadScene()
    {
        SceneManager.LoadScene("Battle");
    }
}
