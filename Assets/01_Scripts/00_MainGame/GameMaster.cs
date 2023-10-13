using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections.Generic;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] Battler player;
    [SerializeField] Battler enemy;
    [SerializeField] GenerateGame generateGame;
    [SerializeField] SelectCard selectCard;
    [SerializeField] int hands = 3;
    private Battler currentBattler;
    private Battler waitBattler;

    private Phase phase = Phase.Init;
    bool isPhase = false;
    private enum Phase
    {
        Init,
        CoinTos,

        Draw,
        ReDraw,
        ChangeType,
        PlayCard,
        Move,
        Attack,
        End,

        Wait
    }

    private void Awake()
    {
        generateGame.GenerateTiles();
    }

    private void ManagedUpdate()
    {
        if (isPhase) return;
        switch (phase)
        {
            case Phase.Init:
                Debug.Log("�����t�F�C�Y");
                InitPhase();
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
            case Phase.ChangeType:
                Debug.Log("�����ύX�t�F�C�Y");
                ChangeTypePhase().Forget();
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
                break;
        }
    }
    private void InitPhase()
    {
        //player.OnSubmitAction = SubmittedAction;
        //Update�����̒ǉ�
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );

        int playerType = 0;
        int enemyType = 0;

        if (PhotonNetwork.IsMasterClient)
        {
            playerType = OnlineMenuManager.HostCharacter;
            enemyType = OnlineMenuManager.GuestCharacter;
            player.Init(20, playerType);//demo
            enemy.Init(20, enemyType);
        }
        else
        {
            playerType = OnlineMenuManager.GuestCharacter;
            enemyType = OnlineMenuManager.HostCharacter;
            player.Init(20, playerType);//demo
            enemy.Init(20, enemyType);
        }

        SendCardsTo(player, isEnemy: false);
        SendCardsTo(enemy, isEnemy: true);

        void SendCardsTo(Battler target, bool isEnemy)
        {
            for (int i = 0; i < hands; i++)
            {
                Debug.Log("�h���[" + i);
                Card card = Locator<CardGenerator>.Instance.Draw(isEnemy);
                target.SetCardToHand(card);
            }
        }
        phase = Phase.CoinTos;
    }

    // �^�[���̐ݒ�
    private void CoinTos()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int rand = 0;
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

        for (int i = 0; i < 2; i++)
        {
            Card card = Locator<CardGenerator>.Instance.Draw(CheckPlayer());
            currentBattler.Draw(card);
            photonView.RPC(nameof(Draw), RpcTarget.Others, card.Base.Id);
        }

        phase = Phase.ChangeType;
        isPhase = false;
    }
    private async UniTask ReDrawPhase()
    {
        isPhase = true;

        List<int> costs = await currentBattler.ReDraw(CheckPlayer());
        if (costs == null)
        {
            phase = Phase.PlayCard;
            isPhase = false;
            return;
        }
        Card card = Locator<CardGenerator>.Instance.ReDraw(costs, CheckPlayer());
        
        photonView.RPC(nameof(ReDraw), RpcTarget.Others, card.Base.Id);
        currentBattler.SetCardToHand(card);
        selectCard.DeleteCard();

        phase = Phase.PlayCard;
        isPhase = false;
    }

    private async UniTask ChangeTypePhase()
    {
        isPhase = true;
        
        int type = await currentBattler.ChangeType();
        photonView.RPC(nameof(ChangeType), RpcTarget.Others, type);
        selectCard.DeleteCard();

        phase = Phase.ReDraw;
        isPhase = false;
    }

    private async UniTask PlayCardPhase()
    {
        isPhase = true;

        await currentBattler.PlayCard();
        selectCard.DeleteCard();
        
        phase = Phase.Move;
        isPhase = false;
    }
    
    private async UniTask MovePhase()
    {
        isPhase = true;
        generateGame.ActiveCanMoveTiles();
        await currentBattler.Move();
        generateGame.DeactiveMoveTiles();
        photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(currentBattler.BattlerMove.PiecePos));
        phase = Phase.Attack;
        isPhase = false;
    }

    private async UniTask AttackPhase()
    {
        isPhase = true;

        Card card = currentBattler.RecentCard;
        bool isMatch = (int)card.Base.Type == currentBattler.CurrentType.Value;
        int id = card.Base.Id;

        List<Vector2Int> attackPos;
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

        // Effect
        Locator<Attack>.Instance.
            PlayEffect(Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, attackPos), (int)card.Base.Type, isMatch, !PhotonNetwork.IsMasterClient);
        photonView.RPC(nameof(AttackEffect), RpcTarget.Others, id, (int)card.Base.Type, isMatch);

        // Damage
        waitBattler.Damage(Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, attackPos), damage);
        photonView.RPC(nameof(Damage), RpcTarget.Others, id, damage, isMatch);

        await UniTask.Delay(500); // �G�t�F�N�g�I���҂�

        isPhase = false;
        phase = Phase.End;
    }

    private async UniTask EndPhase()
    {
        isPhase = true;
        await UniTask.DelayFrame(10); // UI����
        phase = Phase.Wait;
        ChangeTurn();
        photonView.RPC(nameof(ChangeTurn), RpcTarget.Others);
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
        isPhase = false;
    }

    [PunRPC]
    private void SetBattler(int coinTos)
    {

        if(coinTos == 0)
        {
            currentBattler = enemy;
            waitBattler = player;
            Debug.Log("�z�X�g���G�A�Q�X�g������");
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
        this.phase = phase;
    }
    [PunRPC]
    private void ChangeTurn()
    {
        Battler tmp = currentBattler;
        currentBattler = waitBattler;
        waitBattler = currentBattler;
    }
    [PunRPC]
    private void Draw(int id)
    {
        Debug.Log(currentBattler);
        Debug.Log(CheckPlayer());
        currentBattler.Draw(Locator<CardGenerator>.Instance.ChoiceDraw(id, CheckPlayer()));
    }
    [PunRPC]
    private void ReDraw(int id)
    {
        currentBattler.RemoveCard(id);
    }
    [PunRPC]
    private void ChangeType(int type)
    {
        currentBattler.ChangeType(type);
    }
    [PunRPC]
    private void PlayCard(int id)
    {
        currentBattler.PlayCard(id);
    }
    [PunRPC]
    private void Move(int pos)
    {
        currentBattler.Moved(Calculator.IntToVector2Int(pos));
    }
    [PunRPC]
    private void AttackEffect(int id, int type, bool isMatch)
    {
        List<Vector2Int> attackPos;
        if (isMatch) attackPos = Locator<CardGenerator>.Instance.CardBases[id].SAttackPos;
        else attackPos = Locator<CardGenerator>.Instance.CardBases[id].AttackPos;

        Locator<Attack>.Instance.
            PlayEffect(attackPos, type, isMatch, !PhotonNetwork.IsMasterClient);
    }
    [PunRPC]
    private void Damage(int id, int damage, bool isMatch)
    {
        List<Vector2Int> attackPos;
        if (isMatch) attackPos = Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, Locator<CardGenerator>.Instance.CardBases[id].SAttackPos);
        else attackPos = Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, Locator<CardGenerator>.Instance.CardBases[id].AttackPos);

        waitBattler.Damage(attackPos, damage);
    }
}
