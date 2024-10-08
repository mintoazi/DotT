using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;
// オープンカード×２バグ
// カード提出時　バグ　原因不明
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
        CheckCard, // ここまで共通フェーズ
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
        //Update処理の追加
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void OnDestroy() => Locator<GameMaster>.Unbind(this);
    private void Start()
    {
        Debug.Log("初期フェイズ");
        isOnline = PhotonNetwork.IsConnected;
        InitPhase().Forget();
    }

    private void ManagedUpdate()
    {
        if (CardGenerator.IsLoadingCSV) return; // カードデータ読み込み中は処理しない
        if (isEnemyTurn) waitPanel.SetActive(true);
        else waitPanel.SetActive(false);
        if (isPhase) return;
        isPhase = true;
        switch (phase)
        {
            case Phase.Init:
                //Debug.Log("初期フェイズ");

                break;
            case Phase.Draw:
                Debug.Log("ドローフェイズ");
                DrawPhase().Forget();
                break;
            case Phase.ReDraw:
                // 出せるカードがなかったら一枚捨てて一枚ドロー
                Debug.Log("リドローフェイズ");
                ReDrawPhase().Forget(); //=> PlayCard
                break;
            case Phase.PlayCard:
                // カード提出
                Debug.Log("カードプレイフェイズ");
                PlayCardPhase().Forget();//=> CheckCard
                break;
            case Phase.CheckCard:
                // カードのコスト比較
                Debug.Log("ターンセットフェイズ");
                //CheckTurnPhase().Forget();// => Move or Wait
                break;
            case Phase.Move:
                // 先攻移動
                Debug.Log("ムーブフェイズ");
                MovePhase().Forget();
                break;
            case Phase.Attack:
                // 先攻攻撃
                Debug.Log("アタックフェイズ");
                AttackPhase().Forget();
                break;
            case Phase.Support:
                //サポートカードのプレイ
                Debug.Log("サポートフェイズ");
                PlaySupportPhase().Forget();
                break;
            case Phase.End:
                //エンドフェーズ(初期化フェーズ)
                Debug.Log("エンドフェイズ");
                EndPhase().Forget();
                break;
            case Phase.Wait:
                if(!player.IsWait) Debug.Log("waitフェイズ");
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

    // 5枚になるまで引く : CPU設定完了
    private async UniTask DrawPhase()
    {
        int startCards = 5;
        isPhase = true;

        turns++;
        player.IsWait = false; // カードを変なタイミングで選択できないように

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
                Card c = Locator<CardGenerator>.Instance.Draw(true); // あとでtrueに変える
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

    // 使えるコストが無い場合　カードを引かせる : CPU絶対バグる
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
        
        // 相手の選択待ち
        isEnemyTurn = true;
        await UniTask.WaitUntil(()=>isEnemyReDraw);
        isEnemyTurn = false;
        
        await SetPhase(Phase.PlayCard);
    }

    // カードを使用するフェーズ : CPU設定完了
    private async UniTask PlayCardPhase()
    {
        isPhase = true;
        player.IsWait = false;
        submitTime = Time.time; // カードを出したタイミングを保存する用
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
        enemySubmitTime = waitTime; // ここを後で相談
        isEnemySubmited = true;
    }

    private async UniTask PlaySupportCard_CPU()
    {
        while (enemy.CanUseSupport)
        {
            await enemy.PlaySupportCard(cpu.PlaySupportCard().Base.Id); // カードオープンまで待つ
            await enemy.ResetSupportCard(); // カードの削除
        }
        isEnemyUsedSupport = true;
    }


    /// <summary>
    /// サポートカードをプレイさせる
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
            Debug.Log("サポートカードの使用");
            if(isOnline) photonView.RPC(nameof(PlaySupportCard), RpcTarget.Others, player.SupportCard.Base.Id); // 相手に使用したサポートを送信

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
    /// 提出したカードのコストが低い順に移動
    /// </summary>
    /// <returns></returns>
    private async UniTask CheckTurnPhase()
    {
        isEnemyTurn = true;
        await UniTask.WaitUntil(() => isEnemyUsedSupport);
        isEnemyTurn = false;
        await SetPhase(Phase.CheckCard);
        await enemy.AttackCard.OpenCard(); // 敵のカードをオープン
        
        // cost計算
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
        if(isOnline) photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(player.BattlerMove.PiecePos)); // 移動した情報を相手に送信
        await SetPhase(Phase.Attack);
    }

    /// <summary>
    /// 攻撃フェーズ処理
    /// </summary>
    /// <returns></returns>
    private async UniTask AttackPhase()
    {
        isPhase = true;

        // 使用したカードの情報を記録
        Card card = player.AttackCard;
        
        // タイプの検査
        bool isMatch = (int)card.Base.Type == player.Model.CharaType.Value;
        int id = card.Base.Id;

        // 攻撃時　コマの色変化　コストの消費
        player.BattlerMove.UpdatePieceType((int)card.Base.Type);
        //player.Model.ChangeType((int)card.Base.Type);
        player.Model.UseCost(card.Base.Cost);

        // 攻撃場所の計算
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
        await UniTask.WaitForSeconds(2f); // エフェクト終了待ち
        await SetPhase(Phase.Wait);
        await CheckVictoryOrDefeat();

        // 敵のターンの設定
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
                await UniTask.WaitForSeconds(2f); // エフェクト終了待ち
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

        player.Model.ResetBuffs(); // バフのリセット
        enemy.Model.ResetBuffs();　// バフのリセット

        // Drawからスタート
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
        //Debug.Log(phase.ToString() + "フェーズ");
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
        Debug.Log("相手のサポート使用");
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
        // 使用されたカードのIDからカードの情報を取得
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
        // 攻撃場所の計算(相手側からの攻撃なので攻撃場所を鏡映させる)
        // Effect再生
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
        isEnemyAttacked = true; // 相手が殴った
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
