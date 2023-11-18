using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class tes : MonoBehaviour
{
    private void Start()
    {
        Rot().Forget();
    }
    async UniTask Rot()
    {
        Quaternion from = transform.rotation;
        Quaternion to = Quaternion.Euler(0f, 90f, 0f);
        float speed = 4f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            transform.rotation = Quaternion.Lerp(from, to, t);
            await UniTask.DelayFrame(1);
        }
        //hidePanel.SetActive(false);
        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * speed;
            transform.rotation = Quaternion.Lerp(from, to, t);
            await UniTask.DelayFrame(1);
        }
    }
}
