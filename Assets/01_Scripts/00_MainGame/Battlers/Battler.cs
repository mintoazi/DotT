using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;

public class Battler : MonoBehaviour
{
    [SerializeField] BattlerHand hand;
    [SerializeField] BattlerMove battlerMove;
    [SerializeField] BattlerModel model;
    [SerializeField] SelectCard selectCard;

    public Card RecentCard { get; private set; }

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

    int turnType = 0;
    // public UnityAction OnSubmitAction;

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
    public void Draw(int id)
    {
        SetCardToHand(Locator<CardGenerator>.Instance.ChoiceDraw(id, true));
    }

    public void SelectedCard(Card card)
    {
        if (IsSubmit || IsWait) return;
        if(card != null) Locator<GameMaster>.Instance.ActiveAttackTiles(card);
        SelectCard.Set(card);
        //Debug.Log(SelectCard.SelectedCard.name + "が選択された！");
    }

    /// <summary>
    /// 使えるカードと交換する
    /// </summary>
    public async UniTask<List<int>> ReDraw()
    {
        // 使えるカードがあるかチェックする
        bool isReDraw = CheckCanUseCard();
        if (isReDraw) return null;
        
        // パネルの表示
        SetActivePanel(target: playCardPanel, isActive: true);
        //selectCard.SelectedPosition = reDrawPos.transform;

        while (SelectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        var buttonEvent = submitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
        await buttonEvent.OnInvokeAsync();
        SetActivePanel(target: playCardPanel, isActive: false);
        SelectCard.DeleteCard();
        return Model.ReturnNotUseCost();

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
    
    /// <summary>
    /// カードを使用させる
    /// </summary>
    public async UniTask PlayCard()
    {
        SetActivePanel(target:playCardPanel, isActive: true);
        //selectCard.SelectedPosition = playCardPos.transform;
        turnType = Model.CurrentType.Value;
        while (true)
        {
            var buttonEvent = submitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
            await buttonEvent.OnInvokeAsync();
            bool isNull = selectCard.SelectedCard == null;
            if (isNull) continue;
            bool costCheck = !Model.IsCostUses[SelectCard.SelectedCard.Base.Cost].Value;
            if (costCheck) break;
        }
        Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();
        //model.UseCards++; // そのターンに使用したカードを追加
        RecentCard = SelectCard.SelectedCard; // 使用したカードを代入
        OldIsMatchCharaType = IsMatchCharaType;
        IsMatchCharaType = ((int)RecentCard.Base.Type == model.CharaType.Value); // キャラタイプとカードタイプが合ってるかどうか
        CanUseSupport = ((int)RecentCard.Base.Type == turnType); // サポートカードを使えるかどうか
        //Debug.Log("サポートカードの有効" + CanUseSupport);
        //Debug.Log("強化カードの有効" + IsMatchCharaType);
        RemoveCard(SelectCard.SelectedCard);

        SetActivePanel(target: playCardPanel, isActive: false);
    }

    // PlayCardと同じ処理だからここを何とかしたい
    public async UniTask PlaySupport()
    {
        SetActivePanel(target: playSupportCardPanel, isActive: true);

        while (SelectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        //while (true)
        //{
        //    var buttonEvent = submitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
        //    await buttonEvent.OnInvokeAsync();
        //    break;
        //}

        
        RecentCard = SelectCard.SelectedCard;
        CanUseSupport = ((int)RecentCard.Base.Type == turnType);
        if (hand.Hands.Count == 0) CanUseSupport = false;
        if (IsMatchCharaType) model.AddBuff();
        Debug.Log("サポートカードの有効" + CanUseSupport);
        Model.UseSupportCard(RecentCard.Base.Cost + 1); // ここを追加しただけ
        RemoveCard(SelectCard.SelectedCard);
        SetActivePanel(target: playSupportCardPanel, isActive: false);
    }

    /// <summary>
    /// 移動
    /// </summary>
    /// <returns>移動先</returns>
    public async UniTask<Vector2Int> Move()
    {
        var moved = await BattlerMove.Move();
        SelectCard.DeleteCard();
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
        model.UseCost(RecentCard.Base.Cost);
        battlerMove.UpdatePieceType((int)RecentCard.Base.Type);
        Model.ChangeType((int)RecentCard.Base.Type);
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
        IsSubmit = !isActive;
    }

    public void PlayCard(int id)
    {
        Card card = hand.Remove(id);
        SelectedPosition(card).Forget();
        SelectCard.Set(card);
        RecentCard = SelectCard.SelectedCard;
    }
    public void PlaySupportCard(int id)
    {
        Card card = hand.Remove(id);
        SelectedPosition(card).Forget();
        SelectCard.Set(card);
        RecentCard = SelectCard.SelectedCard;
        Model.UseSupportCard(RecentCard.Base.Cost + 1);
    }

    [SerializeField] GameObject selectedPosition = null;
    /// <summary>
    /// 選択カードの移動
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private async UniTask SelectedPosition(Card card)
    {
        float time = 0;
        float moveTime = 0.2f;
        //Debug.Log(card);
        //Debug.Log(card.transform.position);
        if (card == null) return;
        Vector3 startPos = card.transform.position;
        while(time < moveTime)
        {
            time += Time.deltaTime;
            if (time > moveTime) time = moveTime; 
            card.transform.position = Vector3.Lerp(startPos, selectedPosition.transform.position, time / moveTime);
            await UniTask.DelayFrame(1);
        }
    }
}
