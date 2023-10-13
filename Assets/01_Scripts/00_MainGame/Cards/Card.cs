using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Card : MonoBehaviour
{
    //[SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text cost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardSprite = null;
    [SerializeField] private Vector3 selectedScale;
    [SerializeField] private Vector3 handScale;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject hidePanel;
    public CardBase Base { get; private set; }
    // アクションを登録
    public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase, bool isEnemy)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        cost.text = cardBase.Cost.ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];

        // 自分のカードはボタンを表示、相手のカードは裏面表示
        buttonPanel.SetActive(!isEnemy);
        hidePanel.SetActive(isEnemy);
    }

    public void SetPosition(Vector3 pos)
    {
        this.transform.localPosition = pos;
    }

    public void SetLayer(int i)
    {
        transform.SetSiblingIndex(i);
        //canvas.sortingOrder = i;
    }

    public void SetScale(bool isSelect)
    {
        if (isSelect) transform.localScale = selectedScale;
        else transform.localScale = handScale;
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

    -カードと盤面のレイヤー問題

    属性変更の視覚化
    キャラ表示の視覚化
    コスト消費の視覚化

    攻撃実装
    カード効果実装
    
    全PhaseにDelay追加

    キャラ選択実装

    Photon対応

    同期する情報
    ・
    ・攻撃エフェクト
    ・コマ
    ・手札の枚数
    ・使用したカード
    ・キャラUI
    ・HP
    ・属性
    ・コストUI

    同期しないオブジェクト
    ・タイル
    ・UI(手札、選択するUI、)
    ・カメラ
    ・選択可能タイル

    各ゲーム画面に敵の動きの情報だけを提出してRPCで相手のスクリプトを使うイメージ

    */
}
