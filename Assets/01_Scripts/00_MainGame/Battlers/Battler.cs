using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using UniRx;
using UnityEngine.Events;
using Unity.VisualScripting;

public class Battler : MonoBehaviour
{
    private const int COSTS = 7;
    [SerializeField] BattlerHand hand;
    [SerializeField] BattlerMove battlerMove;
    [SerializeField] SelectCard selectCard;

    public Card RecentCard { get; private set; }

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

    public bool IsTurn { get; set; }
    public bool IsSubmit { get; private set; }
    public bool IsWait { get; set; }
    public BattlerHand Hand { get => hand; }
    public BattlerMove BattlerMove { get => battlerMove; }
    // public UnityAction OnSubmitAction;

    //HP
    public IReadOnlyReactiveProperty<int> Health => _health;
    private readonly IntReactiveProperty _health = new IntReactiveProperty(20);

    //Cost
    public IReadOnlyReactiveProperty<bool>[] IsCostUses => _isCostUses;
    private readonly BoolReactiveProperty[] _isCostUses = new BoolReactiveProperty[COSTS];

    //���݂̑���
    public IReadOnlyReactiveProperty<int> CurrentType => _currentType;
    public readonly IntReactiveProperty _currentType = new IntReactiveProperty(0);

    //�L�����̎��
    public IReadOnlyReactiveProperty<int> CharaType => _charaType;
    private readonly IntReactiveProperty _charaType = new IntReactiveProperty(0);
    private void OnDestroy()
    {
        _health.Dispose();
        _currentType.Dispose();
        _charaType.Dispose();
        for (int i = 0; i < _isCostUses.Length; i++)
        {
            _isCostUses[i].Dispose();
        }
    }
    private void Awake()
    {
        for (int i = 0; i < COSTS; i++)
        {
            _isCostUses[i] = new BoolReactiveProperty();
            _isCostUses[i].Value = false;
        }
    }
    public void Init(int health, int type)
    {
        _health.Value = health;
        _currentType.Value = type;
        _charaType.Value = type;
    }

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

    private void SelectedCard(Card card)
    {
        if (IsSubmit || IsWait) return;
        // ���łɃZ�b�g���Ă���΃Z�b�g���Ă����J�[�h����D�ɖ߂�
        if(selectCard.SelectedCard) 
        {
            hand.Add(selectCard.SelectedCard);
        }
        RemoveCard(card);
        selectCard.Set(card);
    }

    /// <summary>
    /// �g����J�[�h�ƌ�������
    /// </summary>
    public async UniTask<List<int>> ReDraw()
    {
        // �g����J�[�h�����邩�`�F�b�N����
        bool isReDraw = CheckCanUseCard();
        if (isReDraw) return null;
        // �󂢂Ă�R�X�g�𒲂ׂ�
        List<int> costs = new List<int>();
        for (int i = 0; i < _isCostUses.Length; i++)
        {
            if (!_isCostUses[i].Value)
            {
                costs.Add(i);
                Debug.Log(i);
            }
        }

        // �p�l���̕\��
        SetActivePanel(target: reDrawPanel, isActive: true);
        selectCard.SelectedPosition = reDrawPos.transform;

        while (selectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        var buttonEvent = reDrawSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
        await buttonEvent.OnInvokeAsync();
        SetActivePanel(target: reDrawPanel, isActive: false);
        
        return costs;

        bool CheckCanUseCard()
        {
            List<Card> cards = hand.Hands;
            
            foreach (Card c in cards)
            {
                //Debug.Log(c.Base.Cost);
                if (!_isCostUses[c.Base.Cost].Value) return true; // �g����J�[�h�����������珈���𔲂���
            }
            return false;
        }
    }

    ///// <summary>
    ///// ������ύX����
    ///// </summary>
    //public async UniTask<int> ChangeType()
    //{
    //    SetActivePanel(target: changeTypePanel, isActive: true);
    //    selectCard.SelectedPosition = changeTypePos.transform;

    //    while(selectCard.SelectedCard == null)
    //    {
    //        await UniTask.DelayFrame(1);
    //    }

    //    var buttonEvent = changeTypeSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
    //    await buttonEvent.OnInvokeAsync();
    //    RecentCard = selectCard.SelectedCard;
    //    ChangeType((int)selectCard.SelectedCard.Base.Type);
    //    SetActivePanel(target: changeTypePanel, isActive: false);
    //    return (int)selectCard.SelectedCard.Base.Type;
    //}
    
    /// <summary>
    /// �J�[�h���g�p������
    /// </summary>
    public async UniTask PlayCard()
    {
        SetActivePanel(target:playCardPanel, isActive: true);
        selectCard.SelectedPosition = playCardPos.transform;

        while (selectCard.SelectedCard == null)
        {
            await UniTask.DelayFrame(1);
        }

        while(true)
        {
            var buttonEvent = playCardSubmitButton.onClick.GetAsyncEventHandler(CancellationToken.None);
            await buttonEvent.OnInvokeAsync();
            if (!_isCostUses[selectCard.SelectedCard.Base.Cost].Value) break;
        }

        // �����̏���
        ChangeType((int)selectCard.SelectedCard.Base.Type);
        _isCostUses[selectCard.SelectedCard.Base.Cost].Value = true;
        RecentCard = selectCard.SelectedCard;

        SetActivePanel(target: playCardPanel, isActive: false);
    }

    /// <summary>
    /// �ړ�
    /// </summary>
    /// <returns>�ړ���</returns>
    public async UniTask<Vector2Int> Move()
    {
        return await BattlerMove.Move();
    }

    public void Damage(List<Vector2Int> attackPos, int damage)
    {
        Debug.Log("�_���[�W���󂯂鑤�̌��ݒn" + battlerMove.PiecePos);

        for (int i = 0; i < attackPos.Count; i++)
        {
            Debug.Log("�_���[�W���󂯂�n�_k" + attackPos[i]);
        }
        attackPos = Calculator.CalcReflection(attackPos);
        
        for (int i = 0; i < attackPos.Count; i++)
        {
            Debug.Log("�_���[�W���󂯂�n�_" + attackPos[i]);
            if (battlerMove.PiecePos == attackPos[i])
            {
                _health.Value -= damage;
            }
        }
    }

   

    private void RemoveCard(Card card)
    {
        hand.Remove(card);
    }

    public void RemoveCard(int id)
    {
        hand.Remove();
    }

    private void SetActivePanel(GameObject target, bool isActive)
    {
        target.SetActive(isActive);
        IsSubmit = !isActive;
    }

    public void ChangeType(int type)
    {
        _currentType.Value = type;
    }
    public void PlayCard(int id)
    {
        hand.Remove();
        CardBase cardBase = Locator<CardGenerator>.Instance.CardBases[id];
        _currentType.Value = (int)cardBase.Type;
        _isCostUses[cardBase.Cost].Value = true;
    }
}
