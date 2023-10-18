using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] Battler player;
    [SerializeField] Battler enemy;
    [SerializeField] GenerateGame generateGame;
    [SerializeField] SelectCard selectCard;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] int hands = 3;
    [SerializeField] InGameSetting inGameSetting;
    private Battler currentBattler;
    private Battler waitBattler;
    private int turns = 0;

    private Phase phase = Phase.Init;
    bool isPhase = false;
    private enum Phase
    {
        Init,
        CoinTos,

        Draw,
        ReDraw,
        PlayCard,
        Move,
        Attack,
        End,
        Win,
        Lose,
        Wait
    }

    private void Awake()
    {
        generateGame.GenerateTiles();
        //player.OnSubmitAction = SubmittedAction;
        //Update�����̒ǉ�
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void Start()
    {
        Debug.Log("�����t�F�C�Y");
        InitPhase();
    }

    private void ManagedUpdate()
    {
        if (CardGenerator.IsLoadingCSV) return;

        if (isPhase) return;
        switch (phase)
        {
            case Phase.Init:
                //Debug.Log("�����t�F�C�Y");
                //InitPhase();
                break;
            case Phase.CoinTos:
                Debug.Log("�R�C���g�X�t�F�C�Y");
                CoinTos();
                break;
            case Phase.Draw:
                Debug.Log("�h���[�t�F�C�Y");
                DrawPhase();
                break;
            case Phase.ReDraw:
                Debug.Log("���h���[�t�F�C�Y");
                ReDrawPhase().Forget();
                break;
            case Phase.PlayCard:
                Debug.Log("�J�[�h�v���C�t�F�C�Y");
                PlayCardPhase().Forget();
                break;
            case Phase.Move:
                Debug.Log("�ړ��t�F�C�Y");
                MovePhase().Forget();
                break;
            case Phase.Attack:
                Debug.Log("�U���t�F�C�Y");
                AttackPhase().Forget();
                break;
            case Phase.End:
                Debug.Log("�G���h�t�F�C�Y");
                EndPhase().Forget();
                break;
            case Phase.Wait:
                WaitPhase();
                break;
            case Phase.Win:
                winPanel.SetActive(true);
                isPhase = true;
                break;
            case Phase.Lose:
                losePanel.SetActive(true);
                isPhase = true;
                break;
        }
    }
    private void InitPhase()
    {
        SendCardsTo(player, isEnemy: false);
        SendCardsTo(enemy, isEnemy: true);

        void SendCardsTo(Battler target, bool isEnemy)
        {
            for (int i = 0; i < hands; i++)
            {
                //Debug.Log("�h���[" + i);
                Card card = Locator<CardGenerator>.Instance.Draw(isEnemy);
                target.SetCardToHand(card);
            }
        }

        int hostType = OnlineMenuManager.HostCharacter;
        int guestType = OnlineMenuManager.GuestCharacter;

        if (PhotonNetwork.IsMasterClient)
        {
            player.Init(20, hostType);//demo
            enemy.Init(20, guestType);
        }
        else
        {
            player.Init(20, guestType);//demo
            enemy.Init(20, hostType);
        }

        SetPhase(Phase.CoinTos);
    }

    // �^�[���̐ݒ�
    private void CoinTos()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int rand = Random.Range(0, 1);
        if (rand == 0) // host�̃^�[��
        {
            currentBattler = player;
            waitBattler = enemy;
            SetPhase(Phase.Draw);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Wait);
            photonView.RPC(nameof(SetBattler), RpcTarget.Others, rand);
        }
        else
        {
            currentBattler = enemy;
            waitBattler = player;
            SetPhase(Phase.Wait);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
            photonView.RPC(nameof(SetBattler), RpcTarget.Others, rand);
        }
    }

    private bool CheckPlayer()
    {
        return currentBattler == enemy;
    }

    // �Q������
    private void DrawPhase()
    {
        isPhase = true;

        turns++;
        currentBattler.IsWait = false;
        //Debug.Log(currentBattler.gameObject.name);

        Card card = Locator<CardGenerator>.Instance.Draw(CheckPlayer());
        currentBattler.Draw(card);
        photonView.RPC(nameof(Draw), RpcTarget.Others, card.Base.Id);
        SetPhase(Phase.ReDraw);
    }
    private async UniTask ReDrawPhase()
    {
        isPhase = true;

        List<int> costs = await currentBattler.ReDraw();
        if (costs == null)
        {
            SetPhase(Phase.PlayCard);
            isPhase = false;
            return;
        }
        Card card = Locator<CardGenerator>.Instance.ReDraw(costs, CheckPlayer());

        photonView.RPC(nameof(ReDraw), RpcTarget.Others, card.Base.Id);
        currentBattler.SetCardToHand(card);
        selectCard.DeleteCard();

        SetPhase(Phase.PlayCard);
    }

    private async UniTask PlayCardPhase()
    {
        isPhase = true;

        await currentBattler.PlayCard();
        photonView.RPC(nameof(PlayCard), RpcTarget.Others, currentBattler.RecentCard.Base.Id);
        selectCard.DeleteCard();

        SetPhase(Phase.Move);
    }

    private async UniTask MovePhase()
    {
        isPhase = true;
        generateGame.ActiveCanMoveTiles();
        Vector2Int movedPos = await currentBattler.Move();
        //Debug.Log(movedPos);
        generateGame.RecieveMove(isMyClient: true, pos: movedPos);
        generateGame.DeactiveMoveTiles();
        //Debug.Log(currentBattler.BattlerMove.PiecePos);
        photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(currentBattler.BattlerMove.PiecePos));
        SetPhase(Phase.Attack);
    }

    /// <summary>
    /// �U���t�F�[�Y����
    /// </summary>
    /// <returns></returns>
    private async UniTask AttackPhase()
    {
        isPhase = true;

        // �g�p�����J�[�h�̏����L�^
        Card card = currentBattler.RecentCard;
        bool isMatch = (int)card.Base.Type == currentBattler.CurrentType.Value;
        int id = card.Base.Id;

        // �U���ꏊ�̌v�Z
        List<Vector2Int> attackPos;
        List<Vector2Int> recievePos;
        int damage;
        if (isMatch)
        {
            attackPos = card.Base.SAttackPos;
            damage = card.Base.SDamage;
        }
        else
        {
            attackPos = card.Base.AttackPos;
            damage = card.Base.Damage;
        }
        attackPos = Calculator.CalcAttackPosition(currentBattler.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcEnemyPosition(attackPos);

        // Effect
        Locator<Attack>.Instance.
            PlayEffect(recievePos, (int)card.Base.Type, isMatch, false);
        photonView.RPC(nameof(AttackEffect), RpcTarget.Others, id, isMatch);

        // Damage
        waitBattler.Damage(attackPos, damage);

        await UniTask.Delay(500); // �G�t�F�N�g�I���҂�
        
        SetPhase(Phase.End);
        CheckVictoryOrDefeat();
    }

    private async UniTask EndPhase()
    {
        isPhase = true;
        await UniTask.DelayFrame(10); // UI����
        
        SetPhase(Phase.Wait);
        
        ChangeTurn();
        photonView.RPC(nameof(ChangeTurn), RpcTarget.Others);
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
    }

    private void CheckVictoryOrDefeat()
    {
        if (waitBattler.Health.Value < 0)
        {
            SetPhase(Phase.Win);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
            return;
        }
        //Debug.Log(currentBattler.IsCostUses.All(result => result.Value == false));
        //Debug.Log(waitBattler.IsCostUses.All(result => result.Value == false));

        if (!(currentBattler.IsCostUses.All(result => result.Value == true) && waitBattler.IsCostUses.All(result => result.Value == true))) return;
        if (currentBattler.Health.Value > waitBattler.Health.Value)
        {
            SetPhase(Phase.Win);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
        }
        else
        {
            SetPhase(Phase.Lose);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        }
    }

    public void Surrender()
    {
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        SetPhase(Phase.Lose);
        inGameSetting.Hide();
    }

    private void WaitPhase()
    {
        waitBattler.IsWait = true;
    }

    [PunRPC]
    private void SetBattler(int coinTos)
    {

        if(coinTos == 0)
        {
            currentBattler = enemy;
            waitBattler = player;
            //Debug.Log("�z�X�g���G�A�Q�X�g������");
        }
        else
        {
            currentBattler = player;
            waitBattler = enemy;
        }
        
    }
    [PunRPC]
    private void SetPhase(Phase phase)
    {
        if (this.phase == Phase.Win || this.phase == Phase.Lose) return;
        isPhase = false;
        this.phase = phase;
        //Debug.Log($"{phase}");
    }
    [PunRPC]
    private void ChangeTurn()
    {
        Battler tmp = currentBattler;
        currentBattler = waitBattler;
        waitBattler = tmp;
    }
    [PunRPC]
    private void Draw(int id)
    {
        currentBattler.Draw(Locator<CardGenerator>.Instance.ChoiceDraw(id, CheckPlayer()));
    }
    [PunRPC]
    private void ReDraw(int id)
    {
        currentBattler.RemoveCard(id);
    }
    
    [PunRPC]
    private void ChangeType(int type, int id)
    {
        currentBattler.ChangeType(type);
        currentBattler.RemoveCard(id);
    }
    [PunRPC]
    private void PlayCard(int id)
    {
        currentBattler.PlayCard(id);
    }
    [PunRPC]
    private void Move(int pos)
    {
        //Debug.Log(pos);
        Vector2Int movedPos = Calculator.IntToVector2Int(pos);
        generateGame.RecieveMove(isMyClient: false, pos: movedPos);
    }
    [PunRPC]
    private void AttackEffect(int id, bool isMatch)
    {
        // �g�p���ꂽ�J�[�h��ID����J�[�h�̏����擾
        CardBase card = Locator<CardGenerator>.Instance.CardBases[id];

        List<Vector2Int> attackPos;
        int damage;
        int type = (int)card.Type;

        
        if (isMatch)
        {
            attackPos = card.SAttackPos;
            damage = card.SDamage;
        }
        else
        {
            attackPos = card.AttackPos;
            damage = card.Damage;
        }

        // �U���ꏊ�̌v�Z(���葤����̍U���Ȃ̂ōU���ꏊ�����f������)
        // Effect�Đ�
        List<Vector2Int> calcAttackPos = Calculator.CalcAttackPosition(currentBattler.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.AttackAhead);
        waitBattler.Damage(calcAttackPos, damage);

        calcAttackPos = Calculator.CalcReflection(calcAttackPos);
        Locator<Attack>.Instance.
            PlayEffect(calcAttackPos, type, isMatch, isEnemy: true);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        SetPhase(Phase.Win);
        //photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
    }
    public void OnClickTitle()
    {
        SceneManager.LoadScene("MatchingScene");
        PhotonNetwork.Disconnect();
        //photonView.RPC(nameof(LoadScene), RpcTarget.Others, "MatchingScene");
    }
    [PunRPC]
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
