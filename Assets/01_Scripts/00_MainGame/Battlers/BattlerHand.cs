using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BattlerHand : MonoBehaviour
{
    List<Card> hands = new List<Card>();
    [SerializeField] private Transform deckTransform;

    public List<Card> Hands { get => hands; }
    private float cardSpace = 150f;

    // hands�ɒǉ����Ď����̎q�v�f�ɂ���
    public void Add(Card card)
    {
        card.transform.position = deckTransform.position;
        hands.Add(card);
        Debug.Log(gameObject.name + "��" + card.Base.Name + "���ǉ����ꂽ");
        card.transform.SetParent(transform);
    }
    // �J�[�h��D���珜�O����
    public void Remove(Card card)
    {
        //Debug.Log(gameObject.name + "��" + card.Base.Name + "����n�ɑ���ꂽ");
        hands.Remove(card);
    }
    public Card Remove(int id)
    {
        Card card = hands.Find(x => x.Base.Id == id);
        if (card == null) Debug.Log("aaa" + id);
        Remove(card);
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
        Debug.Log("�g�p�ł����D" + s);
    }

    // �J�[�h����ёւ���
    public async UniTask ResetPositions()
    {
        float moveTime = 0.4f;
        //Hands.Sort((card0, card1) => card0.Base.Id - card1.Base.Id);
        for (int i = 0; i < Hands.Count; i++) 
        {
            //if(this.gameObject.name == "Player") Debug.Log(hands[i].Base.Name);
            hands[i].SetLayer(i);
            hands[i].SetScale(false);
            float posX = (i - (Hands.Count-1) / 2f) * cardSpace;

            hands[i].ResizeCard(Vector3.one, moveTime).Forget();
            hands[i].MoveCardLocal(new Vector3(posX, 0), moveTime).Forget();
        }
        await UniTask.WaitForSeconds(moveTime);
    }
}
