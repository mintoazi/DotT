using UnityEngine;
using UnityEngine.UI;
public class CardView : MonoBehaviour
{
    [SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text supDescription;
    [SerializeField] private Text cost;
    //[SerializeField] private Text demoCost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardFrameSprite = null;
    [SerializeField] private Image cardImage;
    [SerializeField] private Sprite[] cardSprite;

    private void Awake() => Locator<CardView>.Bind(this);
    private void OnDestroy() => Locator<CardView>.Unbind(this);
    public void View() => parentObject.SetActive(true);
    public void UnView() => parentObject.SetActive(false);
    public void SetCard(Card card)
    {
        cardName.text = card.Base.Name;
        description.text = card.Base.Description;
        supDescription.text = card.Base.SupDescription;
        cost.text = card.Base.Cost.ToString();
        cardFrame.sprite = cardFrameSprite[(int)card.Base.Type];
        cardImage.sprite = cardSprite[card.Base.Id];
    }
}
