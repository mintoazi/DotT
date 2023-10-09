using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlerHand : MonoBehaviour
{
    List<Card> hands = new List<Card>();

    public List<Card> Hands { get => hands; }

    // hands‚É’Ç‰Á‚µ‚ÄŽ©•ª‚ÌŽq—v‘f‚É‚·‚é
    public void Add(Card card)
    {
        Hands.Add(card);
        card.transform.SetParent(transform);
    }
    public void Remove(Card card)
    {
        Hands.Remove(card);
    }
    public void ResetPositions()
    {
        Hands.Sort((card0, card1) => card0.Base.Id - card1.Base.Id);
        for (int i = 0; i < Hands.Count; i++) 
        {
            hands[i].SetLayer(i);
            float posX = (i - Hands.Count / 2f) * 1.3f;
            Hands[i].transform.localPosition = new Vector3(posX, 0);
        }
    }
}
