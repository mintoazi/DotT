using UnityEngine;
using UnityEngine.SceneManagement;

public class movieSceneMove : MonoBehaviour
{
    private float step_time;    // �o�ߎ��ԃJ�E���g�p

    // Use this for initialization
    void Start()
    {
        step_time = 0.0f;       // �o�ߎ��ԏ�����
    }

    // Update is called once per frame
    void Update()
    {
        // �o�ߎ��Ԃ��J�E���g
        step_time += Time.deltaTime;

        // 150�b��ɉ�ʑJ�ځititle�ֈړ��j
        if (step_time >= 150f)
        {
            SceneManager.LoadScene("title");
        }
    }
}