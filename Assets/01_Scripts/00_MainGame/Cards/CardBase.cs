using Unity.VisualScripting;
using UnityEngine;

public class CardBase
{
    // カードの基礎データ
    [SerializeField] string name;
    [SerializeField] CardType type;
    [SerializeField] int id;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    [SerializeField] int cost;

    public string Name { get => name; }
    public CardType Type { get => type; }
    public int Id { get => id; }
    public string Description { get => description; }
    public Sprite Icon { get => icon; }
    public int Cost { get => cost; }

    public CardBase(int id, string name, string desc, int cost, CardType type) 
    { 
        this.id = id;
        this.name = name;
        this.description = desc;
        this.cost = cost;
        this.type = type;
    }
}

public enum CardType
{
    Curse, 
    Tech, 
    Magic
}
