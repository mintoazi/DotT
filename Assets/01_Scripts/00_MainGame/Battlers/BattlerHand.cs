using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlerHand : MonoBehaviour
{
    List<Card> hands = new List<Card>();

    public List<Card> Hands { get => hands; }
    private float cardSpace = 150f;

    // handsに追加して自分の子要素にする
    public void Add(Card card)
    {
        Hands.Add(card);
        card.transform.SetParent(transform);
        ResetPositions();
    }
    // カード手札から除外する
    public void Remove(Card card)
    {
        Hands.Remove(card);
        ResetPositions();
    }
    public void Remove()
    {
        Card c = Hands[0];
        Hands.RemoveAt(0);
        Destroy(c.gameObject);
        ResetPositions();
    }

    // カードを並び替える
    public void ResetPositions()
    {
        //Hands.Sort((card0, card1) => card0.Base.Id - card1.Base.Id);
        for (int i = 0; i < Hands.Count; i++) 
        {
            hands[i].SetLayer(i);
            hands[i].SetScale(false);
            float posX = (i - (Hands.Count-1) / 2f) * cardSpace;
            Hands[i].transform.localPosition = new Vector3(posX, 0);
        }
    }
}
