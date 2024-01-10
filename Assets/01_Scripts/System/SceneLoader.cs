using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
{
    public static SceneLoader instance;
    // �V���O���g�����A�V�[���J�ڂ��Ă��j������Ȃ��悤�ɂ���
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

    [SerializeField] private Image fadePanel;
    [SerializeField] private Animator anim;

    private string[] sceneName = new string[6] { 
        "00_Title", // Scene.TITLE
        "01_Home",  // Scene.HOME
        "02_Matching",// Scene.MATCHING
        "03_Battle",  // Scene.BATTLE
        "03_Battle",
        "05_CPUBattle"
    }; 

    private const float loadTime = 4f;

    
    public async UniTask Load(Scenes.Scene scene)
    {
        float time = Time.time; // ���[�h���v���Ԍv���J�n

        FadeAlpha.FadeOut(fadePanel).Forget(); // �t�F�[�h�A�E�g
        anim.SetTrigger("LoadStart");
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

        anim.SetTrigger("LoadEnd");
        FadeAlpha.FadeIn(fadePanel).Forget(); // �t�F�[�h�C��
    }
}
