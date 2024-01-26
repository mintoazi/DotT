using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class CostJudge : MonoBehaviour
{
    [SerializeField] private CostCardView pCard;
    [SerializeField] private CostCardView eCard;

    [SerializeField] private Text pCost;
    [SerializeField] private Text eCost;

    [SerializeField] private Text pBuff;
    [SerializeField] private Text eBuff;

    [SerializeField] private Text pTotalCost;
    [SerializeField] private Text eTotalCost;

    [SerializeField] private Text pTimer;
    [SerializeField] private Text eTimer;

    [SerializeField] private Text playerTurn;
    [SerializeField] private Text enemyTurn;

    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private TimelineAsset[] timelines;

    private const string FIRST = "æU";
    private const string SECOND = "ŒãU";

    private bool isEnd = false;
    private void EndTimeline(PlayableDirector pDirector) => isEnd = true;

    public enum Winner
    {
        Player,
        Enemy
    }

    private void OnEnable()
    {
        playableDirector.stopped += EndTimeline;
    }

    public void Set(Card player, int playerBuff, int playerTotalCost, Card enemy, int enemyBuff, int enemyTotalCost)
    {
        Debug.Log(player);
        pCard.SetCard(player);
        eCard.SetCard(enemy);
        pCost.text = (player.Base.Cost + 1).ToString();
        eCost.text = (enemy.Base.Cost + 1).ToString();

        pBuff.text = playerBuff.ToString();
        eBuff.text = enemyBuff.ToString();

        pTotalCost.text = playerTotalCost.ToString();
        eTotalCost.text = enemyTotalCost.ToString();
    }

    public void SetTimer(float player, float enemy)
    {
        pTimer.text = (player * 1000f).ToString("F0") + "ms";
        eTimer.text = (enemy * 1000f).ToString("F0") + "ms";
    }

    public async UniTask StartJudge(Winner winner)
    {
        if (winner == Winner.Player)
        {
            playerTurn.text = FIRST;
            enemyTurn.text = SECOND;
        }
        else
        {
            playerTurn.text = SECOND;
            enemyTurn.text = FIRST;
        }
        
        playableDirector.Play(timelines[(int)winner]);
        while (!isEnd)
        {
            await UniTask.DelayFrame(1);
        }
        isEnd = false;
    }
}
