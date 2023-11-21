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

    // �J�[�h���z����
    public void SetCardToHand(Card card)
    {
        hand.Add(card);
        //card.OnClickCard = SelectedCard;
    }
    // �J�[�h������
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
    }

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
    /// �g����J�[�h�ƌ�������
    /// </summary>
    public async UniTask<List<int>> ReDraw()
    {
        // �g����J�[�h�����邩�`�F�b�N����
        bool isReDraw = CheckCanUseCard();
        if (isReDraw) return null;

        await SelectingCard(isPlayCardPhase:false);
        
        SelectCard.DeleteCard(); // �I�������J�[�h�̍폜
        return Model.ReturnNotUseCost(); // �g���Ă��Ȃ��R�X�g��Ԃ�

        bool CheckCanUseCard()
        {
            List<Card> cards = hand.Hands;
            
            foreach (Card c in cards)
            {
                //Debug.Log(c.Base.Cost);
                if (!Model.IsCostUses[c.Base.Cost].Value)
                {
                    Debug.Log("�g�p�ł���R�X�g" + c.Base.Cost);
                    return true; // �g����J�[�h�����������珈���𔲂���
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// �J�[�h���g�p������
    /// </summary>
    public async UniTask PlayCard()
    {
        turnType = Model.CurrentType.Value;

        await SelectingCard(isPlayCardPhase: true);

        Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();

        RecentCard = SelectCard.SelectedCard; // �g�p�����J�[�h����
        OldIsMatchCharaType = IsMatchCharaType;
        IsMatchCharaType = ((int)RecentCard.Base.Type == model.CharaType.Value); // �L�����^�C�v�ƃJ�[�h�^�C�v�������Ă邩�ǂ���
        CanUseSupport = ((int)RecentCard.Base.Type == turnType); // �T�|�[�g�J�[�h���g���邩�ǂ���
        //Debug.Log("�T�|�[�g�J�[�h�̗L��" + CanUseSupport);
        //Debug.Log("�����J�[�h�̗L��" + IsMatchCharaType);
        RemoveCard(SelectCard.SelectedCard);
    }

    // PlayCard�Ɠ������������炱�������Ƃ�������
    public async UniTask PlaySupport()
    {
        SetActivePanel(target: playSupportCardPanel, isActive: true);
        Hand.SetSelectable(true);
        while (SelectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }
        
        RecentCard = SelectCard.SelectedCard;
        CanUseSupport = ((int)RecentCard.Base.Type == turnType);
        
        if (IsMatchCharaType) model.AddBuff();
        
        Model.UseSupportCard(RecentCard.Base.Cost); 
        RemoveCard(SelectCard.SelectedCard);
        if (hand.Hands.Count == 0) CanUseSupport = false;

        Debug.Log("�T�|�[�g�J�[�h�̗L��" + CanUseSupport);
        Hand.SetSelectable(false);
        SetActivePanel(target: playSupportCardPanel, isActive: false);
    }

    /// <summary>
    /// �ړ�
    /// </summary>
    /// <returns>�ړ���</returns>
    public async UniTask<Vector2Int> Move()
    {
        var moved = await BattlerMove.Move();
        SelectCard.DeleteCard();
        return moved;
    }

    public void Damage(List<Vector2Int> attackPos, int damage)
    {
        //Debug.Log("�_���[�W���󂯂鑤�̌��ݒn" + battlerMove.PiecePos);

        for (int i = 0; i < attackPos.Count; i++)
        {
            //Debug.Log("�_���[�W���󂯂�n�_k" + attackPos[i]);
        }
        attackPos = Calculator.CalcReflection(attackPos);
        
        for (int i = 0; i < attackPos.Count; i++)
        {
            //Debug.Log("�_���[�W���󂯂�n�_" + attackPos[i]);
            if (battlerMove.PiecePos == attackPos[i])
            {
                Model.Damage(damage);
                //Debug.Log(damage + "�_���[�W�B");
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
    public async UniTask PlaySupportCard(int id)
    {
        Card card = hand.Remove(id);
        SelectedPosition(card).Forget();
        SelectCard.Set(card);
        RecentCard = SelectCard.SelectedCard;
        await RecentCard.OpenCard();
        Model.UseSupportCard(RecentCard.Base.Cost);
    }

    [SerializeField] GameObject selectedPosition = null;
    /// <summary>
    /// �I���J�[�h�̈ړ�
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
