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
    // �{�^������������}�b�`���O�J�n
    // ���[��ID�Ń}�b�`���O
    // �Ȃ���Ύ����ō��
    // �������񖼂ɂȂ�΃V�[���J��

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
        PhotonNetwork.JoinOrCreateRoom(roomIdText.text, new RoomOptions() { MaxPlayers = 2 }, TypedLobby.Default);
    }

    // �Q�[���T�[�o�[�ւ̐ڑ��������������ɌĂ΂��R�[���o�b�N
    public override void OnJoinedRoom()
    {
        inRoom = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {

    }

    private void Awake()
    {

        //Update�����̒ǉ�
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
