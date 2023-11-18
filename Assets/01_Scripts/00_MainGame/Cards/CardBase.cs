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
    [SerializeField] string supDescription;
    [SerializeField] int cost;
    [SerializeField] CardTypeM type;

    [SerializeField] int[] range;
    [SerializeField] int damage;
    [SerializeField] int[] sRange;
    [SerializeField] int sDamage;
    [SerializeField] int attackAhead;
    List<Vector2Int> attackPos;
    List<Vector2Int> sAttackPos;
    [SerializeField] Sprite icon;
    

    public string Name { get => name; }
    public CardTypeM Type { get => type; }
    public int Id { get => id; }
    public string Description { get => description; }
    public Sprite Icon { get => icon; }
    public int Cost { get => cost; }
    public string SDescription { get => sDescripiton; }
    public string SupDescription { get => supDescription; }
    public int Damage{ get => damage; }
    public int SDamage { get => sDamage; }
    public int AttackAhead { get => attackAhead; }
    public List<Vector2Int> AttackPos { get => attackPos; }
    public List<Vector2Int> SAttackPos { get => sAttackPos; }
    public CardBase(
          int id, string name, string desc, string sDesc, string supDesc, int cost,
          int attackAhead,
          List<Vector2Int> attackPos, int damage,
          List<Vector2Int> sAttackPos, int sDamage, 
          CardTypeM type) 
    { 
        this.id = id;
        this.name = name;
        this.description = desc;
        this.sDescripiton = sDesc;
        this.supDescription = supDesc;
        this.cost = cost;
        this.attackAhead = attackAhead;
        this.type = type;
        this.attackPos = attackPos;
        this.sAttackPos = sAttackPos;
        this.damage = damage;
        this.sDamage = sDamage;
    }
}

public enum CardTypeM
{
    Curse, 
    Tech, 
    Magic
}
