using UnityEngine;
using UnityEngine.SceneManagement;
public class InGameSetting : MonoBehaviour
{
    [SerializeField] GameObject settingPanel;
    [SerializeField] GameMaster gameMaster;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingPanel.SetActive(!settingPanel.activeSelf);
        }
    }
}
