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

    public static List<Vector2Int> CalcDamagePos(Vector2Int attackerPos, List<Vector2Int> attackPos)
    {
        List<Vector2Int> pos = new List<Vector2Int>();

        for (int i = 0; i < attackPos.Count; i++)
        {
            int x = attackPos[i].x + (attackerPos.x - 1);
            int y = attackPos[i].y + (attackerPos.y - 1);
            if (x < 0 || x > 2 || y < 0 || y > 2)
            {
                continue;
            }

            pos.Add(new Vector2Int(x, y));
        }
        return pos;
    }
    public static List<Vector2Int> CalcEffectPos(Vector2Int attackerPos, List<Vector2Int> attackPos, int attackAhead)
    {
        List<Vector2Int> pos = new List<Vector2Int>();

        for (int i = 0; i < attackPos.Count; i++)
        {
            int y = attackPos[i].x + (attackerPos.x + attackAhead - 1);
            int x = attackPos[i].y + (attackerPos.y +  - 1);
            if (x < 0 || x > 2 || y < 0 || y > 6)
            {
                continue;
            }

            pos.Add(new Vector2Int(x, y));
        }
        return pos;
    }
}
