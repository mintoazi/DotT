using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Attack : MonoBehaviour
{
    [SerializeField] private GenerateGame generateGame;
    [SerializeField] private GameObject[] effects = new GameObject[3];
    [SerializeField] private GameObject[] matchEffects = new GameObject[3];
    [SerializeField] private float destroyTime = 0.4f;
    private void OnEnable()
    {
        Locator<Attack>.Bind(this);
    }
    
    private void OnDestroy()
    {
        Locator<Attack>.Unbind(this);
    }

    public void PlayEffect(List<Vector2Int> pos, int type, bool isMatchType, bool isEnemy)
    {
        GameObject effect;
        int enemyPos = 3;
        if (isMatchType) effect = matchEffects[type];
        else effect = effects[type];
        
        for (int i = 0; i < pos.Count; i++)
        {
            GameObject e = Instantiate(effect, transform);
            int x = pos[i].x;
            int y = pos[i].y;
            if (!isEnemy)
            {
                y += enemyPos;
            }
            Vector3 p = generateGame.Tiles[x,y].transform.position;
            p.y += 0.1f;
            e.transform.position = p;
            Destroy(e, destroyTime);
        }
    }
}
