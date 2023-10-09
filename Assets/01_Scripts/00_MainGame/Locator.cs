// サービスロケーター
public static class Locator<T> where T : class
{
    public static T Instance { get; private set; }

    public static bool IsValid() => Instance != null; // 有効かどうか

    // Instanceの値を設定する
    public static void Bind(T instance)
    {
        Instance = instance;
    }

    // Instanceの値をnullにする
    public static void Unbind(T instance)
    {
        if (Instance == instance)
        {
            Instance = null;
        }
    }

    // 強制的にnullにする
    public static void Clear()
    {
        Instance = null;
    }

    /* 使用方法 */
    /*
    Awake(),OnEnable()等で
    Locator<T>.Bind(this);でインスタンス登録
    
    OnDestroy(),OnDisable()等で
    Locator<T>.Unbind(this);で解除

    public virtual void Hoge(T hoge){}


    関数を実行する時は
    Locator<T>.Instance.Hoge(hoge);
    */
}
