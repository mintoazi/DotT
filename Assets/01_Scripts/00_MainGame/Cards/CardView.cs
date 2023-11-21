using UnityEngine;
using UnityEngine.UI;
public class CardView : MonoBehaviour
{
    //[SerializeField] private GameObject parentObject;

    //[SerializeField] private Text cardName;
    //[SerializeField] private Text description;
    //[SerializeField] private Text cost;
    //[SerializeField] private Image cardFrame;
    //[SerializeField] private Sprite[] cardSprite = null;

    private void Awake() => Locator<CardView>.Bind(this);
    private void OnDestroy() => Locator<CardView>.Unbind(this);
    //public void View() => parentObject.SetActive(true);
    //public void UnView() => parentObject.SetActive(false);
    //public void SetCard(CardInfo card)
    //{
    //    cardName.text = card.Name;
    //    description.text = card.Description;
    //    cost.text = card.Cost.ToString();
    //    cardFrame.sprite = cardSprite[card.EType];
    //}
}
