using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Battler : MonoBehaviour
{
    [SerializeField] BattlerHand hand;
    [SerializeField] BattlerMove battlerMove;
    [SerializeField] BattlerModel model;
    [SerializeField] SelectCard selectCard;

    public Card RecentCard { get; private set; }
    public Card AttackCard { get; private set; }
    public Card SupportCard { get; private set; }

    // Panel
    [SerializeField] GameObject playCardPanel;
    [SerializeField] GameObject playSupportCardPanel;

    // Buttons
    [SerializeField] Button submitButton;

    public bool IsTurn { get; set; }
    public bool IsSubmit { get; private set; }
    public bool IsWait { get; set; }
    public bool CanUseSupport { get; private set; }
    public bool IsMatchCharaType { get; private set; }
    public bool OldIsMatchCharaType { get; private set; }
    public BattlerHand Hand { get => hand; }
    public BattlerMove BattlerMove { get => battlerMove; }
    public BattlerModel Model { get => model; }
    public SelectCard SelectCard { get => selectCard; }

    // カードが配られる
    public void SetCardToHand(Card card)
    {
        hand.Add(card);
        //card.OnClickCard = SelectedCard;
    }
    // カードを引く
    public void Draw(Card card)
    {
        SetCardToHand(card);
    }

    // カードを選ぶ
    public void SelectedCard(Card card)
    {
        if (IsWait) return;
        if(card != null) Locator<GameMaster>.Instance.ActiveAttackTiles(card);
        SelectCard.Set(card);
    }

    // カード選択中処理
    private async UniTask SelectingCard(bool isPlayCardPhase)
    {
        SetActivePanel(target: playCardPanel, isActive: true);
        if(isPlayCardPhase) Hand.SetSelectable(Model.GetUsedCosts());
        else Hand.SetSelectable(true);
        while (true)
        {
            var buttonEvent = submitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
            await buttonEvent.OnInvokeAsync();
            if (selectCard.SelectedCard) break;
            else continue;
        }
        Hand.SetSelectable(false);
        SetActivePanel(target: playCardPanel, isActive: false);
    }

    /// <summary>
    /// 使えるカードと交換する
    /// </summary>
    public async UniTask<List<int>> ReDraw()
    {
        // 使えるカードがあるかチェックする
        bool isReDraw = CheckCanUseCard();
        if (isReDraw) return null;

        await SelectingCard(isPlayCardPhase: false);
        RecentCard = SelectCard.SelectedCard;
        Hand.Remove(SelectCard.SelectedCard);
        SelectCard.DeleteCard(); // 選択したカードの削除

        return Model.ReturnNotUseCost(); // 使っていないコストを返す

        bool CheckCanUseCard()
        {
            List<Card> cards = hand.Hands;
            
            foreach (Card c in cards)
            {
                //Debug.Log(c.Base.Cost);
                if (!Model.IsCostUses[c.Base.Cost].Value)
                {
                    Debug.Log("使用できるコスト" + c.Base.Cost);
                    return true; // 使えるカードが見つかったら処理を抜ける
                }
            }
            return false;
        }
    }
    private void CheckType(int type)
    {
        CanUseSupport = model.CharaType.Value == type;
        IsMatchCharaType = CanUseSupport;
        // タイプあってる時オーラ的なのまとわせたい
    }
    /// <summary>
    /// カードを使用させる
    /// </summary>
    public async UniTask PlayCard()
    {
        //turnType = Model.CurrentType.Value;

        await SelectingCard(isPlayCardPhase: true);
        IsSubmit = true;
        Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();

        AttackCard = SelectCard.SelectedCard; // 使用したカードを代入
        CheckType((int)AttackCard.Base.Type); // サポートカードを使えるかどうか
        RemoveCard(SelectCard.SelectedCard);
    }

    // PlayCardと同じ処理だからここを何とかしたい
    public async UniTask PlaySupport()
    {
        SetActivePanel(target: playSupportCardPanel, isActive: true);
        Hand.SetSelectable(true);

        // Reset
        SupportCard = null;
        SelectCard.Set(null);

        while (SelectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }
        
        SupportCard = SelectCard.SelectedCard;

        CheckType((int)SupportCard.Base.Type);
        if (IsMatchCharaType) model.AddBuff();
        
        Model.UseSupportCard(SupportCard.Base.Cost); 
        RemoveCard(SupportCard);
        if (hand.Hands.Count == 0) CanUseSupport = false;
        Hand.SetSelectable(false);
        SetActivePanel(target: playSupportCardPanel, isActive: false);
    }

    public void ResetAttackCard()
    {
        AttackCard.Delete();
        AttackCard = null;
    }
    public void ResetSupportCard()
    {
        SupportCard.Delete();
        SupportCard = null;
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <returns>移動先</returns>
    public async UniTask<Vector2Int> Move()
    {
        IsSubmit = false;
        var moved = await BattlerMove.Move();
        return moved;
    }

    public void Damage(List<Vector2Int> attackPos, int damage)
    {
        //Debug.Log("ダメージを受ける側の現在地" + battlerMove.PiecePos);

        for (int i = 0; i < attackPos.Count; i++)
        {
            //Debug.Log("ダメージを受ける地点k" + attackPos[i]);
        }
        attackPos = Calculator.CalcReflection(attackPos);
        
        for (int i = 0; i < attackPos.Count; i++)
        {
            //Debug.Log("ダメージを受ける地点" + attackPos[i]);
            if (battlerMove.PiecePos == attackPos[i])
            {
                Model.Damage(damage);
                //Debug.Log(damage + "ダメージ。");
            }
        }
    }

   public void Attack()
    {
        model.UseCost(AttackCard.Base.Cost);
        battlerMove.UpdatePieceType((int)AttackCard.Base.Type);
        //Model.ChangeType((int)AttackCard.Base.Type);
    }

    private void RemoveCard(Card card)
    {
        hand.Remove(card);
    }

    public void RemoveCard(int id)
    {
        hand.Remove(id);
    }

    private void SetActivePanel(GameObject target, bool isActive)
    {
        target.SetActive(isActive);
        //IsSubmit = !isActive;
    }

    public void PlayCard(int id)
    {
        Card card = hand.Remove(id);
        SelectedPosition(card, attackPosition.position).Forget();
        SelectCard.Set(card);
        AttackCard = SelectCard.SelectedCard;
        CheckType((int)AttackCard.Base.Type);
    }
    public async UniTask PlaySupportCard(int id)
    {
        Card card = hand.Remove(id);
        SelectedPosition(card, supportPosition.position).Forget();
        SupportCard = card;
        await SupportCard.OpenCard();
        Model.UseSupportCard(SupportCard.Base.Cost);
        if (IsMatchCharaType) model.AddBuff();
    }

    [SerializeField] Transform attackPosition = null;
    [SerializeField] Transform supportPosition = null;
    /// <summary>
    /// 選択カードの移動
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private async UniTask SelectedPosition(Card card, Vector3 pos)
    {
        float moveTime = 0.2f;
        if (card == null) return;
        Vector3 size = new Vector3(1.5f, 1.5f, 1.5f);

        card.MoveCard(pos, moveTime).Forget();
        await card.ResizeCard(size, moveTime);
    }
}
