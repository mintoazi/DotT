using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class titleScenemovement : MonoBehaviour
{

    public void OnClickStartButton()
    {
        SceneManager.LoadScene("title");
    }
}
