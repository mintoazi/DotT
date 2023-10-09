using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using TMPro.EditorUtilities;
using System.Threading;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Battler : MonoBehaviour
{
    [SerializeField] BattlerHand hand;
    [SerializeField] BattlerMove battlerMove;
    [SerializeField] SelectCard selectCard;
    [SerializeField] bool[] isCostUse = new bool[8];

    // Panels
    [SerializeField] GameObject reDrawPanel;
    [SerializeField] GameObject changeTypePanel;
    [SerializeField] GameObject playCardPanel;

    // CardPosition
    [SerializeField] Transform reDrawPos;
    [SerializeField] Transform changeTypePos;
    [SerializeField] Transform playCardPos;
    
    // Buttons
    [SerializeField] Button reDrawSubmitButton;
    [SerializeField] Button changeTypeSubmitButton;
    [SerializeField] Button playCardSubmitButton;

    private CardType currentType = CardType.Curse;
    public bool IsTurn { get; set; }
    public bool IsSubmit { get; private set; }
    //public UnityAction OnSubmitAction;
    public BattlerHand Hand { get => hand; }

    public void SetCardToHand(Card card)
    {
        hand.Add(card);

        card.OnClickCard = SelectedCard;

        hand.ResetPositions();
    } 
    
    private void SelectedCard(Card card)
    {
        if (IsSubmit) return;
        // すでにセットしていればセットしていたカードを手札に戻す
        if(selectCard.SelectedCard) 
        {
            hand.Add(selectCard.SelectedCard);
        }
        hand.Remove(card);
        selectCard.Set(card);
        hand.ResetPositions();
    }

    // カードを引く
    public void Draw()
    {
        Card card = Locator<CardGenerator>.Instance.Draw();
        SetCardToHand(card);
        card = Locator<CardGenerator>.Instance.Draw();
        SetCardToHand(card);
    }

    /// <summary>
    /// 使えるカードと交換する
    /// </summary>
    public async UniTask ReDraw()
    {
        // 使えるカードがあるかチェックする
        List<Card> cards = hand.Hands;
        bool isReDraw = true;
        foreach (Card c in cards)
        {
            isReDraw = isCostUse[c.Base.Cost];
            if (!isReDraw) break; // 使えるカードが見つかったら処理を抜ける
        }
        if (!isReDraw) return;

        // 空いてるコストを記録する
        List<int> costs = new List<int>();
        for (int i = 0; i < isCostUse.Length; i++)
        {
            if (!isCostUse[i]) costs.Add(i);
        }

        // パネルの表示
        reDrawPanel.SetActive(true);
        IsSubmit = false;
        selectCard.SelectedPosition = reDrawPos.transform;

        while (selectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        var buttonEvent = reDrawSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
        await buttonEvent.OnInvokeAsync();

        Card card = Locator<CardGenerator>.Instance.ReDraw(costs);
        SetCardToHand(card);

        selectCard.DeleteCard();

        reDrawPanel.SetActive(false);
        IsSubmit = true;
    }

    /// <summary>
    /// 属性を変更する
    /// </summary>
    /// <returns></returns>
    public async UniTask ChangeType()
    {
        changeTypePanel.SetActive(true);
        IsSubmit = false;
        selectCard.SelectedPosition = changeTypePos.transform;

        while(selectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        var buttonEvent = changeTypeSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
        await buttonEvent.OnInvokeAsync();

        currentType = selectCard.SelectedCard.Base.Type;

        selectCard.DeleteCard();

        changeTypePanel.SetActive(false);
        IsSubmit = true;
    }

    /// <summary>
    /// カードを使用させる
    /// </summary>
    /// <returns></returns>
    public async UniTask PlayCard()
    {
        playCardPanel.SetActive(true);
        IsSubmit = false;
        selectCard.SelectedPosition = playCardPos.transform;

        while (selectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        while(true)
        {
            var buttonEvent = playCardSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
            await buttonEvent.OnInvokeAsync();
            if (!isCostUse[selectCard.SelectedCard.Base.Cost]) break;
        }
        
        currentType = selectCard.SelectedCard.Base.Type;
        isCostUse[selectCard.SelectedCard.Base.Cost] = true;


        selectCard.DeleteCard();

        playCardPanel.SetActive(false);
        IsSubmit = true;
    }

    public async UniTask Move()
    {
        await battlerMove.Move();
    }
}
