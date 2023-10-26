using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class Element
{
    public enum Type
    {
        Curse, Tech, Magic
    }
}

public class CardGenerator : MonoBehaviour
{
    public static bool IsLoadingCSV = false;

    [SerializeField] private Card cardPrefab;

    [SerializeField] private List<string[]> cardInfoData = new List<string[]>();
    [SerializeField] private TextAsset cardInfoCSV = null;
    [SerializeField] private List<CardBase> cardBases = new List<CardBase>();

    public List<CardBase> CardBases { get => cardBases; }

    private enum Num
    {
        Id, Name, Description, SDescription, Cost, EType, AttackAhead,
        AttackRange = 7,
        SAttackRange = 17,
        Damage = 16,
        SDamage = 26
    }
    private void Awake()
    {
        //ロケート
        Locator<CardGenerator>.Bind(this);
        IsLoadingCSV = true;
        // カードの読み込み
        cardInfoData = CSVLoader.Load(cardInfoCSV, isFirstLine: false);
        for (int i = 0; i < cardInfoData.Count; i++)
        {
            int id = int.Parse(cardInfoData[i][(int)Num.Id]);
            string name = cardInfoData[i][(int)Num.Name];
            string desc = cardInfoData[i][(int)Num.Description];
            string sDesc = cardInfoData[i][(int)Num.SDescription];
            int cost = int.Parse(cardInfoData[i][(int)Num.Cost]);
            int attackAhead = int.Parse(cardInfoData[i][(int)Num.AttackAhead]);
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
                      attackAhead: attackAhead,
                      attackPos: ReturnAttackPos(attackRange),
                      damage: damage,
                      sAttackPos: ReturnAttackPos(sAttackRange),
                      sDamage: sDamage,
                      ReturnEType(cardInfoData[i][(int)Num.EType])
                    )
                );
        }
        IsLoadingCSV = false;
        List<Vector2Int> ReturnAttackPos(int[] posData)
        {
            List<Vector2Int> attackPos = new List<Vector2Int>();
            int tileLength = 3;
            int none = 0;
            for (int i = 0; i < posData.Length; i++)
            {
                if (posData[i] == none) continue;
                attackPos.Add(new Vector2Int(i / tileLength, i % tileLength));
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
        List<int> ids = new List<int>();
        foreach (CardBase cb in CardBases)
        {
            if (cost.Contains(cb.Cost)) ids.Add(cb.Id);
        }
        return DrawMethod(ids, isEnemy);
    }

    public Card DrawMethod(int cards, bool isEnemy)
    {
        int rand = UnityEngine.Random.Range(0, cards);
        Card card = Instantiate(cardPrefab);

        card.Set(CardBases[rand], isEnemy);
        return card;
    }

    public Card DrawMethod(List<int> ids , bool isEnemy)
    {
        int rand = UnityEngine.Random.Range(0, ids.Count);
        Card card = Instantiate(cardPrefab);
        card.Set(CardBases[ids[rand]], isEnemy);
        return card;
    }

    public Card ChoiceDraw(int id, bool isEnemy)
    {
        Card card = Instantiate(cardPrefab);
        card.Set(CardBases[id], isEnemy);
        return card;
    }
}
