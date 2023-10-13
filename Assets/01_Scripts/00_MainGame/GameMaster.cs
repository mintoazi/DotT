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
        ChangeType,
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
        //Update処理の追加
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void Start()
    {
        Debug.Log("初期フェイズ");
        InitPhase();
    }

    private void ManagedUpdate()
    {
        if (isPhase) return;
        switch (phase)
        {
            case Phase.Init:
                Debug.Log("初期フェイズ");
                //InitPhase();
                break;
            case Phase.CoinTos:
                Debug.Log("コイントスフェイズ");
                CoinTos();
                break;
            case Phase.Draw:
                Debug.Log("ドローフェイズ");
                DrawPhase();
                break;
            case Phase.ReDraw:
                Debug.Log("リドローフェイズ");
                ReDrawPhase().Forget();
                break;
            case Phase.ChangeType:
                Debug.Log("属性変更フェイズ");
                ChangeTypePhase().Forget();
                break;
            case Phase.PlayCard:
                Debug.Log("カードプレイフェイズ");
                PlayCardPhase().Forget();
                break;
            case Phase.Move:
                Debug.Log("移動フェイズ");
                MovePhase().Forget();
                break;
            case Phase.Attack:
                Debug.Log("攻撃フェイズ");
                AttackPhase().Forget();
                break;
            case Phase.End:
                Debug.Log("エンドフェイズ");
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
                //Debug.Log("ドロー" + i);
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

    // ターンの設定
    private void CoinTos()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        int rand = 0;
        if (rand == 0) // hostのターン
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

    // ２枚引く
    private void DrawPhase()
    {
        isPhase = true;

        turns++;
        currentBattler.IsWait = false;
        for (int i = 0; i < 2; i++)
        {
            Debug.Log(currentBattler.gameObject.name);
            Card card = Locator<CardGenerator>.Instance.Draw(CheckPlayer());
            currentBattler.Draw(card);
            photonView.RPC(nameof(Draw), RpcTarget.Others, card.Base.Id);
        }

        SetPhase(Phase.ChangeType);
        isPhase = false;
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
        isPhase = false;
    }

    private async UniTask ChangeTypePhase()
    {
        isPhase = true;

        int type = await currentBattler.ChangeType();
        photonView.RPC(nameof(ChangeType), RpcTarget.Others, type, currentBattler.RecentCard.Base.Id);
        selectCard.DeleteCard();

        SetPhase(Phase.ReDraw);
        isPhase = false;
    }

    private async UniTask PlayCardPhase()
    {
        isPhase = true;

        await currentBattler.PlayCard();
        photonView.RPC(nameof(PlayCard), RpcTarget.Others, currentBattler.RecentCard.Base.Id);
        selectCard.DeleteCard();

        SetPhase(Phase.Move);
        isPhase = false;
    }

    private async UniTask MovePhase()
    {
        isPhase = true;
        generateGame.ActiveCanMoveTiles();
        Vector2Int movedPos = await currentBattler.Move();
        Debug.Log(movedPos);
        generateGame.RecieveMove(isMyClient: true, pos: movedPos);
        generateGame.DeactiveMoveTiles();
        //Debug.Log(currentBattler.BattlerMove.PiecePos);
        photonView.RPC(nameof(Move), RpcTarget.Others, Calculator.Vector2IntToInt(currentBattler.BattlerMove.PiecePos));
        SetPhase(Phase.Attack);
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
            PlayEffect(Calculator.CalcEffectPos(currentBattler.BattlerMove.PiecePos, attackPos, card.Base.AttackAhead), (int)card.Base.Type, isMatch, false);
        photonView.RPC(nameof(AttackEffect), RpcTarget.Others, id, (int)card.Base.Type, isMatch);

        // Damage
        waitBattler.Damage(Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, attackPos), damage);
        photonView.RPC(nameof(Damage), RpcTarget.Others, id, damage, isMatch);

        await UniTask.Delay(500); // エフェクト終了待ち

        isPhase = false;
        
        SetPhase(Phase.End);
        CheckVictoryOrDefeat();
    }

    private async UniTask EndPhase()
    {
        isPhase = true;
        await UniTask.DelayFrame(10); // UI処理
        
        SetPhase(Phase.Wait);
        
        ChangeTurn();
        photonView.RPC(nameof(ChangeTurn), RpcTarget.Others);
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Draw);
        
        isPhase = false;
    }

    private void CheckVictoryOrDefeat()
    {
        if (waitBattler.Health.Value < 0)
        {
            SetPhase(Phase.Win);
            photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
            return;
        }
        if (currentBattler.IsCostUses.All(result => result.Value == false) && waitBattler.IsCostUses.All(result => result.Value == false)) return;
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
        SetPhase(Phase.Lose);
        photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Win);
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
            //Debug.Log("ホストが敵、ゲストが味方");
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
    private void AttackEffect(int id, int type, bool isMatch)
    {
        List<Vector2Int> attackPos;
        CardBase card = Locator<CardGenerator>.Instance.CardBases[id];
        if (isMatch) attackPos = card.SAttackPos;
        else attackPos = card.AttackPos;
        List <Vector2Int> calcAttackPos = Calculator.CalcEffectPos(currentBattler.BattlerMove.PiecePos, attackPos, card.AttackAhead);
        Locator<Attack>.Instance.
            PlayEffect(calcAttackPos, type, isMatch, isEnemy: true);
    }
    [PunRPC]
    private void Damage(int id, int damage, bool isMatch)
    {
        List<Vector2Int> attackPos;
        if (isMatch) attackPos = Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, Locator<CardGenerator>.Instance.CardBases[id].SAttackPos);
        else attackPos = Calculator.CalcDamagePos(currentBattler.BattlerMove.PiecePos, Locator<CardGenerator>.Instance.CardBases[id].AttackPos);

        waitBattler.Damage(attackPos, damage);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        SetPhase(Phase.Win);
        //photonView.RPC(nameof(SetPhase), RpcTarget.Others, Phase.Lose);
    }
    public void OnClickTitle()
    {
        SceneManager.LoadScene("MatchingScene");
        photonView.RPC(nameof(LoadScene), RpcTarget.Others, "MatchingScene");
    }
    [PunRPC]
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
