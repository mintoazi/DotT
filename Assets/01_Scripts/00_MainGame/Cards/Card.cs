using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.ComponentModel;

public class Card : MonoBehaviour
{
    [SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text cost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardSprite = null;
    [SerializeField] private Canvas canvas = null;

    public CardBase Base { get; private set; }
    // アクションを登録
    public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        cost.text = cardBase.Cost.ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];
        
    }

    public void SetLayer(int i)
    {
        canvas.sortingOrder = i;
    }

    public void OnClick()
    {
        //Debug.Log("カードが選択されました。");
        OnClickCard?.Invoke(this);
    }

    /*
    -たまに変なところにカードが置かれるバグ修正
    -カードプレイ実装
    -移動実装

    カードと盤面のレイヤー問題

    攻撃実装
    カード効果実装
    
    全PhaseにDelay追加

    キャラ選択実装

    Photon対応
    */
}
