using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIAutoSFX_Global : MonoBehaviour
{
    private static UIAutoSFX_Global instance;

    [Header("Sound ID for UI Click")]
    public string uiClickID = "ui_click";

    void Awake()
    {
        // Singleton
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Hook scene change
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        InjectButtonSounds();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InjectButtonSounds();
    }

    void InjectButtonSounds()
    {
    #if UNITY_2023_1_OR_NEWER
        // Include inactive objects (same behavior as FindObjectsOfType(true))
        Button[] buttons = FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
    #else
        // Old API fallback
        Button[] buttons = FindObjectsOfType<Button>(true);
    #endif

        foreach (Button btn in buttons)
        {
            btn.onClick.RemoveListener(PlayClickSound);
            btn.onClick.AddListener(PlayClickSound);
        }
    }


    void PlayClickSound()
    {
        SoundManager.Instance.PlayUI(uiClickID);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        // Automatically create one if missing
        if (instance == null)
        {
            GameObject obj = new GameObject("UIAutoSFX_Global");
            instance = obj.AddComponent<UIAutoSFX_Global>();
        }
    }
}
