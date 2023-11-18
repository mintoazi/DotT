using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LogMenu : MonoBehaviour
{
    public static LogMenu instance;

    [SerializeField]
    private Text m_textUI = null;
    [SerializeField]
    private ScrollRect scrollRect;

    private void Awake()
    {
        SceneManager.activeSceneChanged += ActiveSceneChanged;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        m_textUI.text = null;
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessage;
    }

    private void ActiveSceneChanged(Scene thisScene, Scene nextScene)
    {
        //m_textUI.text = null;
    }
    private void OnLogMessage(string i_logText, string i_stackTrace, LogType i_type)
    {
        if (!m_textUI) return;
        if (string.IsNullOrEmpty(i_logText))
        {
            return;
        }
        if (!string.IsNullOrEmpty(i_stackTrace))
        {
            switch (i_type)
            {
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    i_logText += System.Environment.NewLine + i_stackTrace;
                    break;
                default:
                    break;
            }
        }
        switch (i_type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                i_logText = string.Format("<color=red>{0}</color>", i_logText);
                break;
            case LogType.Warning:
                i_logText = string.Format("<color=yellow>{0}</color>", i_logText);
                break;
            default:
                break;
        }
        m_textUI.text += i_logText + System.Environment.NewLine;
        int LineCount = m_textUI.cachedTextGeneratorForLayout.lineCount;
        if (LineCount >= 100) m_textUI.text = "";
        m_textUI.GetComponent<ContentSizeFitter>().SetLayoutVertical();
        scrollRect.verticalNormalizedPosition = 0;
    }
} // class LogMenu