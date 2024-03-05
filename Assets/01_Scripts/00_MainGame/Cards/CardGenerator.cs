using System.Collections.Generic;
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
    public static bool IsLoadingCSV = false;

    [SerializeField] private Card cardPrefab;

    [SerializeField] private TextAsset cardInfoCSV = null;
    [SerializeField] private TextAsset[] decks = null;
    [SerializeField] private List<CardBase> cardBases = new List<CardBase>();

    public List<CardBase> CardBases { get => cardBases; }

    private enum Num
    {
        Id, Name, Description, SDescription, SupportDescription, Cost, EType, AttackAhead,
        Damage = 8,
        AttackRange = 9
    }
    private void Awake()
    {
        //ロケート
        Locator<CardGenerator>.Bind(this);
        
    }
    public void GenerateCard(int p, int e)
    {
        Debug.Log(p + ";" + e);
        IsLoadingCSV = true;

        // カードの読み込み
        List<string[]> cardInfoData = new List<string[]>();  
        cardInfoData = CSVLoader.Load(decks[p], isFirstLine: false);
        LoadCard(cardInfoData,p);
        cardInfoData = CSVLoader.Load(decks[e], isFirstLine: false);
        LoadCard(cardInfoData,e);

        IsLoadingCSV = false;
        
    }

    private void LoadCard(List<string[]> cards,int t)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int id = CardBases.Count;
            int id2 = int.Parse(cards[i][(int)Num.Id]);
            string name = cards[i][(int)Num.Name];
            string desc = cards[i][(int)Num.Description];
            string sDesc = cards[i][(int)Num.SDescription];
            string supDesc = cards[i][(int)Num.SupportDescription];
            int cost = int.Parse(cards[i][(int)Num.Cost]);
            int attackAhead = int.Parse(cards[i][(int)Num.AttackAhead]);
            int damage = int.Parse(cards[i][(int)Num.Damage]);
            int sDamage = int.Parse(cards[i][(int)Num.Damage]);
            int[] attackRange = new int[18];
            for (int j = 0; j < attackRange.Length; j++)
            {
                attackRange[j] = int.Parse(cards[i][(int)Num.AttackRange + j]);
            }
            //var type = ReturnEType(cards[i][(int)Num.EType]);
            CardBases.Add(
                new CardBase(
                     id: id,
                     id2: id2,
                     name: name,
                     desc: desc,
                     sDesc: sDesc,
                     supDesc: supDesc,
                     cost: cost,
                     attackAhead: attackAhead,
                     attackPos: ReturnAttackPos(attackRange),
                     damage: damage,
                     sAttackPos: ReturnAttackPos(attackRange),
                     sDamage: sDamage,
                     ReturnEType(t)
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
                attackPos.Add(new Vector2Int(i / tileLength, i % tileLength));
            }

            return attackPos;
        }
        CardTypeM ReturnEType(int t)
        {
            return (CardTypeM)t;
            //if (type == "Curse") return CardTypeM.Curse;
            //else if (type == "Tech") return CardTypeM.Tech;
            //else if (type == "Magic") return CardTypeM.Magic;
            //else
            //{
            //    return (CardTypeM)Random.Range(0, 3);
            //}
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
        int rand = Random.Range(0, cards);
        Card card = Instantiate(cardPrefab);
        Debug.Log(rand);
        card.Set(CardBases[rand], isEnemy);
        return card;
    }

    public Card DrawMethod(List<int> ids , bool isEnemy)
    {
        int rand = Random.Range(0, ids.Count);
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
