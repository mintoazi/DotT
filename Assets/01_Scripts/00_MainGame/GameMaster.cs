using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

public class GameMaster : MonoBehaviour
{
    [SerializeField] Battler player;
    [SerializeField] Battler enemy;
    [SerializeField] int hands = 0;
    private Battler currentBattler;
    private Battler waitBattler;
    private Phase phase;
    bool isPhase = false;
    private enum Phase
    {
        Init,
        CoinTos,
        Draw,
        ReDraw,
        ChangeType,
        PlayCard,
        Move,
        Attack,
        End
    }

    // �J�[�h�𐶐����Ĕz��
    private void Start()
    {
        SetUp();
        phase = Phase.Init;
        InitPhase();
    }
    private void ManagedUpdate()
    {
        if (isPhase) return;
        switch (phase)
        {
            case Phase.Init:
                Debug.Log("�����t�F�C�Y");
                InitPhase();
                break;
            case Phase.CoinTos:
                Debug.Log("�R�C���g�X�t�F�C�Y");
                SetTurn();
                break;
            case Phase.Draw:
                Debug.Log("�h���[�t�F�C�Y");
                DrawPhase();
                break; 
            case Phase.ReDraw:
                Debug.Log("���h���[�t�F�C�Y");
                ReDrawPhase().Forget();
                break;
            case Phase.ChangeType:
                Debug.Log("�����ύX�t�F�C�Y");
                ChangeTypePhase().Forget();
                break;
            case Phase.PlayCard:
                Debug.Log("�J�[�h�v���C�t�F�C�Y");
                PlayCardPhase().Forget();
                break;
            case Phase.Move:
                Debug.Log("�ړ��t�F�C�Y");
                MovePhase().Forget();
                break;
            case Phase.Attack:
                break;
            case Phase.End:
                Debug.Log("�G���h�t�F�C�Y");
                EndPhase().Forget();
                break;
        }
    }
    private void SetUp()
    {
        //player.OnSubmitAction = SubmittedAction;
        //Update�����̒ǉ�
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void InitPhase()
    {
        isPhase = true;

        SendCardsTo(player);
        SendCardsTo(enemy);
        phase = Phase.CoinTos;

        void SendCardsTo(Battler target)
        {
            for (int i = 0; i < hands; i++)
            {
                Card card = Locator<CardGenerator>.Instance.Draw();
                target.SetCardToHand(card);
            }
        }

        isPhase = false;
    }

    // �^�[���̐ݒ�
    private void SetTurn()
    {
        isPhase = true;

#if false
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            currentBattler = player;
            waitBattler = enemy;
        }
        else
        {
            currentBattler = enemy;
            waitBattler = player;
        }
#else
        currentBattler = player;
        waitBattler = enemy;
#endif
        phase = Phase.Draw;

        isPhase = false;
    }

    private void DrawPhase()
    {
        isPhase = true;
        currentBattler.Draw();
        phase = Phase.ChangeType;
        isPhase = false;
    }
    private async UniTask ReDrawPhase()
    {
        isPhase = true;
        await currentBattler.ReDraw();
        phase = Phase.PlayCard;
        isPhase = false;
    }

    private async UniTask ChangeTypePhase()
    {
        isPhase = true;
        await currentBattler.ChangeType();
        phase = Phase.ReDraw;
        isPhase = false;
    }

    private async UniTask PlayCardPhase()
    {
        isPhase = true;
        await currentBattler.PlayCard();
        phase = Phase.Move;
        isPhase = false;
    }
    
    private async UniTask MovePhase()
    {
        isPhase = true;
        await currentBattler.Move();
        phase = Phase.End;
        isPhase = false;
    }

    /*
    private async UniTask AttackPhase()
    {
        isPhase = true;
        await currentBattler.Attack();
        phase = Phase.Move;
        isPhase = false;
    }*/
    private async UniTask EndPhase()
    {
        isPhase = true;
        if(currentBattler == player)
        {
            currentBattler = enemy;
            waitBattler = player;
        }
        else
        {
            currentBattler = player;
            waitBattler = enemy;
        }
        phase = Phase.Draw;
        isPhase = false;
    }
    
    //private void SubmittedAction()
    //{
    //    if (currentBattler.IsSubmit)
    //    {
    //        submitButton.SetActive(false);
    //    }
    //}   
}
