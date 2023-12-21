using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
{
    public enum Scene 
    {
        TITLE,
        HOME,
        MATCHING,
        BATTLE
    }

    [SerializeField] private Image fadePanel;

    private string[] sceneName = new string[4] { 
        "00_Title", // Scene.TITLE
        "01_Home",  // Scene.HOME
        "02_Matching",// Scene.MATCHING
        "03_Battle" // Scene.BATTLE
    }; 

    private const float loadTime = 2f;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public async UniTask Load(Scenes.Scene scene)
    {
        float time = Time.time; // ���[�h���v���Ԍv���J�n

        FadeAlpha.FadeOut(fadePanel).Forget(); // �t�F�[�h�A�E�g

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName[(int)scene]); // �񓯊��Ń��[�h���s��

        // ���[�h���������Ă��Ă��C�V�[���̃A�N�e�B�u���͋����Ȃ�
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) //0.9�Ŏ~�܂�
        {
            await UniTask.Yield();
        }

        // (�K�莞��-���v����)���[�f�B���O��ʂ�\��������
        float timeTaken = Time.time - time;
        if (timeTaken < loadTime) await UniTask.WaitForSeconds(loadTime - timeTaken);

        // ���[�h�����������Ƃ��ɃV�[���̃A�N�e�B�u����������
        asyncLoad.allowSceneActivation = true;

        // ���[�h����������܂ő҂�
        await asyncLoad;

        FadeAlpha.FadeIn(fadePanel).Forget(); // �t�F�[�h�C��
    }
}
