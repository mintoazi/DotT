using UnityEngine;
using UnityEngine.UI;

public class CostCardView : MonoBehaviour
{
    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text supDescription;
    [SerializeField] private Text cost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardFrameSprite = null;
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite[] cardSprite;
    public void SetCard(Card card)
    {
        cardName.text = card.Base.Name;
        description.text = card.Base.Description;
        supDescription.text = card.Base.SupDescription;
        cost.text = (card.Base.Cost+1).ToString();
        cardFrame.sprite = cardFrameSprite[(int)card.Base.Type];
        cardImage.sprite = cardSprite[card.Base.Id2];
    }
}
