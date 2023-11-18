using Cysharp.Threading.Tasks;
using UnityEngine;

public class PhaseUI : MonoBehaviour
{
    [SerializeField] private GameObject[] phasePanels;
    private enum PhasePanel
    {
        Draw,
        ReDraw,
        PlayCard,
        Check,
        Move,
        Attack,
        Support,
        None
    }
    
    public async UniTask Display(GameMaster.Phase phase)
    {
        if (phase == GameMaster.Phase.Init ||
            phase == GameMaster.Phase.Wait ||
            phase == GameMaster.Phase.Win ||
            phase == GameMaster.Phase.Lose ||
            phase == GameMaster.Phase.Attack ||
            phase == GameMaster.Phase.End ) return;
        PhasePanel p = PhasePanel.None;
        switch (phase)
        {
            case GameMaster.Phase.Draw:
                phasePanels[(int)PhasePanel.Draw].SetActive(true);
                p = PhasePanel.Draw;
                break;
            case GameMaster.Phase.ReDraw:
                phasePanels[(int)PhasePanel.ReDraw].SetActive(true);
                p = PhasePanel.ReDraw;
                break;
            case GameMaster.Phase.PlayAttackCard:
                phasePanels[(int)PhasePanel.PlayCard].SetActive(true);
                p = PhasePanel.PlayCard;
                break;
            case GameMaster.Phase.CheckCard:
                phasePanels[(int)PhasePanel.Check].SetActive(true);
                p = PhasePanel.Check;
                break;
            case GameMaster.Phase.Move:
                phasePanels[(int)PhasePanel.Move].SetActive(true);
                p = PhasePanel.Move;
                break;
            case GameMaster.Phase.Attack:
                phasePanels[(int)PhasePanel.Attack].SetActive(true);
                p = PhasePanel.Attack;
                break;
            case GameMaster.Phase.PlaySupportCard:
                phasePanels[(int)PhasePanel.Support].SetActive(true);
                p = PhasePanel.Support;
                break;
            default:
                
                break;
        }
        await UniTask.WaitForSeconds(1f);
        if (p == PhasePanel.None) return;
        phasePanels[(int)p].SetActive(false);
    }
}
