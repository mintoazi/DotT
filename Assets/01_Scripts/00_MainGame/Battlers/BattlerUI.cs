using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BattlerUI : MonoBehaviour
{
    // 現在の属性表示
    [SerializeField] private Image typeImage;
    [SerializeField] private Sprite[] typeSprite;

    // 現在のコスト表示
    [SerializeField] private Image[] costsImage;
    [SerializeField] private Sprite[] costsType;
    [SerializeField] private Color usedColor;
 
    // キャラクターの表示
    [SerializeField] private Image battlerSkin;
    [SerializeField] private Sprite[] battlerSprite;

    [SerializeField] private Image fieldSkin;
    [SerializeField] private Sprite[] fieldSprite;

    // バフの表示
    [SerializeField] private Text[] buffText;
    private enum Buffs { Attack, Defence, Cost }

    [SerializeField] private Text hpText;

    public void UpdateCharaType(int charaType)
    {
        battlerSkin.sprite = battlerSprite[charaType];
        fieldSkin.sprite = fieldSprite[charaType];
        typeImage.sprite = typeSprite[charaType];
        for (int i = 0; i < costsImage.Length; i++)
        {
            costsImage[i].sprite = costsType[charaType];
        }
    }

    public void UseCost(int cost, bool isUse)
    {
        if (isUse) costsImage[cost].sprite = costsType[3];
        else return;
    }

    public void UpdateType(int type)
    {
        typeImage.sprite = typeSprite[type];
    }

    public void UpdateHP(int hp)
    {
        if (int.Parse(hpText.text) > hp) ShakeChara(0.3f, 20).Forget();
        hpText.text = hp.ToString();
    }

    public void UpdateAttackBuff(int value) => buffText[(int)Buffs.Attack].text = value.ToString();
    public void UpdateDefenceBuff(int value) => buffText[(int)Buffs.Defence].text = value.ToString();
    public void UpdateCostBuff(int value) => buffText[(int)Buffs.Cost].text = value.ToString();

    private async UniTask ShakeChara(float duration, float magnitude)
    {
        Vector3 oldPos = battlerSkin.rectTransform.localPosition;
        float time = duration;
        Vector3 shake = new Vector3();
        while (time > 0)
        {
            time -= Time.deltaTime;
            shake.x = Random.Range(-magnitude, magnitude);
            shake.y = Random.Range(-magnitude, magnitude);
            battlerSkin.rectTransform.localPosition = oldPos + shake;
            await UniTask.DelayFrame(1);
        }
        battlerSkin.rectTransform.localPosition = oldPos;
    }
}
