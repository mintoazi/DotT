using System.Collections.Generic;
using UnityEngine;

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
        List<Vector2Int> effectPos = pos;
        if (isMatchType) effect = matchEffects[type];
        else effect = effects[type];
        
        for (int i = 0; i < pos.Count; i++)
        {
            GameObject e = Instantiate(effect, transform);

            if (isEnemy)
            {
                //e.transform.rotation = Quaternion.Euler(0, 180, 0);
            }

            Vector3 p = generateGame.Tiles[effectPos[i].x ,effectPos[i].y].transform.position;
            p.y += 0.1f;
            e.transform.position = p;
            Destroy(e, destroyTime);
        }
    }
}
