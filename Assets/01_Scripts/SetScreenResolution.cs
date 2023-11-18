using UnityEngine;

public class SetScreenResolution : MonoBehaviour
{
    bool isFullscreen = true;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(isFullscreen)
            {
                Screen.SetResolution(1920 / 2, 1080 / 2, false);
                isFullscreen = false;
            }
            else
            {
                Screen.SetResolution(1920, 1080, true);
                isFullscreen = true;
            }
        }
    }
}
