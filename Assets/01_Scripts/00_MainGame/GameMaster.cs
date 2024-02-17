using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;
// �I�[�v���J�[�h�~�Q�o�O
// �J�[�h��o���@�o�O�@�����s��
public class GameMaster : MonoBehaviourPunCallbacks
{
    [SerializeField] Battler player;
    [SerializeField] Battler enemy;
    [SerializeField] CpuController cpu;
    [SerializeField] GenerateGame generateGame;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] InGameSetting inGameSetting;
    [SerializeField] CutInController cutInController;
    [SerializeField] DisplayCharacterInfo displayCharacterInfo;
    [SerializeField] CostJudge costJudge;
    private int turns = 0;

    bool isOnline = false;
    bool isEnd = false;
    bool isEnemySubmited = false;
    bool isEnemyAttacked = false;
    bool isEnemyUsedSupport = false;
    bool isEnemyReDraw = false;
    bool isEnemyTurn = false;
    float submitTime = 0f;
    float enemySubmitTime = 0f;

    private Phase phase = Phase.Init;
    bool isPhase = false;
    public enum Phase
    {
        Init,
        Draw,
        ReDraw,
        PlayCard,
        CheckCard, // �����܂ŋ��ʃt�F�[�Y
        Move,
        Attack,
        Support,
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
        isOnline = PhotonNetwork.IsConnected;
        InitPhase().Forget();
    }

    private void ManagedUpdate()
    {
        if (CardGenerator.IsLoadingCSV) return; // �J�[�h�f�[�^�ǂݍ��ݒ��͏������Ȃ�
        if (isEnemyTurn) waitPanel.SetActive(true);
        else waitPanel.SetActive(false);
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
            case Phase.PlayCard:
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
            case Phase.Support:
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
                break;
            case Phase.Lose:
                losePanel.SetActive(true);
                break;
        }
    }
    private async UniTask InitPhase()
    {
        int firstType, secondType;
        int firstDeck, secondDeck;
        isPhase = true;
        if (!isOnline)
        {
            firstType = PlayerData.Instance.PlayerChara;
            secondType = PlayerData.Instance.EnemyChara;
            firstDeck = PlayerData.Instance.PlayerDeck;
            secondDeck = PlayerData.Instance.EnemyDeck;
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            firstType = OnlineMenuManager.HostCharacter;
            secondType = OnlineMenuManager.GuestCharacter;
            firstDeck = OnlineMenuManager.HostDeck;
            secondDeck = OnlineMenuManager.GuestDeck;
        }
        else
        {
            firstType = OnlineMenuManager.GuestCharacter;
            secondType = OnlineMenuManager.HostCharacter;
            firstDeck = OnlineMenuManager.HostDeck;
            secondDeck = OnlineMenuManager.GuestDeck;
        }

        Locator<CardGenerator>.Instance.GenerateCard(firstDeck, secondDeck);
        //Debug.Log(firstType);
        player.Model.Init(20, firstType);
        player.BattlerMove.Init(firstType);
        enemy.Model.Init(20, secondType);
        enemy.BattlerMove.Init(secondType);
        displayCharacterInfo.SetData(firstType);
        await UniTask.Delay(1);
        
        await SetPhase(Phase.Draw);
    }

    // 5���ɂȂ�܂ň��� : CPU�ݒ芮��
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
            player.SetCardToHand(card);

            if (isOnline) photonView.RPC(nameof(Draw), RpcTarget.Others, card.Base.Id);
        }
        if (!isOnline)
        {
            currentCards = enemy.Hand.Hands.Count;
            for (int i = currentCards; i < startCards; i++)
            {
                Card c = Locator<CardGenerator>.Instance.Draw(true); // ���Ƃ�true�ɕς���
                enemy.SetCardToHand(c);
            }
        }
        await player.Hand.ResetPositions();
        if(isOnline)
        {
            photonView.RPC(nameof(ResetHandPosition), RpcTarget.Others);
        }
        else
        {
            enemy.Hand.ResetPositions().Forget();
        }
        await SetPhase(Phase.ReDraw);
    }

    // �g����R�X�g�������ꍇ�@�J�[�h���������� : CPU��΃o�O��
    private async UniTask ReDrawPhase()
    {
        isPhase = true;
        player.IsWait = false;
        List<int> costs = await player.ReDraw();
        if (costs != null)
        {
            Card card = Locator<CardGenerator>.Instance.ReDraw(costs, isEnemy: false);

            int removedCardID = player.RecentCard.Base.Id;
            int drewCardID = card.Base.Id;

            if (isOnline) photonView.RPC(nameof(ReDraw), RpcTarget.Others, removedCardID, drewCardID);
            player.SetCardToHand(card);
            await player.Hand.ResetPositions();
        }
        
        if(!isOnline)
        {
            Card enemyCard = cpu.ReDraw();
            if (enemyCard != null)
            {
                enemy.SetCardToHand(enemyCard);
                enemy.Hand.ResetPositions().Forget();
            }
            isEnemyReDraw = true;
        }
         else photonView.RPC(nameof(ReDrawed), RpcTarget.Others);
        
        // ����̑I��҂�
        isEnemyTurn = true;
        await UniTask.WaitUntil(()=>isEnemyReDraw);
        isEnemyTurn = false;
        
        await SetPhase(Phase.PlayCard);
    }

    // �J�[�h���g�p����t�F�[�Y : CPU�ݒ芮��
    private async UniTask PlayCardPhase()
    {
        isPhase = true;
        player.IsWait = false;
        submitTime = Time.time; // �J�[�h���o�����^�C�~���O��ۑ�����p
        if (!isOnline) PlayCard_CPU().Forget();
        await player.PlayCard();
        submitTime = Time.time - submitTime;

        CardBase card = player.AttackCard.Base;
        if (isOnline) photonView.RPC(nameof(PlayCard), RpcTarget.Others, card.Id, submitTime);

        await UniTask.WaitUntil(() => isEnemySubmited);

        if(!isOnline)
        {
            SetPhase(Phase.Support).Forget();
            return;
        }

        if (submitTime < enemySubmitTime) SetPhase(Phase.Support).Forget();
        else
        {
            SetPhase(Phase.Wait).Forget();
        }
    }

    private async UniTask PlayCard_CPU()
    {
        float waitTime = 2.0f;
        await UniTask.WaitForSeconds(waitTime);
        int id = cpu.PlayCard().Base.Id;
        enemy.PlayCard(id);
        enemySubmitTime = waitTime; // ��������ő��k
        isEnemySubmited = true;
    }

    private async UniTask PlaySupportCard_CPU()
    {
        while (enemy.CanUseSupport)
        {
            await enemy.PlaySupportCard(cpu.PlaySupportCard().Base.Id); // �J�[�h�I�[�v���܂ő҂�
            await enemy.ResetSupportCard(); // �J�[�h�̍폜
        }
        isEnemyUsedSupport = true;
    }


    /// <summary>
    /// �T�|�[�g�J�[�h���v���C������
    /// </summary>
    /// <returns></returns>
    private async UniTask PlaySupportPhase()
    {
        if (!isOnline && submitTime > enemySubmitTime)
        {
            isEnemyTurn = true;
            await PlaySupportCard_CPU();
            isEnemyTurn = false;
        }
        isPhase = true;
        player.IsWait = false;

        while (player.CanUseSupport)
        {
            await player.PlaySupport();
            Debug.Log("�T�|�[�g�J�[�h�̎g�p");
            if(isOnline) photonView.RPC(nameof(PlaySupportCard), RpcTarget.Others, player.SupportCard.Base.Id); // ����Ɏg�p�����T�|�[�g�𑗐M

            await player.ResetSupportCard();
        }

        player.IsWait = true;

        if (isOnline) photonView.RPC(nameof(UsedSupportCard), RpcTarget.Others);

        if (!isEnemyUsedSupport)
        {
            if (isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Support);
            else PlaySupportCard_CPU().Forget();
        }

        CheckTurnPhase().Forget();
    }

    public void ActiveAttackTiles(Card card)
    {
        if (phase != Phase.PlayCard) return;
        List<Vector2Int> attackPos;
        List<Vector2Int> recievePos;
        attackPos = card.Base.AttackPos;

        attackPos = Calculator.CalcAttackPosition(player.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcEnemyPosition(attackPos);
        generateGame.ActiveAttackRangeTiles(recievePos);
    }

    public void ActiveAttackTiles(Vector2Int prePlayerPos)
    {
        List<Vector2Int> attackPos;
        List<Vector2Int> recievePos;
        Card card = player.AttackCard;
        attackPos = card.Base.AttackPos;

        attackPos = Calculator.CalcAttackPosition(prePlayerPos,
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
        isEnemyTurn = true;
        await UniTask.WaitUntil(() => isEnemyUsedSupport);
        isEnemyTurn = false;
        await SetPhase(Phase.CheckCard);
        await enemy.AttackCard.OpenCard(); // �G�̃J�[�h���I�[�v��
        
        // cost�v�Z
        int pCost = Mathf.Max(1, player.AttackCard.Base.Cost + 1 - player.Model.CostBuff.Value); 
        int eCost = Mathf.Max(1, enemy.AttackCard.Base.Cost + 1 - enemy.Model.CostBuff.Value);

        costJudge.gameObject.SetActive(true);
        costJudge.Set(player.AttackCard, player.Model.CostBuff.Value, pCost, enemy.AttackCard, enemy.Model.CostBuff.Value, eCost);
        costJudge.SetTimer(submitTime, enemySubmitTime);

        CostJudge.Winner winner = CheckMoveFirst();

        await costJudge.StartJudge(winner);
        costJudge.gameObject.SetActive(false);

        if (winner == CostJudge.Winner.Player)
        {
            await SetPhase(Phase.Move);
        }
        else
        {
            await SetPhase(Phase.Wait);
            if (!isOnline)
            {
                await Move_CPU();
                await SetPhase(Phase.Move);
            }
        }

        player.IsWait = true;

        CostJudge.Winner CheckMoveFirst()
        {
            if (pCost == eCost)
            {
                if (submitTime < enemySubmitTime) return CostJudge.Winner.Player;
                else
                {
                    return CostJudge.Winner.Enemy;
                }
            }
            else if (pCost < eCost)
            {
                return CostJudge.Winner.Player;
            }
            else
            {
                return CostJudge.Winner.Enemy;
            }
        }
    }

    private async UniTask Move_CPU()
    {
        if (isEnd) return;
        await UniTask.WaitForSeconds(0.5f);
        Move(cpu.MovePos());
        AttackEffect(enemy.AttackCard.Base.Id, enemy.IsMatchCharaType);
        await CheckVictoryOrDefeat();
        await UniTask.WaitForSeconds(2f);
    }

    private async UniTask MovePhase()
    {
        generateGame.ActiveCanMoveTiles();
        Vector2Int movedPos = await player.Move();
        //Debug.Log(movedPos);
        generateGame.RecieveMove(isMyClient: true, pos: movedPos);
        generateGame.DeactiveMoveTiles();
        //Debug.Log(currentBattler.BattlerMove.PiecePos);
        if(isOnline) photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(player.BattlerMove.PiecePos)); // �ړ��������𑊎�ɑ��M
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
        Card card = player.AttackCard;
        
        // �^�C�v�̌���
        bool isMatch = (int)card.Base.Type == player.Model.CharaType.Value;
        int id = card.Base.Id;

        // �U�����@�R�}�̐F�ω��@�R�X�g�̏���
        player.BattlerMove.UpdatePieceType((int)card.Base.Type);
        //player.Model.ChangeType((int)card.Base.Type);
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
        if (damage < 0) damage = 0;

        attackPos = Calculator.CalcAttackPosition(player.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.Base.AttackAhead);
        recievePos = Calculator.CalcEnemyPosition(attackPos);

        // Effect
        Locator<Attack>.Instance.
            PlayEffect(recievePos, (int)card.Base.Type, isMatch, false);
        if (isOnline) photonView.RPC(nameof(AttackEffect), RpcTarget.Others, id, isMatch);

        // Damage
        int reciveDamage = enemy.Damage(attackPos, damage);
        if (player.Model.CharaType.Value == (int)CardTypeM.Curse && player.Model.CharaType.Value == (int)card.Base.Type)
        {
            player.Model.Heal(reciveDamage);
            if (isOnline) photonView.RPC(nameof(Heal), RpcTarget.Others, damage);
        }

        player.ResetAttackCard();
        await UniTask.WaitForSeconds(2f); // �G�t�F�N�g�I���҂�
        await SetPhase(Phase.Wait);
        await CheckVictoryOrDefeat();

        // �G�̃^�[���̐ݒ�
        if (isEnemyAttacked)
        {
            if(isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.End);
            await SetPhase(Phase.End);
        }
        else
        {
            await SetPhase(Phase.Wait);
            if(isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Move);
            else
            {
                await Move_CPU();
                await UniTask.WaitForSeconds(2f); // �G�t�F�N�g�I���҂�
                await SetPhase(Phase.End);
            }
        }
    }
    
    private async UniTask EndPhase()
    {
        isPhase = true;
        await CheckVictoryOrDefeat();

        isEnemyAttacked = false;
        isEnemySubmited = false;
        isEnemyUsedSupport = false;
        isEnemyReDraw = false;

        player.Model.ResetBuffs(); // �o�t�̃��Z�b�g
        enemy.Model.ResetBuffs();�@// �o�t�̃��Z�b�g

        // Draw����X�^�[�g
        await SetPhase(Phase.Draw);
        //photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
    }

    private async UniTask CheckVictoryOrDefeat()
    {
        if (enemy.Model.Health.Value <= 0)
        {
            await SetPhase(Phase.Win);
            isEnd = true;
            return;
        }
        if(player.Model.Health.Value <= 0)
        {
            await SetPhase(Phase.Lose);
            isEnd = true;
            return;
        }
        //Debug.Log(currentBattler.IsCostUses.All(result => result.Value == false));
        //Debug.Log(waitBattler.IsCostUses.All(result => result.Value == false));

        if (!(player.Model.IsCostUses.All(result => result.Value == true) && enemy.Model.IsCostUses.All(result => result.Value == true))) return;
        if (player.Model.Health.Value > enemy.Model.Health.Value)
        {
            await SetPhase(Phase.Win);
            isEnd = true;
            if (isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
        }
        else
        {
            await SetPhase(Phase.Lose);
            isEnd = true;
            if (isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        }
    }

    public void Surrender()
    {
        if (isOnline) photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
        SetPhase(Phase.Lose).Forget();
        inGameSetting.Hide();
    }

    [SerializeField] private GameObject waitPanel;
    private void WaitPhase()
    {
        waitPanel.SetActive(true);
        player.IsWait = true;
    }
    
    [PunRPC]
    private async UniTask SetPhase(Phase phase)
    {
        waitPanel.SetActive(false);
        if (this.phase == Phase.Win || this.phase == Phase.Lose) return;
        this.phase = phase;
        //Debug.Log(phase.ToString() + "�t�F�[�Y");
        await cutInController.CutInPhase(phase);
        isPhase = false;
    }

    [PunRPC]
    private void Draw(int id)
    {
        Card card = Locator<CardGenerator>.Instance.ChoiceDraw(id, isEnemy: true);
        enemy.SetCardToHand(card);
    }
    [PunRPC]
    private void ResetHandPosition()
    {
        enemy.Hand.ResetPositions().Forget();
    }
    [PunRPC]
    private void ReDraw(int removeID, int drawID)
    {
        enemy.RemoveCard(removeID);
        Draw(drawID);
        enemy.Hand.ResetPositions().Forget();
        ReDrawed();
    }
    [PunRPC]
    private void ReDrawed()
    {
        isEnemyReDraw = true;
    }
    
    [PunRPC]
    private void PlayCard(int id, float time)
    {
        enemy.PlayCard(id);
        enemySubmitTime = time;
        isEnemySubmited = true;
    }
    [PunRPC]
    private async UniTask PlaySupportCard(int id)
    {
        //enemy.Model.SetHp(health);
        Debug.Log("����̃T�|�[�g�g�p");
        await enemy.PlaySupportCard(id);
        await enemy.ResetSupportCard();
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
        damage += enemy.Model.AttackBuff.Value - player.Model.DefenceBuff.Value;
        if (damage < 0) damage = 0;
        // �U���ꏊ�̌v�Z(���葤����̍U���Ȃ̂ōU���ꏊ�����f������)
        // Effect�Đ�
        List<Vector2Int> calcAttackPos = Calculator.CalcAttackPosition(enemy.BattlerMove.PiecePos,
                                                  attackPos,
                                                  card.AttackAhead);
        int recieveDamage = player.Damage(calcAttackPos, damage);
        if (!isOnline)
        {
            if (enemy.Model.CharaType.Value == (int)CardTypeM.Curse && enemy.Model.CharaType.Value == (int)card.Type)
            {
                enemy.Model.Heal(recieveDamage);
            }
        }
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
        SetPhase(Phase.Win).Forget();
        //photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
    }
    public void OnClickTitle()
    {
        Disconnect();
        SceneLoader.Instance.Load(Scenes.Scene.HOME).Forget();
        
        //photonView.RPC(nameof(LoadScene), RpcTarget.Others, "MatchingScene");
    }
    [PunRPC]
    public void LoadScene(string sceneName)
    {
        Disconnect();
        SceneManager.LoadScene(sceneName);
    }

    private void Disconnect()
    {
        if (isOnline)
        {
            PhotonNetwork.Disconnect();
            //if(OnlineMenuManager.onlineManager.gameObject != null) Destroy(OnlineMenuManager.onlineManager.gameObject);
        }
    }
}
