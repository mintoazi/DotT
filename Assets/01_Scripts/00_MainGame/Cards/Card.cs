using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

public class Card : MonoBehaviour
{
    //[SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text matchDescription;
    [SerializeField] private Text supDescription;
    [SerializeField] private Text cost;
    [SerializeField] private Text demoCost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardSprite = null;
    [SerializeField] private Vector3 selectedScale;
    [SerializeField] private Vector3 handScale;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject hidePanel;
    [SerializeField] private CardMovement cardMovement;
    public CardBase Base { get; private set; }
    // アクションを登録
    //public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase, bool isEnemy)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        matchDescription.text = "<color=#ff00ff>現在の属性と同じ属性だったら</color>\n" + cardBase.SDescription;
        supDescription.text = "<color=#7cfc00>サポート使用時</color>\n" + cardBase.SupDescription;
        cost.text = (cardBase.Cost + 1).ToString();
        demoCost.text = (cardBase.Cost + 1).ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];

        // 自分のカードはボタンを表示、相手のカードは裏面表示
        //buttonPanel.SetActive(!isEnemy);
        cardMovement.IsEnemy = isEnemy;
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

    public async UniTask OpenCard()
    {
        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.Euler(0f, 90f, 0f);
        float speed = 4f;
        float t = 0f;
        Debug.Log("カードオープン");
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.rotation = Quaternion.Lerp(from, to, t);
            await UniTask.DelayFrame(1);
        }
        hidePanel.SetActive(false);
        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * speed;
            transform.rotation = Quaternion.Lerp(from, to, t);
            await UniTask.DelayFrame(1);
        }
    }

    public void OnClick()
    {
        Debug.Log("カードが選択されました。");
        //OnClickCard?.Invoke(this);
    }
}
