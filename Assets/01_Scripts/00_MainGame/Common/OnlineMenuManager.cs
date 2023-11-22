using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine.SceneManagement;
public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    public static OnlineMenuManager onlineManager;
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

    private void Awake()
    {
        // シングルトン
        if (OnlineMenuManager.onlineManager == null)
        {
            OnlineMenuManager.onlineManager = this;
        }
        else
        {
            if(OnlineMenuManager.onlineManager != null)
            {
                Destroy(OnlineMenuManager.onlineManager.gameObject);
                OnlineMenuManager.onlineManager = this;
            }
        }

        //Update処理の追加
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void Start()
    {
        HostCharacter = 0;
        GuestCharacter = 0;
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
        RoomOptions options = new RoomOptions();
        options.PublishUserId = true;
        options.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom(roomIdText.text, options, TypedLobby.Default);
        Debug.Log("ロビーに入室しました。");
        
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        inRoom = true;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log(PhotonNetwork.MasterClient.UserId + "のロビーに入室しました。");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.UserId + "が入室しました。");
    }

    public void OnClickLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("退出しました。");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.UserId + "が退出しました。");
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
