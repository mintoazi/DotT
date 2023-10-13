using UnityEngine;
using UniRx;

public sealed class BattlerPresenter : MonoBehaviour
{
    [SerializeField] private Battler model;
    [SerializeField] private BattlerUI view;

    private void Start()
    {
        // Player��Health���Ď�
        model.Health
            .Subscribe(x =>
            {
                // View�ɔ��f
                view.UpdateHP(x);
            }).AddTo(this);

        for (int i = 0; i < model.IsCostUses.Length; i++)
        {
            SubscribeUseCost(i);
        }
        void SubscribeUseCost(int n)
        {
            model.IsCostUses[n]
           .Subscribe(x =>
           {
               // View�ɔ��f
               view.UseCost(n, x);
           }).AddTo(this);
        }

        model.CurrentType
            .Subscribe(x =>
            {
                // View�ɔ��f
                view.UpdateType(x);
            }).AddTo(this);
    }
}
