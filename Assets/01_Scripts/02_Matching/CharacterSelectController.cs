using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityEngine.Windows;

public class CharacterSelectController : MonoBehaviour
{
    private enum Type
    {
        Curse,
        Tech,
        Magic
    }

    [SerializeField]
    Image playerImage;
    [SerializeField]
    Image playerDescription;

    [SerializeField]
    GameObject enemyInfoObject;
    [SerializeField]
    GameObject enemyWaitObject;
    [SerializeField]
    Image enemyImage;
    [SerializeField]
    Image enemyDescription;

    [SerializeField]
    Sprite[] characters;
    [SerializeField]
    Sprite[] descriptions;

    [SerializeField] UIOutline[] uiOutlines;
    [SerializeField] private float selectedWidth;
    [SerializeField] private float defaultWidth;
    private const float selectedDuration = 0.3f;

    private void OnEnable() => Locator<CharacterSelectController>.Bind(this);
    private void OnDisable() => Locator<CharacterSelectController>.Unbind(this);

    // 呪術キャラ選択
    public void OnSelectCurse()
    {
        OnSelect(Type.Curse);
    }
    // 技術キャラ選択
    public void OnSelectTech()
    {
        OnSelect(Type.Tech);
    }
    // 魔法キャラ選択
    public void OnSelectMagic()
    {
        OnSelect(Type.Magic);
    }

    public void OnReady()
    {
        OnlineMenuManager.onlineManager.OnStartButton();
    }

    public void OnDisconnect()
    {
        OnlineMenuManager.onlineManager.OnClickLeftRoom();
        Locator<MatchingManager>.Instance.SetState(MatchingManager.State.Room);
    }

    public void EnemyHasJoined()
    {
        enemyWaitObject.SetActive(false);
        enemyInfoObject.SetActive(true);
    }

    public void EnemyHasLeft()
    {
        enemyInfoObject.SetActive(false);
        enemyWaitObject.SetActive(true);
    }

    public void ChangeEnemyChara(int type)
    {
        enemyImage.sprite = characters[type];
        enemyDescription.sprite = descriptions[type];
    }

    private void ChangePlayerChara(int type)
    {
        playerImage.sprite = characters[type];
        playerDescription.sprite = descriptions[type];
    }

    private void OnSelect(Type type)
    {
        int iType = (int)type;
        ChangePlayerChara(iType);
        for (int i = 0; i < uiOutlines.Length; i++)
        {
            if(i == iType) uiOutlines[i].ResizeOutline(selectedWidth, selectedDuration).Forget();
            else uiOutlines[i].ResizeOutline(defaultWidth, selectedDuration).Forget();
        }
        OnlineMenuManager.onlineManager.OnClickType(iType);
    }
}
