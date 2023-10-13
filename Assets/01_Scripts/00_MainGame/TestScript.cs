using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private int id = 0;
    [SerializeField] CardGenerator cg;
    [SerializeField] private Vector2Int myPos;
    private void Start()
    {
        List<Vector2Int> a;
        List<Vector2Int> b = new List<Vector2Int>();
        b.Add(new Vector2Int(1, 1));
        b.Add(new Vector2Int(1, 0));
        b.Add(new Vector2Int(1, 2));
        a = CalcAttackPos(new Vector2Int(0, 0), b);
    }

    // Attack(CalcAttackPos(PiecePos, card.Base.AttackPos), card.Base.Damage);
    // Attack(CalcAttackPos(PiecePos, card.Base.SAttackPos), card.Base.SDamage);
    private void Attack(List<Vector2Int> attackPos, int damage)
    {
        // StartEffect

        // 攻撃エフェクト処理
        // Instantiate(prefab, transform.Position)

        // 相手の攻撃エフェクト処理
        // 自分のダメージ処理
        // enemy.Damage(attackPos, damage);
        // photonView.RPC(Damage,All,attackPos)
    }
    private void Damage(List<Vector2Int> attackPos, int damage)
    {
        for (int i = 0; i < attackPos.Count; i++)
        {
            if (myPos == attackPos[i])
            {
                //HP - damage
            }
        }
    }

    // 自分の位置と攻撃範囲から敵に伝える攻撃範囲の計算
    private List<Vector2Int> CalcAttackPos(Vector2Int myPos, List<Vector2Int> attackPos)
    {
        List<Vector2Int> pos = new List<Vector2Int>();
        for (int i = 0; i < attackPos.Count; i++)
        {
            int x = attackPos[i].x - myPos.x - 1;
            int y = attackPos[i].y - myPos.y - 1;
            if (x < 0 || x > 2 || y < 0 || y > 2)
            {
                continue;
            }
            pos.Add(new Vector2Int(x, y));
        }

        for (int i = 0; i < pos.Count; i++)
        {
            Debug.Log(pos[i]);
        }
        return pos;
    }
}
