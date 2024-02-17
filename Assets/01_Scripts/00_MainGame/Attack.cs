using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [SerializeField] private GenerateGame generateGame;
    
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy;

    [SerializeField] private GameObject[] effects = new GameObject[3];
    [SerializeField] private GameObject[] matchEffects = new GameObject[3];
    [SerializeField] private float destroyTime = 0.4f;
    private void OnEnable()
    {
        Locator<Attack>.Bind(this);
        //List<Vector2Int> v = new List<Vector2Int>
        //{
        //    new Vector2Int(3, 0)
        //};
        //MagicEffect(v, false);
    }

    private void OnDestroy()
    {
        Locator<Attack>.Unbind(this);
    }

    public void PlayEffect(List<Vector2Int> pos, int type, bool isMatchType, bool isEnemy)
    {
        //if (type == 2)
        //{
        //    MagicEffect(pos, isEnemy);
        //    return;
        //}
        GameObject effect;
        List<Vector2Int> effectPos = pos;
        //if (isMatchType) effect = matchEffects[type];
        //else effect = effects[type];
        effect = effects[type];
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

    private void MagicEffect(List<Vector2Int> pos, bool isEnemy)
    {
        Vector3 startPos;
        if (isEnemy) startPos = enemy.transform.position;
        else startPos = player.transform.position;

        List<Vector2Int> effectPos = pos;
        for(int i = 0; i < pos.Count; i++)
        {
            GameObject effect = Instantiate(effects[2]);
            Vector3 target = generateGame.Tiles[effectPos[i].x, effectPos[i].y].transform.position;
            effect.transform.position = startPos;
            MoveEffect(target,effect).Forget();
        }
    }

    private async UniTask MoveEffect(Vector3 target, GameObject effect)
    {
        Vector3 position = effect.transform.position;
        Vector3 velocity = new Vector3(Random.Range(-1.0f, 1.0f), 1f, 0f);
        float period = 1.0f;
        
        while(period > 0f)
        {
            var acceleration = Vector3.zero;
            var diff = target - transform.position;
            acceleration += (diff - velocity * period) * 2f / (period * period);
            if (acceleration.magnitude > 10f)
            {
                acceleration = acceleration.normalized * 10f;
            }
            period -= Time.deltaTime;

            velocity += acceleration * Time.deltaTime;
            position += velocity * Time.deltaTime;
            effect.transform.position = position;
            await UniTask.Yield();
        }
        Destroy(effect);
    }
}
