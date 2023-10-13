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
    // �{�^������������}�b�`���O�J�n
    // ���[��ID�Ń}�b�`���O
    // �Ȃ���Ύ����ō��
    // �������񖼂ɂȂ�΃V�[���J��

    private bool inRoom;
    private bool isMatching;

    [SerializeField] private Text roomIdText;
    [SerializeField] private GameObject matchFailedWindow;
    public void OnMatchingButton()
    {
        if (roomIdText.text == "") return;
        // PhotonServerSettings�̐ݒ���e���g���ă}�X�^�[�T�[�o�[�֐ڑ�����
        PhotonNetwork.ConnectUsingSettings();
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
        // �����_���ȍ��W�Ɏ��g�̃A�o�^�[�i�l�b�g���[�N�I�u�W�F�N�g�j�𐶐�����
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
        //Update�����̒ǉ�
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
            //Scene�J��
            SceneManager.LoadScene("Battle");
        }
    }
}
