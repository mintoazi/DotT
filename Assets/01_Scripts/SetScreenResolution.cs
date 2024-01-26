using UnityEngine;

public class SetScreenResolution : SingletonMonoBehaviour<SetScreenResolution>
{
    public static SetScreenResolution instance;
    public override void CheckSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    bool isFullscreen = true;
    private void Awake()
    {
        if (isFullscreen)
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
