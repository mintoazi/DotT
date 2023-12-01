using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.UI;
using static GameMaster;
using UnityEngine.Timeline;

public class CutInController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private Text phaseNameText;

    private void Awake()
    {
        //director.gameObject.SetActive(false);
    }
    public async UniTask CutInPhase(Phase phase)
    {
        if (phase == Phase.Init ||
            phase == Phase.Wait ||
            phase == Phase.Win ||
            phase == Phase.Lose ||
            phase == Phase.Attack ||
            phase == Phase.End) return;

        phaseNameText.text = phase.ToString();
        director.Play();
        await UniTask.WaitWhile(() => director.state == PlayState.Playing);
        //director.gameObject.SetActive(false);
    }
}
