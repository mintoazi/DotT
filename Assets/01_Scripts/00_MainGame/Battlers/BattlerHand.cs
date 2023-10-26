using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlerHand : MonoBehaviour
{
    List<Card> hands = new List<Card>();

    public List<Card> Hands { get => hands; }
    private float cardSpace = 150f;

    // hands�ɒǉ����Ď����̎q�v�f�ɂ���
    public void Add(Card card)
    {
        Hands.Add(card);
        card.transform.SetParent(transform);
        ResetPositions();
    }
    // �J�[�h��D���珜�O����
    public void Remove(Card card)
    {
        Hands.Remove(card);
        ResetPositions();
    }
    public Card Remove(int id)
    {
        Card card = hands.Find(x => x.Base.Id == id);
        Remove(card);
        ResetPositions();
        return card;
    }

    // �J�[�h����ёւ���
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
