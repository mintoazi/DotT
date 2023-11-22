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
    // �{�^������������}�b�`���O�J�n
    // ���[��ID�Ń}�b�`���O
    // �Ȃ���Ύ����ō��
    // �������񖼂ɂȂ�΃V�[���J��

    private bool inRoom;
    private bool isMatching;

    [SerializeField] private Text roomIdText;
    [SerializeField] private GameObject matchFailedWindow;

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
    public void OnMatchingButton()
    {
        if (roomIdText.text == "") return;
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�[�֐ڑ�����
        PhotonNetwork.ConnectUsingSettings();
    }
    public void OnStartButton()
    {
        if (isMatching) return;
        if (!inRoom) return;
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("���Ȃ��̓z�X�g�ł͂���܂���B");
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
            //Scene�J��
            photonView.RPC(nameof(LoadScene), RpcTarget.AllViaServer);
        }
    }

    // �}�X�^�[�T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnConnectedToMaster()
    {
        // "Room"�Ƃ������O�̃��[���ɎQ������i���[�������݂��Ȃ���΍쐬���ĎQ������j
        RoomOptions options = new RoomOptions();
        options.PublishUserId = true;
        options.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom(roomIdText.text, options, TypedLobby.Default);
        Debug.Log("���r�[�ɓ������܂����B");
        
    }

    // �Q�[���T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnJoinedRoom()
    {
        inRoom = true;
        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Debug.Log(PhotonNetwork.MasterClient.UserId + "�̃��r�[�ɓ������܂����B");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.UserId + "���������܂����B");
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
        Debug.Log(otherPlayer.UserId + "���ޏo���܂����B");
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
