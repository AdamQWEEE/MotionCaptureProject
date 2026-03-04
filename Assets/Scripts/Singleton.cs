using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            lock (_lock)
            {
                // ✅ Unity 6 推荐的查找方式
                _instance = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);

                if (_instance == null)
                {
                    // 若场景中不存在，自动创建
                    GameObject singletonObj = new GameObject(typeof(T).Name);
                    _instance = singletonObj.AddComponent<T>();
                    DontDestroyOnLoad(singletonObj);
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // ✅ 防止重复实例
            Destroy(gameObject);
        }
    }
}
