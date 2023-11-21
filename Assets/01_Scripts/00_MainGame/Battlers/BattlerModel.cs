using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UniRx;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BattlerModel : MonoBehaviour
{
    private const int COSTS = 7;
    //HP
    public IReadOnlyReactiveProperty<int> Health => _health;
    public readonly IntReactiveProperty _health = new IntReactiveProperty(20);

    //Cost
    public IReadOnlyReactiveProperty<bool>[] IsCostUses => _isCostUses;
    private readonly BoolReactiveProperty[] _isCostUses = new BoolReactiveProperty[COSTS];

    //現在の属性
    public IReadOnlyReactiveProperty<int> CurrentType => _currentType;
    public readonly IntReactiveProperty _currentType = new IntReactiveProperty(0);

    //キャラの種類
    public IReadOnlyReactiveProperty<int> CharaType => _charaType;
    private readonly IntReactiveProperty _charaType = new IntReactiveProperty(0);

    //攻撃バフの数
    public IReadOnlyReactiveProperty<int> AttackBuff => _attackBuff;
    private readonly IntReactiveProperty _attackBuff = new IntReactiveProperty(0);

    //防御バフの数
    public IReadOnlyReactiveProperty<int> DefenceBuff => _defenceBuff;
    private readonly IntReactiveProperty _defenceBuff = new IntReactiveProperty(0);
    
    //コストバフの数
    public IReadOnlyReactiveProperty<int> CostBuff => _costBuff;
    private readonly IntReactiveProperty _costBuff = new IntReactiveProperty(0);

    private enum SupportCost
    {
        DecreaceCostByOne = 0,
        IncreaceAttackByTwo = 1,
        DecreaceCostByTwo = 2,
        IncreaceDefenceByTwo = 3,
        DecreaceCostByThree = 4,
        HealByTwo = 5,
        Special = 6,
        Error = 7
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
    public List<int> ReturnNotUseCost()
    {
        List<int> costs = new List<int>();
        for (int i = 0; i < IsCostUses.Length; i++)
        {
            if (!IsCostUses[i].Value)
            {
                costs.Add(i);
            }
        }
        return costs;
    }
    public void ResetBuffs()
    {
        _costBuff.Value = 0;
        _attackBuff.Value = 0;
        _defenceBuff.Value = 0;
    }
    public void UseSupportCard(int cost)
    {
        //Debug.Log(cost + "コストのサポートを使用した！");
        switch (cost)
        {
            case (int)SupportCost.DecreaceCostByOne:
                _costBuff.Value += 1;
                break;
            case (int)SupportCost.IncreaceAttackByTwo:
                _attackBuff.Value += 2;
                break;
            case (int)SupportCost.DecreaceCostByTwo:
                _costBuff.Value += 2;
                break;
            case (int)SupportCost.IncreaceDefenceByTwo:
                _defenceBuff.Value += 2;
                break;
            case (int)SupportCost.DecreaceCostByThree:
                _costBuff.Value += 3;
                break;
            case (int)SupportCost.HealByTwo:
                Heal(2);
                break;
            case (int)SupportCost.Special:
                
                break;
        }
    }

    public void AddBuff()
    {
        if (CharaType.Value == (int)CardTypeM.Tech) _defenceBuff.Value++;
        else if (CharaType.Value == (int)CardTypeM.Magic) _attackBuff.Value++;
    }

    public void Heal(int value)
    {
        int old = _health.Value;

        if (_health.Value + value >= 20) _health.Value = 20; // 上限超えないように
        else _health.Value += value;

        DebugDisplayHP(old, _health.Value);
    }

    public void SetHp(int value)
    {
        DebugDisplayHP(_health.Value, value);
        _health.Value = value;
    }

    public void Damage(int damage)
    {
        int old = _health.Value;
        _health.Value -= damage;

        DebugDisplayHP(old, _health.Value);
    }
    public void DebugDisplayHP(int old, int current)
    {
        Debug.Log(this.gameObject.name + "：" + old + "=>" + current);
    }
    public void ChangeType(int type)
    {
        int old = _currentType.Value;
        _currentType.Value = type;
        Debug.Log(this.gameObject.name + "：" + (CardTypeM)old + "=>" + (CardTypeM)type);
    }
    public void UseCost(int cost)
    {
        _isCostUses[cost].Value = true;
    }
    public List<bool> GetUsedCosts()
    {
        List<bool> usedCosts = new List<bool>();
        for(int i = 0; i < COSTS; i++)
        {
            usedCosts.Add(IsCostUses[i].Value);
        }
        return usedCosts;
    }

    private void OnDestroy()
    {
        _health.Dispose();
        _currentType.Dispose();
        _charaType.Dispose();
        _attackBuff.Dispose();
        _defenceBuff.Dispose();
        _costBuff.Dispose();
        for (int i = 0; i < _isCostUses.Length; i++)
        {
            _isCostUses[i].Dispose();
        }
    }
}