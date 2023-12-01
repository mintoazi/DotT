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

    // handsに追加して自分の子要素にする
    public void Add(Card card)
    {
        card.transform.position = deckTransform.position;
        hands.Add(card);
        Debug.Log(gameObject.name + "に" + card.Base.Name + "が追加された");
        card.transform.SetParent(transform);
    }
    // カード手札から除外する
    public void Remove(Card card)
    {
        //Debug.Log(gameObject.name + "の" + card.Base.Name + "が墓地に送られた");
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
        Debug.Log("使用できる手札" + s);
    }

    // カードを並び替える
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
