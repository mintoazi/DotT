using UnityEngine;
using UniRx;
using UniRx.Triggers;
public class InGameSetting : MonoBehaviour
{
    [SerializeField] GameObject settingPanel;
    private void Awake()
    {
        //Updateˆ—‚Ì“o˜^
        this.UpdateAsObservable()
        .Subscribe(
            _ => ManagedUpdate()
        );
    }
    private void ManagedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickSetting();
        }
    }
    public void Hide()
    {
        settingPanel.SetActive(false);
    }

    public void OnClickSetting()
    {
        settingPanel.SetActive(!settingPanel.activeSelf);
    }
}
