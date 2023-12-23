using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLog : MonoBehaviour
{
    List<Card> usedCards = new List<Card>();
    List<UseCase> usedCase = new List<UseCase>();
    public enum UseCase
    {
        Attack,
        Support,
        Redraw
    }

    public void UsedCard(Card card, UseCase useCase)
    {
        usedCards.Add(card);
        usedCase.Add(useCase);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            for(int i = 0; i < usedCards.Count; i++)
            {
                Debug.Log(usedCards[i].Base.Name + ":" + usedCase[i].ToString());
            }
        }
    }
}
