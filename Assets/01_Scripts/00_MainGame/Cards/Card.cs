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
    // �A�N�V������o�^
    public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase, bool isEnemy)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        cost.text = cardBase.Cost.ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];

        // �����̃J�[�h�̓{�^����\���A����̃J�[�h�͗��ʕ\��
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
    }

    public void SetScale(bool isSelect)
    {
        if (isSelect) transform.localScale = selectedScale;
        else transform.localScale = handScale;
    }

    public void OnClick()
    {
        Debug.Log("�J�[�h���I������܂����B");
        OnClickCard?.Invoke(this);
    }
}
