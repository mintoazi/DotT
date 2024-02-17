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
    // �{�^������������}�b�`���O�J�n
    // ���[��ID�Ń}�b�`���O
    // �Ȃ���Ύ����ō��
    // �������񖼂ɂȂ�΃V�[���J��

    private bool inRoom = false;
    private bool isStart = false;
    private bool eIsReady = false;

    private string roomIdText;

    private void Awake()
    {
        // �V���O���g��
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

        //Update�����̒ǉ�
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
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�[�֐ڑ�����
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnStartButton()
    {
        if (isStart) return;
        if (!inRoom) return;
        if (!PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(OnReady), RpcTarget.Others);
            Debug.Log("���Ȃ��̓z�X�g�ł͂���܂���B");
            return;
        }

        StartGame().Forget();
    }

    private async UniTask StartGame()
    {
        await UniTask.WaitUntil(() => eIsReady);
        isStart = true;
        //Scene�J��
        photonView.RPC(nameof(LoadScene), RpcTarget.AllViaServer);
    }

    // �}�X�^�[�T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnConnectedToMaster()
    {
        // "Room"�Ƃ������O�̃��[���ɎQ������i���[�������݂��Ȃ���΍쐬���ĎQ������j
        RoomOptions options = new RoomOptions();
        options.PublishUserId = true;
        options.MaxPlayers = 2;
        //if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount) return;
        PhotonNetwork.JoinOrCreateRoom(roomIdText, options, TypedLobby.Default);
        Debug.Log("���r�[�ɓ������܂����B");
        
    }

    // �Q�[���T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnJoinedRoom()
    {
        inRoom = true;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            EnemyHasJoined();
            Debug.Log(PhotonNetwork.MasterClient.UserId + "�̃��r�[�ɓ������܂����B");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SceneLoader.Instance.Load(Scenes.Scene.HOME).Forget();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        EnemyHasJoined();
        Debug.Log(newPlayer.UserId + "���������܂����B");
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
        Debug.Log("�ޏo���܂����B");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Locator<CharacterSelectController>.Instance.EnemyHasLeft();
        Debug.Log(otherPlayer.UserId + "���ޏo���܂����B");
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
