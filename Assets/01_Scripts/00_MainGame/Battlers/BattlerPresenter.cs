using UnityEngine;
using UniRx;

public sealed class BattlerPresenter : MonoBehaviour
{
    [SerializeField] private BattlerModel model;
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

        model.CharaType
            .Subscribe(x =>
            {
                // View�ɔ��f
                view.UpdateCharaType(x);
            }).AddTo(this);

        model.AttackBuff
            .Subscribe(x =>
            {
                view.UpdateAttackBuff(x);
            }).AddTo(this);

        model.DefenceBuff
            .Subscribe(x =>
            {
                view.UpdateDefenceBuff(x);
            }).AddTo(this);

        model.CostBuff
            .Subscribe(x =>
            {
                view.UpdateCostBuff(x);
            }).AddTo(this);

        model.IsDamage
            .Subscribe(x =>
            {
                view.DamageEffect(x);
            }).AddTo(this);
    }
}
