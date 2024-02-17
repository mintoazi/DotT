using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using System.Runtime.CompilerServices;

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
    //[SerializeField] private Image battlerSkin;
    //[SerializeField] private Sprite[] battlerSprite;

    [SerializeField] private Image fieldSkin;
    [SerializeField] private Sprite[] fieldSprite;

    // バフの表示
    [SerializeField] private Text[] buffText;
    private enum Buffs { Attack, Defence, Cost }

    [SerializeField] private Text hpText;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private PlayableDirector[] playableDirectors;
    [SerializeField]
    private Text[] addValues;

    [SerializeField] private GameObject[] skeletons;
    [SerializeField] private string[] stateName;
    [SerializeField] private SkeletonGraphic skeletonAnimation;
    private Spine.AnimationState spineAnimationState;
    int myChara = 0;

    private enum UI_Index 
    {
        HP,
        Attack,
        Defence,
        Cost
    }

    private enum StateName
    {
        Idle,
        Win,
        Damage,
    }

    private void Init()
    {
        skeletons[myChara].SetActive(true);
        skeletonAnimation = skeletons[myChara].GetComponent<SkeletonGraphic>();
        spineAnimationState = skeletonAnimation.AnimationState;
    }
    public void UpdateCharaType(int charaType)
    {
        //battlerSkin.sprite = battlerSprite[charaType];
        fieldSkin.sprite = fieldSprite[charaType];
        typeImage.sprite = typeSprite[charaType];
        for (int i = 0; i < costsImage.Length; i++)
        {
            costsImage[i].sprite = costsType[charaType];
        }
        myChara = charaType;
        
        Init();
        PlayIdleAnimation(null);
    }

    public void UseCost(int cost, bool isUse)
    {
        if (isUse) costsImage[cost].sprite = costsType[3];
        else return;
    }

    public void UpdateHP(int current)
    {
        int old = int.Parse(hpText.text);
        if (old < current) HealEffect();
        // 数値変更エフェクト
        UpdateValueEffect(UI_Index.HP, old, current);
        hpText.text = current.ToString();
    }

    public void DamageEffect(bool isDamage)
    {
        Debug.Log("damage");
        if (isDamage)
        {
            Debug.Log("damage2");
            ShakeChara(0.3f, 20).Forget();
            PlayAnimation((int)StateName.Damage, isLoop: false);
        }
        else return;
    }

    public void UpdateAttackBuff(int value) 
    {
        // 数値変更エフェクト
        UpdateValueEffect(UI_Index.Attack, int.Parse(buffText[(int)Buffs.Attack].text), current: value);
        buffText[(int)Buffs.Attack].text = value.ToString();
    } 
    public void UpdateDefenceBuff(int value)
    {
        // 数値変更エフェクト
        UpdateValueEffect(UI_Index.Defence, int.Parse(buffText[(int)Buffs.Defence].text), current: value);
        buffText[(int)Buffs.Defence].text = value.ToString();
    }
    public void UpdateCostBuff(int value)
    {
        // 数値変更エフェクト
        UpdateValueEffect(UI_Index.Cost, int.Parse(buffText[(int)Buffs.Cost].text), current: value);
        buffText[(int)Buffs.Cost].text = value.ToString();
    }

    private void UpdateValueEffect(UI_Index index, int old, int current)
    {
        playableDirectors[(int)index].Stop();
        int diff =  current - old;
        string s;

        if (diff > 0)
        {
            s = "+" + diff.ToString();
        }
        else if(diff < 0)
        {
            s = "-" + Mathf.Abs(diff).ToString();
        }
        else
        {
            s = "0";
        }

        //Debug.Log("<color=red> UI </color>");
        addValues[(int)index].text = s;
        playableDirectors[(int)index].Play();
    }

    private async UniTask ShakeChara(float duration, float magnitude)
    {
        Vector3 oldPos = skeletonAnimation.rectTransform.localPosition;
        float time = duration;
        Vector3 shake = new Vector3();
        while (time > 0)
        {
            time -= Time.deltaTime;
            shake.x = Random.Range(-magnitude, magnitude);
            shake.y = Random.Range(-magnitude, magnitude);
            skeletonAnimation.rectTransform.localPosition = oldPos + shake;
            await UniTask.DelayFrame(1);
        }
        skeletonAnimation.rectTransform.localPosition = oldPos;
    }

    private void HealEffect()
    {
        particle.Play();
    }

    private void Win()
    {
        PlayAnimation((int)StateName.Win, false);
    }

    private void PlayAnimation(int stateIndex, bool isLoop)
    {
        string state = stateName[stateIndex];
        TrackEntry trackEntry = spineAnimationState.SetAnimation(0, state, isLoop);
        trackEntry.Complete += PlayIdleAnimation;
    }
    private void PlayIdleAnimation(TrackEntry trackEntry)
    {
        string state = stateName[(int)StateName.Idle];
        spineAnimationState.SetAnimation(0, state, true);
    }
}
