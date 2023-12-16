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

    // �J�[�h��I��
    public void SelectedCard(Card card)
    {
        if (IsWait) return;
        if(card != null) Locator<GameMaster>.Instance.ActiveAttackTiles(card);
        SelectCard.Set(card);
    }

    // �J�[�h�I�𒆏���
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

        await SelectingCard(isPlayCardPhase: false);
        RecentCard = SelectCard.SelectedCard;
        Hand.Remove(SelectCard.SelectedCard);
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
    private void CheckType(int type)
    {
        CanUseSupport = model.CharaType.Value == type;
        IsMatchCharaType = CanUseSupport;
        // �^�C�v�����Ă鎞�I�[���I�Ȃ̂܂Ƃ킹����
    }
    /// <summary>
    /// �J�[�h���g�p������
    /// </summary>
    public async UniTask PlayCard()
    {
        //turnType = Model.CurrentType.Value;

        await SelectingCard(isPlayCardPhase: true);
        IsSubmit = true;
        Locator<GenerateGame>.Instance.DeactiveAttackRangeTiles();

        AttackCard = SelectCard.SelectedCard; // �g�p�����J�[�h����
        CheckType((int)AttackCard.Base.Type); // �T�|�[�g�J�[�h���g���邩�ǂ���
        RemoveCard(SelectCard.SelectedCard);
    }

    // PlayCard�Ɠ������������炱�������Ƃ�������
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
    /// �ړ�
    /// </summary>
    /// <returns>�ړ���</returns>
    public async UniTask<Vector2Int> Move()
    {
        IsSubmit = false;
        var moved = await BattlerMove.Move();
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
    /// �I���J�[�h�̈ړ�
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
