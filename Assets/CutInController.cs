using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using UnityEngine.UI;
using static GameMaster;

public class CutInController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private Text phaseNameText;
    [SerializeField] private Text[] phaseFlowTexts;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color highlightColor;

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

        HighlightFlow(phase);
        phaseNameText.text = phase.ToString();
        director.Play();
        await UniTask.WaitWhile(() => director.state == PlayState.Playing);
        //director.gameObject.SetActive(false);
    }

    private enum Flow
    {
        Draw,
        ReDraw,
        PlayCard,
        Support,
        CheckCard,
        Move
    }

    private void HighlightFlow(Phase phase)
    {
        int i = 0;
        switch (phase)
        {
            case Phase.Draw:
                i = (int)Flow.Draw;
                break;
            case Phase.ReDraw:
                i = (int)Flow.ReDraw;
                break;
            case Phase.PlayCard:
                i = (int)Flow.PlayCard;
                break;
            case Phase.Support:
                i = (int)Flow.Support;
                break;
            case Phase.CheckCard:
                i = (int)Flow.CheckCard;
                break;
            case Phase.Move:
                i = (int)Flow.Move;
                break;
        }

        foreach (Text t in phaseFlowTexts)
        {
            t.color = defaultColor;
        }
        phaseFlowTexts[i].color = highlightColor;
    }
}
