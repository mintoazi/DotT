using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    public static OnlineMenuManager onlineManager;
    public static int HostCharacter = 0;
    public static int GuestCharacter = 0;
    public static int HostDeck = 0;
    public static int GuestDeck = 0;
    // ボタンを押したらマッチング開始
    // ルームIDでマッチング
    // なければ自分で作る
    // 部屋が二名になればシーン遷移

    private bool inRoom = false;
    private bool isStart = false;
    private bool eIsReady = false;

    private string roomIdText;

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
    public void OnMatchingButton(string s)
    {
        roomIdText = s;
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnStartButton()
    {
        if (isStart) return;
        if (!inRoom) return;
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(OnReady), RpcTarget.Others);
            Debug.Log("あなたはホストではありません。");
            return;
        }

        StartGame().Forget();
    }

    private async UniTask StartGame()
    {
        await UniTask.WaitUntil(() => eIsReady);
        isStart = true;
        //Scene遷移
        photonView.RPC(nameof(LoadScene), RpcTarget.AllViaServer);
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        RoomOptions options = new RoomOptions();
        options.PublishUserId = true;
        options.MaxPlayers = 2;
        //if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount) return;
        PhotonNetwork.JoinOrCreateRoom(roomIdText, options, TypedLobby.Default);
        Debug.Log("ロビーに入室しました。");
        
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom()
    {
        inRoom = true;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            EnemyHasJoined();
            Debug.Log(PhotonNetwork.MasterClient.UserId + "のロビーに入室しました。");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SceneLoader.Instance.Load(Scenes.Scene.HOME).Forget();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        EnemyHasJoined();
        Debug.Log(newPlayer.UserId + "が入室しました。");
    }
    private void EnemyHasJoined()
    {
        Locator<CharacterSelectController>.Instance.EnemyHasJoined();
        if (PhotonNetwork.IsMasterClient) photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, HostCharacter);
        else photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, HostCharacter);

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
        Locator<CharacterSelectController>.Instance.EnemyHasLeft();
        Debug.Log(otherPlayer.UserId + "が退出しました。");
    }
    private void ManagedUpdate()
    {
        
    }

    public void OnClickType(int type)
    {
        if (!inRoom) return;

        if (PhotonNetwork.IsMasterClient) HostCharacter = type;
        else GuestCharacter = type;
       
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC(nameof(EnemyChangeChara), RpcTarget.Others, type);
        }
    }
    [PunRPC]
    private void OnReady()
    {
        eIsReady = true;
    }
    [PunRPC]
    private void EnemyChangeChara(int type)
    {
        if (PhotonNetwork.IsMasterClient) GuestCharacter = type;
        else HostCharacter = type;
        Locator<CharacterSelectController>.Instance.ChangeEnemyChara(type);
    }

    [PunRPC]
    private void LoadScene()
    {
        SceneLoader.Instance.Load(Scenes.Scene.BATTLE).Forget();
    }
}
