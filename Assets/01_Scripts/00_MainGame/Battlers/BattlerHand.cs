using System.Collections.Generic;
using UnityEngine;

public class BattlerHand : MonoBehaviour
{
    List<Card> hands = new List<Card>();

    public List<Card> Hands { get => hands; }
    private float cardSpace = 150f;

    // handsに追加して自分の子要素にする
    public void Add(Card card)
    {
        Hands.Add(card);
        Debug.Log(card.Base.Name + "が追加された");
        card.transform.SetParent(transform);
        ResetPositions();
    }
    // カード手札から除外する
    public void Remove(Card card)
    {
        Debug.Log(card.Base.Name + "が墓地に送られた");
        Hands.Remove(card);
    }
    public Card Remove(int id)
    {
        Card card = hands.Find(x => x.Base.Id == id);
        if (card == null) Debug.Log("aaa" + id);
        Remove(card);
        ResetPositions();
        return card;
    }

    public void SetSelectable(bool canSelect)
    {
        foreach(Card c in Hands)
        {
            c.gameObject.GetComponent<CardMovement>().SetSelectable(canSelect);
        }
    }

    public void SetSelectable(List<bool> isCostUsed)
    {
        string s = "";
        foreach(Card c in Hands)
        {
            if (isCostUsed[c.Base.Cost]) continue;
            s += c.Base.Cost.ToString() + ":";
            c.gameObject.GetComponent<CardMovement>().SetSelectable(true);
        }
        Debug.Log("使用できる手札" + s);
    }

    // カードを並び替える
    public void ResetPositions()
    {
        //Hands.Sort((card0, card1) => card0.Base.Id - card1.Base.Id);
        for (int i = 0; i < Hands.Count; i++) 
        {
            //if(this.gameObject.name == "Player") Debug.Log(hands[i].Base.Name);
            hands[i].SetLayer(i);
            hands[i].SetScale(false);
            float posX = (i - (Hands.Count-1) / 2f) * cardSpace;
            Hands[i].transform.localPosition = new Vector3(posX, 0);
        }
    }
}
