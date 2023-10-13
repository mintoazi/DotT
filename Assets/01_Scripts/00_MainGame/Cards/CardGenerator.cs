using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Element
{
    public enum Type
    {
        Curse, Tech, Magic
    }
}

public class CardGenerator : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;

    [SerializeField] private List<string[]> cardInfoData = new List<string[]>();
    [SerializeField] private TextAsset cardInfoCSV = null;
    [SerializeField] private List<CardBase> cardBases = new List<CardBase>();

    public List<CardBase> CardBases { get => cardBases; }

    private enum Num
    {
        Id, Name, Description, SDescription, Cost, EType, AttackPos, 
        AttackRange = 7,
        SAttackRange = 17,
        Damage = 16,
        SDamage = 26
    }
    private void Awake()
    {
        //ロケート
        Locator<CardGenerator>.Bind(this);

        // カードの読み込み
        cardInfoData = CSVLoader.Load(cardInfoCSV);
        for(int i = 0; i < cardInfoData.Count; i++)
        {
            int id = int.Parse(cardInfoData[i][(int)Num.Id]);
            string name = cardInfoData[i][(int)Num.Name];
            string desc = cardInfoData[i][(int)Num.Description];
            string sDesc = cardInfoData[i][(int)Num.SDescription];
            int cost = int.Parse(cardInfoData[i][(int)Num.Cost]);
            int damage = int.Parse(cardInfoData[i][(int)Num.Damage]);
            int sDamage = int.Parse(cardInfoData[i][(int)Num.SDamage]);
            int[] attackRange = new int[9];
            int[] sAttackRange = new int[9];
            for (int j = 0; j < attackRange.Length; j++)
            {
                attackRange[j] = int.Parse(cardInfoData[i][(int)Num.AttackRange + j]);
                sAttackRange[j] = int.Parse(cardInfoData[i][(int)Num.SAttackRange + j]);
            }
            CardBases.Add(
                new CardBase(
                      id: id, 
                      name: name, 
                      desc: desc, 
                      sDesc: sDesc, 
                      cost: cost, 
                      attackPos: ReturnAttackPos(attackRange),
                      damage: damage,
                      sAttackPos: ReturnAttackPos(sAttackRange),
                      sDamage:sDamage,
                      ReturnEType(cardInfoData[i][(int)Num.EType])
                    )
                );
        }
        List<Vector2Int> ReturnAttackPos(int[] posData)
        {
            List<Vector2Int> attackPos = new List<Vector2Int>();
            int tileLength = 3;
            int none = 0;
            for (int i = 0; i < posData.Length; i++)
            {
                if (posData[i] == none) continue;
                attackPos.Add(new Vector2Int(i % tileLength, i / tileLength));
            }
            
            return attackPos;
        }
        CardType ReturnEType(string type)
        {
            if (type == "Curse") return CardType.Curse;
            else if (type == "Tech") return CardType.Tech;
            else return CardType.Magic;
            //else Debug.LogError("未知のElementTypeです"); 
        }
    }
    private void OnDisable()
    {
        Locator<CardGenerator>.Unbind(this);
    }

    public Card Draw(bool isEnemy)
    {
        return DrawMethod(CardBases.Count, isEnemy);
    }

    public Card ReDraw(List<int> cost, bool isEnemy)
    {
        List<CardBase> cbs = new List<CardBase>();
        Debug.Log(cost);
        int rand = UnityEngine.Random.Range(0, cost.Count);
        rand = cost[rand];

        foreach (CardBase cb in CardBases)
        {
            if(rand == cb.Cost) cbs.Add(cb);
        }
        return DrawMethod(cbs.Count, isEnemy);
    }

    public Card DrawMethod(int cards, bool isEnemy)
    {
        int rand = UnityEngine.Random.Range(0, cards);
        Card card = Instantiate(cardPrefab);
        card.Set(CardBases[rand], isEnemy);
        return card;
    }
    public Card ChoiceDraw(int id, bool isEnemy)
    {
        Card card = Instantiate(cardPrefab);
        card.Set(CardBases[id], isEnemy);
        return card;
    }
}
