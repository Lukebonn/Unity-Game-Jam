using UnityEngine;
using UnityEngine.SceneManagement;

public class menuManager : MonoBehaviour
{
    private static menuManager _instance;
    public static menuManager Instance
    {
        get
        {
            // Lazy initialization 
            if (_instance == null)
            {
                _instance = FindObjectOfType<menuManager>();

                if (_instance == null)
                {
                    Debug.Log("menuManager not found, creating new instance...");
                    GameObject go = new GameObject("menuManager");
                    _instance = go.AddComponent<menuManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }
    //Converts different scnes to these names can be expanded upon for multiple levels
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Game State")]
    [SerializeField] private bool isGamePaused = false;

    private bool isLoading = false;

    public bool IsGamePaused
    {
        get => isGamePaused;
        set => isGamePaused = value;
    }
    //Makes sur ethere is no dupluicate menue manager (needs checking to see if still needed)
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.Log("Duplicate menuManager destroyed");
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("menuManager initialized and persistent");
    }
    //resets game state on start
    private void Start()
    {
        ResetGameState();
    }
    //calles the game starting
    public void StartGame()
    {
        if (isLoading) return;

        Debug.Log("menuManager: Starting game...");
        isLoading = true;
        ResetGameState();
        SceneManager.LoadScene(gameSceneName);
    }
    //Return to main Menu
    public void ReturnToMainMenu()
    {
        if (isLoading) return;

        Debug.Log("menuManager: Returning to main menu...");
        isLoading = true;
        ResetGameState();
        SceneManager.LoadScene(mainMenuSceneName);
    }
    //After quiting the game
    public void QuitGame()
    {
        Debug.Log("menuManager: Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //Reset the game state
    private void ResetGameState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", 1f);
        isGamePaused = false;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }
    //allowing and not allowing the cursor in certain scenes
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoading = false;
        Debug.Log($"menuManager: Loaded scene: {scene.name}");

        if (scene.name == gameSceneName)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
        else if (scene.name == mainMenuSceneName)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}