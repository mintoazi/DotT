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
    public static int GuestCharacter = 1;
    // ボタンを押したらマッチング開始
    // ルームIDでマッチング
    // なければ自分で作る
    // 部屋が二名になればシーン遷移

    private bool inRoom;
    private bool isMatching;

    [SerializeField] private Text roomIdText;
    [SerializeField] private GameObject matchFailedWindow;
    public void OnMatchingButton()
    {
        if (roomIdText.text == "") return;
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
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
        // ランダムな座標に自身のアバター（ネットワークオブジェクト）を生成する
        //var position = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        //PhotonNetwork.Instantiate("Piece", position, Quaternion.identity);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //base.OnJoinRoomFailed(returnCode, message);
        //matchFailedWindow.SetActive(true);
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
        if (isMatching) return;
        if (!inRoom) return;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            isMatching = true;
            //Scene遷移
            SceneManager.LoadScene("Battle");
        }
    }
}
