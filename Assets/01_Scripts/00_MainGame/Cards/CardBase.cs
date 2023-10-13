using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class CardBase
{
    // カードの基礎データ
    [SerializeField] int id;
    [SerializeField] string name;
    
    [SerializeField] string description;
    [SerializeField] string sDescripiton;
    [SerializeField] int cost;
    [SerializeField] CardType type;

    [SerializeField] int[] range;
    [SerializeField] int damage;
    [SerializeField] int[] sRange;
    [SerializeField] int sDamage;
    List<Vector2Int> attackPos;
    List<Vector2Int> sAttackPos;
    [SerializeField] Sprite icon;
    

    public string Name { get => name; }
    public CardType Type { get => type; }
    public int Id { get => id; }
    public string Description { get => description; }
    public Sprite Icon { get => icon; }
    public int Cost { get => cost; }
    public string SDescripiton { get => sDescripiton; }
    public int Damage{ get => damage; }
    public int SDamage { get => sDamage; }
    public List<Vector2Int> AttackPos { get => attackPos; }
    public List<Vector2Int> SAttackPos { get => sAttackPos; }
    public CardBase(
          int id, string name, string desc, string sDesc,int cost,
          List<Vector2Int> attackPos, int damage,
          List<Vector2Int> sAttackPos, int sDamage, 
          CardType type) 
    { 
        this.id = id;
        this.name = name;
        this.description = desc;
        this.sDescripiton = sDesc;
        this.cost = cost;
        this.type = type;
        this.attackPos = attackPos;
        this.sAttackPos = sAttackPos;
        this.damage = damage;
        this.sDamage = sDamage;
    }
}

public enum CardType
{
    Curse, 
    Tech, 
    Magic
}
