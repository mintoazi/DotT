using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : MonoBehaviour
{
    [SerializeField] private TextAsset[] cardInfos = null;
    private List<List<CardBase>> CardBases = new List<List<CardBase>>();
    private enum Num
    {
        Id, Name, Description, SDescription, SupportDescription, Cost, EType, AttackAhead,
        Damage = 8,
        AttackRange = 9
    }

    private void Awake()
    {
        for(int i = 0; i < cardInfos.Length; i++)
        {
            LoadCard(CSVLoader.Load(cardInfos[i], isFirstLine: false), i);
        }
    }

    private void LoadCard(List<string[]> cards, int index)
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
            CardBases[index].Add(
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
                     ReturnEType(cards[i][(int)Num.EType])
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
        CardTypeM ReturnEType(string type)
        {
            if (type == "Curse") return CardTypeM.Curse;
            else if (type == "Tech") return CardTypeM.Tech;
            else if (type == "Magic") return CardTypeM.Magic;
            else
            {
                return (CardTypeM)Random.Range(0, 3);
            }
            //else Debug.LogError("–¢’m‚ÌElementType‚Å‚·"); 
        }
    }
}
