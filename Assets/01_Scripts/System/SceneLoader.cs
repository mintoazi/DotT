using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
{
    public static SceneLoader instance;
    // シングルトンかつ、シーン遷移しても破棄されないようにする
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
        float time = Time.time; // ロード所要時間計測開始

        FadeAlpha.FadeOut(fadePanel).Forget(); // フェードアウト
        anim.SetTrigger("LoadStart");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName[(int)scene]); // 非同期でロードを行う

        // ロードが完了していても，シーンのアクティブ化は許可しない
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f) //0.9で止まる
        {
            await UniTask.Yield();
        }

        // (規定時間-所要時間)ローディング画面を表示させる
        float timeTaken = Time.time - time;
        if (timeTaken < loadTime) await UniTask.WaitForSeconds(loadTime - timeTaken);

        // ロードが完了したときにシーンのアクティブ化を許可する
        asyncLoad.allowSceneActivation = true;

        // ロードが完了するまで待つ
        await asyncLoad;

        anim.SetTrigger("LoadEnd");
        FadeAlpha.FadeIn(fadePanel).Forget(); // フェードイン
    }
}
