using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardInfo
{
    public int CardId; 
    public string Name;
    public string Description;
    public int Cost;
    public int EType;

    public CardInfo(int id, string name, string desc, int cost, int eType)
    {
        this.CardId = id;
        this.Name = name;
        this.Description = desc;
        this.Cost = cost;
        this.EType = eType;
    }
}

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
    //[SerializeField] private CardBase[] cardBases;

    [SerializeField] private List<string[]> cardInfoData = new List<string[]>();
    [SerializeField] private TextAsset cardInfoCSV = null;
    [SerializeField] private List<CardBase> cardBases = new List<CardBase>();
    
    private enum Num
    {
        Id, Name, Description, Cost, EType
    }
    private void Awake()
    {
        //ロケート
        Locator<CardGenerator>.Bind(this);

        // カードの読み込み
        cardInfoData = CSVLoader.Load(cardInfoCSV);
        for(int i = 0; i < cardInfoData.Count; i++)
        {
            cardBases.Add(
                new CardBase(
                    int.Parse(cardInfoData[i][(int)Num.Id]),
                    cardInfoData[i][(int)Num.Name],
                    cardInfoData[i][(int)Num.Description],
                    int.Parse(cardInfoData[i][(int)Num.Cost]),
                    ReturnEType(cardInfoData[i][(int)Num.EType])
                    )
                );
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

    public Card Draw()
    {
        return DrawMethod(cardBases.Count);
    }

    public Card ReDraw(List<int> cost)
    {
        List<CardBase> cbs = new List<CardBase>();

        int rand = UnityEngine.Random.Range(0, cost.Count);
        rand = cost[rand];

        foreach (CardBase cb in cardBases)
        {
            if(rand == cb.Cost) cbs.Add(cb);
        }
        return DrawMethod(cbs.Count);
    }

    public Card DrawMethod(int cards)
    {
        int rand = UnityEngine.Random.Range(0, cards);
        Card card = Instantiate(cardPrefab);
        card.Set(cardBases[rand]);
        return card;
    }
}
