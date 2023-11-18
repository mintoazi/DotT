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
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] InGameSetting inGameSetting;
    private int turns = 0;
    
    bool isEnemySubmited = false;
    bool isEnemyAttacked = false;
    bool isEnemyUsedSupport = false;
    bool isEnemyEnd = false;
    float submitTime = 0f;
    float enemySubmitTime = 0f;

    private Phase phase = Phase.Init;
    bool isPhase = false;
    public enum Phase
    {
        Init,
        Draw,
        ReDraw,
        PlayAttackCard,
        CheckCard, // �����܂ŋ��ʃt�F�[�Y
        Move,
        Attack,
        PlaySupportCard,
        End,
        Win,
        Lose,
        Wait
    }

    private void Awake()
    {
        generateGame.GenerateTiles();
        Locator<GameMaster>.Bind(this);
        //player.OnSubmitAction = SubmittedAction;
        //Update�����̒ǉ�
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void OnDestroy() => Locator<GameMaster>.Unbind(this);
    private void Start()
    {
        Debug.Log("�����t�F�C�Y");
        InitPhase().Forget();
    }

    private void ManagedUpdate()
    {
        if (CardGenerator.IsLoadingCSV) return; // �J�[�h�f�[�^�ǂݍ��ݒ��͏������Ȃ�

        if (isPhase) return;
        isPhase = true;
        switch (phase)
        {
            case Phase.Init:
                //Debug.Log("�����t�F�C�Y");

                break;
            case Phase.Draw:
                Debug.Log("�h���[�t�F�C�Y");
                DrawPhase().Forget();
                break;
            case Phase.ReDraw:
                // �o����J�[�h���Ȃ�������ꖇ�̂ĂĈꖇ�h���[
                Debug.Log("���h���[�t�F�C�Y");
                ReDrawPhase().Forget(); //=> PlayCard
                break;
            case Phase.PlayAttackCard:
                // �J�[�h��o
                Debug.Log("�J�[�h�v���C�t�F�C�Y");
                PlayCardPhase().Forget();//=> CheckCard
                break;
            case Phase.CheckCard:
                // �J�[�h�̃R�X�g��r
                Debug.Log("�^�[���Z�b�g�t�F�C�Y");
                //CheckTurnPhase().Forget();// => Move or Wait
                break;
            case Phase.Move:
                // ��U�ړ�
                Debug.Log("���[�u�t�F�C�Y");
                MovePhase().Forget();
                break;
            case Phase.Attack:
                // ��U�U��
                Debug.Log("�A�^�b�N�t�F�C�Y");
                AttackPhase().Forget();
                break;
            case Phase.PlaySupportCard:
                //�T�|�[�g�J�[�h�̃v���C
                Debug.Log("�T�|�[�g�t�F�C�Y");
                PlaySupportPhase().Forget();
                break;
            case Phase.End:
                //�G���h�t�F�[�Y(�������t�F�[�Y)
                Debug.Log("�G���h�t�F�C�Y");
                EndPhase().Forget();
                break;
            case Phase.Wait:
                if(!player.IsWait) Debug.Log("wait�t�F�C�Y");
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
    private async UniTask InitPhase()
    {
        int firstType = 0;
        int secondType = 0;
        isPhase = true;
        if (PhotonNetwork.IsMasterClient)
        {
            firstType = OnlineMenuManager.HostCharacter;
            secondType = OnlineMenuManager.GuestCharacter;
        }
        else
        {
            firstType = OnlineMenuManager.GuestCharacter;
            secondType = OnlineMenuManager.HostCharacter;
        }
        player.Model.Init(20, firstType);
        player.BattlerMove.Init(firstType);
        enemy.Model.Init(20, secondType);
        enemy.BattlerMove.Init(secondType);

        await UniTask.Delay(1);
        
        await SetPhase(Phase.Draw);
    }

    // 5���ɂȂ�܂ň���
    private async UniTask DrawPhase()
    {
        int startCards = 5;
        isPhase = true;

        turns++;
        player.IsWait = false; // �J�[�h��ςȃ^�C�~���O�őI���ł��Ȃ��悤��

        int currentCards = player.Hand.Hands.Count;
        for (int i = currentCards; i < startCards; i++)
        {
            Card card = Locator<CardGenerator>.Instance.Draw(false);
            player.Draw(card);
            photonView.RPC(nameof(Draw), RpcTarget.Others, card.Base.Id); 
        }
        await SetPhase(Phase.ReDraw);
    }
    private async UniTask ReDrawPhase()
    {
        isPhase = true;
        player.IsWait = false;
        List<int> costs = await player.ReDraw();
        if (costs == null)
        {
            await SetPhase(Phase.PlayAttackCard);
            return;
        }
        Card card = Locator<CardGenerator>.Instance.ReDraw(costs, isEnemy: false);

        photonView.RPC(nameof(ReDraw), RpcTarget.Others, card.Base.Id);
        player.SetCardToHand(card);

        await SetPhase(Phase.PlayAttackCard);
    }

    private async UniTask PlayCardPhase()
    {
        isPhase = true;
        player.IsWait = false;
        submitTime = Time.time; // �J�[�h���o�����^�C�~���O��ۑ�����p
        await player.PlayCard();
        submitTime = Time.time - submitTime;

        CardBase card = player.RecentCard.Base;
        photonView.RPC(nameof(PlayCard), RpcTarget.Others, card.Id, submitTime);

        CheckTurnPhase().Forget();
    }

    public void ActiveAttackTiles(Card card)
    {
        if (phase != Phase.PlayAttackCard) return;
        List<Vector2Int> attackPos;
        List<Vector2Int> recievePos;
        attackPos = card.Base.AttackPos;

        attackPos = Calculator.CalcAttackPosition(player.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcEnemyPosition(attackPos);
        generateGame.ActiveAttackRangeTiles(recievePos);
    }
    
    /// <summary>
    /// ��o�����J�[�h�̃R�X�g���Ⴂ���Ɉړ�
    /// </summary>
    /// <returns></returns>
    private async UniTask CheckTurnPhase()
    {
        isPhase = true;
        await UniTask.WaitUntil(() => isEnemySubmited);
        await SetPhase(Phase.CheckCard);
        await enemy.RecentCard.OpenCard(); // �G�̃J�[�h���I�[�v��
        
        // cost�v�Z
        int pCost = player.RecentCard.Base.Cost - player.Model.CostBuff.Value; 
        int eCost = enemy.RecentCard.Base.Cost - enemy.Model.CostBuff.Value;
        if (pCost == eCost)
        {
            if (submitTime < enemySubmitTime) await SetPhase(Phase.Move);
            else await SetPhase(Phase.Wait);
            // ������������
        }
        else if(pCost < eCost)
        {
            await SetPhase(Phase.Move);
        }
        else
        {
            await SetPhase(Phase.Wait);
        }

        player.IsWait = true;
    }
    private async UniTask MovePhase()
    {
        generateGame.ActiveCanMoveTiles();
        Vector2Int movedPos = await player.Move();
        //Debug.Log(movedPos);
        generateGame.RecieveMove(isMyClient: true, pos: movedPos);
        generateGame.DeactiveMoveTiles();
        //Debug.Log(currentBattler.BattlerMove.PiecePos);
        photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(player.BattlerMove.PiecePos));
        await SetPhase(Phase.Attack);
    }

    /// <summary>
    /// �U���t�F�[�Y����
    /// </summary>
    /// <returns></returns>
    private async UniTask AttackPhase()
    {
        isPhase = true;

        // �g�p�����J�[�h�̏����L�^
        Card card = player.RecentCard;
        
        // �^�C�v�̌���
        bool isMatch = (int)card.Base.Type == player.Model.CurrentType.Value;
        int id = card.Base.Id;

        // �U�����@�R�}�̐F�ω��@�R�X�g�̏���
        player.BattlerMove.UpdatePieceType((int)card.Base.Type);
        player.Model.ChangeType((int)card.Base.Type);
        player.Model.UseCost(card.Base.Cost);

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
        damage += player.Model.AttackBuff.Value - enemy.Model.DefenceBuff.Value;

        attackPos = Calculator.CalcAttackPosition(player.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcEnemyPosition(attackPos);

        // Effect
        Locator<Attack>.Instance.
            PlayEffect(recievePos, (int)card.Base.Type, isMatch, false);
        photonView.RPC(nameof(AttackEffect), RpcTarget.Others, id, isMatch);

        // Damage
        enemy.Damage(attackPos, damage);
        if (player.OldIsMatchCharaType && player.Model.CharaType.Value == (int)CardTypeM.Curse)
        {
            player.Model.Heal(damage);
            photonView.RPC(nameof(Heal), RpcTarget.Others, damage);
        }


        await UniTask.Delay(500); // �G�t�F�N�g�I���҂�

        await SetPhase(Phase.Wait);
        await CheckVictoryOrDefeat();
        
        // �G�̃^�[���̐ݒ�
        if (isEnemyAttacked) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.PlaySupportCard);
        else photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Move);
    }
    /// <summary>
    /// �T�|�[�g�J�[�h���v���C������
    /// </summary>
    /// <returns></returns>
    private async UniTask PlaySupportPhase()
    {
        isPhase = true;
        player.IsWait = false;
        player.Model.ResetBuffs();
        enemy.Model.ResetBuffs();
        while(player.CanUseSupport)
        {
            await player.PlaySupport();
            photonView.RPC(nameof(PlaySupportCard), RpcTarget.Others, player.RecentCard.Base.Id, player.Model.Health.Value);
            
            await UniTask.WaitForSeconds(1.0f);
            player.SelectCard.DeleteCard();
        }

        if (isEnemyUsedSupport)
        {
            await SetPhase(Phase.End);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.End);
        }
        else
        {
            await SetPhase(Phase.Wait);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.PlaySupportCard);
            photonView.RPC(nameof(UsedSupportCard), RpcTarget.Others);
        }
        player.IsWait = true;
    }
    private async UniTask EndPhase()
    {
        isPhase = true;
        await CheckVictoryOrDefeat();

        isEnemyAttacked = false;
        isEnemySubmited = false;
        isEnemyUsedSupport = false;
        isEnemyEnd = false;

        // Draw����X�^�[�g
        await SetPhase(Phase.Draw);
        //photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
    }

    private async UniTask CheckVictoryOrDefeat()
    {
        if (enemy.Model.Health.Value < 0)
        {
            await SetPhase(Phase.Win);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
            return;
        }
        //Debug.Log(currentBattler.IsCostUses.All(result => result.Value == false));
        //Debug.Log(waitBattler.IsCostUses.All(result => result.Value == false));

        if (!(player.Model.IsCostUses.All(result => result.Value == true) && enemy.Model.IsCostUses.All(result => result.Value == true))) return;
        if (player.Model.Health.Value > enemy.Model.Health.Value)
        {
            await SetPhase(Phase.Win);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
        }
        else
        {
            await SetPhase(Phase.Lose);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        }
    }

    public async UniTask Surrender()
    {
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        await SetPhase(Phase.Lose);
        inGameSetting.Hide();
    }

    private void WaitPhase()
    {
        player.IsWait = true;
    }
    [SerializeField] private PhaseUI phaseUI;
    [PunRPC]
    private async UniTask SetPhase(Phase phase)
    {
        if (this.phase == Phase.Win || this.phase == Phase.Lose) return;
        this.phase = phase;
        await phaseUI.Display(phase);
        isPhase = false;
    }

    [PunRPC]
    private void Draw(int id)
    {
        enemy.Draw(Locator<CardGenerator>.Instance.ChoiceDraw(id, isEnemy: true));
    }
    [PunRPC]
    private void ReDraw(int id)
    {
        enemy.RemoveCard(id);
    }
    
    [PunRPC]
    private void PlayCard(int id, float time)
    {
        enemy.PlayCard(id);
        enemySubmitTime = time;
        isEnemySubmited = true;
    }
    [PunRPC]
    private async UniTask PlaySupportCard(int id, int health)
    {
        //enemy.Model.SetHp(health);
        enemy.PlaySupportCard(id);
        await enemy.RecentCard.OpenCard();
        await UniTask.WaitForSeconds(1.0f);
        enemy.SelectCard.DeleteCard();
    }
    [PunRPC]
    private void Move(int pos)
    {
        //Debug.Log(pos);
        Vector2Int movedPos = Calculator.IntToVector2Int(pos);
        generateGame.RecieveMove(isMyClient: false, pos: movedPos);
        enemy.SelectCard.DeleteCard();
    }
    [PunRPC]
    private void UsedSupportCard()
    {
        isEnemyUsedSupport = true;
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
        List<Vector2Int> calcAttackPos = Calculator.CalcAttackPosition(enemy.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.AttackAhead);
        player.Damage(calcAttackPos, damage + enemy.Model.AttackBuff.Value - player.Model.DefenceBuff.Value);

        calcAttackPos = Calculator.CalcReflection(calcAttackPos);
        Locator<Attack>.Instance.
            PlayEffect(calcAttackPos, type, isMatch, isEnemy: true);
        enemy.Attack();
        isEnemyAttacked = true; // ���肪������
    }
    [PunRPC]
    private void Heal(int value)
    {
        enemy.Model.Heal(value);
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
