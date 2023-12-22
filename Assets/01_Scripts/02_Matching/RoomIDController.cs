using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RoomIDController : MonoBehaviour
{
    [SerializeField] private Text[] ids;
    [SerializeField] private Text id;
    [SerializeField] private InputField inputID;

    public void OnValueChanged()
    {
        foreach(Text t in ids)
        {
            t.text = null;
        }
        for(int i = 0; i < inputID.text.Length; i++)
        {
            ids[i].text = inputID.text[i].ToString(); // ‚P•¶Žš‚¸‚Â•\Ž¦
        }
    }
    public void OnJoinRoom()
    {
        if (inputID.text.Length < inputID.characterLimit) return;
        OnlineMenuManager.onlineManager.OnMatchingButton(inputID.text);
        Locator<MatchingManager>.Instance.SetState(MatchingManager.State.CharaSelect);
    }

    public void OnLeftScene()
    {
        SceneLoader.Instance.Load(Scenes.Scene.HOME).Forget();
    }
}
