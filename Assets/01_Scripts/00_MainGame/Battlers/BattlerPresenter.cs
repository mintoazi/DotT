using UnityEngine;
using UniRx;

public sealed class BattlerPresenter : MonoBehaviour
{
    [SerializeField] private Battler model;
    [SerializeField] private BattlerUI view;

    private void Start()
    {
        // Player‚ÌHealth‚ðŠÄŽ‹
        model.Health
            .Subscribe(x =>
            {
                // View‚É”½‰f
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
               // View‚É”½‰f
               view.UseCost(n, x);
           }).AddTo(this);
        }

        model.CurrentType
            .Subscribe(x =>
            {
                // View‚É”½‰f
                view.UpdateType(x);
            }).AddTo(this);
    }
}
