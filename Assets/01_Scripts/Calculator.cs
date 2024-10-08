using System.Collections.Generic;
using UnityEngine;

public class Calculator
{
    private const int Length = 3;
    public static Vector2Int IntToVector2Int(int i)
    {
        Vector2Int v2i = new Vector2Int(i / Length, i % Length);
        return v2i;
    }
    public static List<Vector2Int> IntToVector2Int(List<int> intList)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int i = 0; i < intList.Count; i++)
        {
            list.Add(IntToVector2Int(intList[i]));
        }
        return list;
    }
    public static int Vector2IntToInt(Vector2Int v2i)
    {
        int i = v2i.x * Length + v2i.y;
        return i;
    }
    public static List<int> Vector2IntToInt(List<Vector2Int> v2iList)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < v2iList.Count; i++)
        {
            list.Add(Vector2IntToInt(v2iList[i]));
        }
        return list;
    }

    public static Vector2Int CalcReflection(Vector2Int v2i)
    {
        int x = Mathf.Abs(v2i.x - 2);
        int y = Mathf.Abs(v2i.y - 2);
        return new Vector2Int(x, y);
    }

    public static List<Vector2Int> CalcReflection(List<Vector2Int> list)
    {
        List<Vector2Int> returnList = new List<Vector2Int>();
        for (int i = 0; i < list.Count; i++)
        {
            returnList.Add(CalcReflection(list[i]));
        }
        return returnList;
    }

    public static List<Vector2Int> CalcEnemyPosition(List<Vector2Int> pos)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int i = 0; i < pos.Count; i++)
        {
            list.Add(CalcEnemyPosition(pos[i]));
        }
        return list;
    }
    public static Vector2Int CalcEnemyPosition(Vector2Int pos)
    {
        return new Vector2Int(pos.x + Length, pos.y);
    }

    /// <summary>
    /// 攻撃場所の計算
    /// </summary>
    /// <param name="attackerPos">攻撃する側</param>
    /// <param name="attackPos">攻撃の場所</param>
    /// <param name="attackAhead">何マス先に攻撃するか</param>
    /// <returns>計算された攻撃場所</returns>
    public static Vector2Int CalcAttackPosition(Vector2Int attackerPos, Vector2Int attackPos, int attackAhead)
    {
        Vector2Int v2i = new Vector2Int(0, 0);

        v2i.x = attackPos.x + attackerPos.x - 1 + attackAhead - Length;
        v2i.y = attackPos.y + attackerPos.y - 1;

        return v2i;
    }
    /// <summary>
    /// 攻撃場所の計算(複数)
    /// </summary>
    /// <param name="attackerPos">攻撃する側</param>
    /// <param name="attackPos">攻撃の場所</param>
    /// <param name="attackAhead">何マス先に攻撃するか</param>
    /// <returns>計算された攻撃場所</returns>
    public static List<Vector2Int> CalcAttackPosition(Vector2Int attackerPos, List<Vector2Int> attackPos, int attackAhead)
    {
        List<Vector2Int> attackPositions = new List<Vector2Int>();
        //Debug.Log("攻撃する側の位置" + attackerPos);
        for (int i = 0; i < attackPos.Count; i++)
        {
            Vector2Int v2i = CalcAttackPosition(attackerPos, attackPos[i], attackAhead);
            if (!(v2i.x < Length && v2i.x > -4 && v2i.y < Length && v2i.y > -1)) continue;
            attackPositions.Add(v2i);
            //Debug.Log("ダメージ位置" + v2i);
        }
        return attackPositions;
    }
}
