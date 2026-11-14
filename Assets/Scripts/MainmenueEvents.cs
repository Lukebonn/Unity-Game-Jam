using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;

public class MainmenueEvents : MonoBehaviour
{
    private UIDocument _document;

    private Button _startButton;
    private Button _exitButton;
    private Button _settingsButton;
    private Button _creditsButton;

    private VisualElement _mainMenuPanel;
    private VisualElement _settingsPanel;
    private Button _closeSettingsButton;
    private Slider _volumeSlider;
    private Toggle _fullscreenToggle;

    private VisualElement _creditsPanel;
    private Button _closeCreditsButton;

    private VisualElement _quitConfirmPanel;
    private Button _confirmQuitButton;
    private Button _cancelQuitButton;

    private void Awake()
    {
        // Ensure Event System exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Debug.Log(" Created Event System");
        }

        _document = GetComponent<UIDocument>();
        var root = _document.rootVisualElement;

        // Set all VisualElements to ignore clicks initially
        var allElements = root.Children().ToList();
        foreach (var element in allElements)
        {
            element.pickingMode = PickingMode.Ignore;
        }

        // Set only buttons to receive clicks
        var allButtons = root.Query<Button>().Build().ToList();
        foreach (var button in allButtons)
        {
            button.pickingMode = PickingMode.Position;
        }

        // Set sliders and toggles to receive clicks
        var allSliders = root.Query<Slider>().Build().ToList();
        foreach (var slider in allSliders)
        {
            slider.pickingMode = PickingMode.Position;
        }

        var allToggles = root.Query<Toggle>().Build().ToList();
        foreach (var toggle in allToggles)
        {
            toggle.pickingMode = PickingMode.Position;
        }

        // Find main menu panel
        _mainMenuPanel = root.Query<VisualElement>("MainMenuPanel").First();
        if (_mainMenuPanel == null)
        {
            _mainMenuPanel = root.Query<VisualElement>("MainContainer").First();
        }

        // Find all buttons and panels
        _startButton = root.Q<Button>("StartGameButton");
        _exitButton = root.Q<Button>("ExitButton");
        _creditsButton = root.Q<Button>("CreditsButton");
        _settingsButton = root.Q<Button>("SettingsButton");

        _settingsPanel = root.Q<VisualElement>("SettingsPanel");
        if (_settingsPanel != null)
        {
            _closeSettingsButton = _settingsPanel.Q<Button>("CloseSettingsButton");
            _volumeSlider = _settingsPanel.Q<Slider>("VolumeSlider");
            _fullscreenToggle = _settingsPanel.Q<Toggle>("FullscreenToggle");
        }

        _creditsPanel = root.Q<VisualElement>("CreditsPanel");
        if (_creditsPanel != null)
        {
            _closeCreditsButton = _creditsPanel.Q<Button>("CloseCreditsButton");
        }

        _quitConfirmPanel = root.Q<VisualElement>("QuitConfirmPanel");
        if (_quitConfirmPanel != null)
        {
            _confirmQuitButton = _quitConfirmPanel.Q<Button>("ConfirmQuitButton");
            _cancelQuitButton = _quitConfirmPanel.Q<Button>("CancelQuitButton");
        }

        // Register button events
        if (_startButton != null)
            _startButton.clicked += OnPlayGameClick;
        if (_exitButton != null)
            _exitButton.clicked += OnExitGameClick;
        if (_creditsButton != null)
            _creditsButton.clicked += OnOpenCredits;
        if (_settingsButton != null)
            _settingsButton.clicked += OnOpenSettings;
        if (_closeSettingsButton != null)
            _closeSettingsButton.clicked += OnCloseSettings;
        if (_closeCreditsButton != null)
            _closeCreditsButton.clicked += OnCloseCredits;
        if (_confirmQuitButton != null)
            _confirmQuitButton.clicked += OnConfirmQuit;
        if (_cancelQuitButton != null)
            _cancelQuitButton.clicked += OnCancelQuit;

        LoadSettings();

        if (_volumeSlider != null)
            _volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
        if (_fullscreenToggle != null)
            _fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);

        root.RegisterCallback<KeyDownEvent>(OnKeyDown);
    }

    private void Start()
    {
        ShowMainMenu();
        HideAllOverlayPanels();
        Debug.Log("Main menu ready");
    }

    private void ShowMainMenu()
    {
        if (_mainMenuPanel != null)
        {
            _mainMenuPanel.style.display = DisplayStyle.Flex;
            _mainMenuPanel.pickingMode = PickingMode.Ignore;
        }
    }

    private void HideMainMenu()
    {
        if (_mainMenuPanel != null)
        {
            _mainMenuPanel.style.display = DisplayStyle.None;
        }
    }

    private void HideAllOverlayPanels()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _settingsPanel.pickingMode = PickingMode.Ignore;
        }
        if (_creditsPanel != null)
        {
            _creditsPanel.style.display = DisplayStyle.None;
            _creditsPanel.pickingMode = PickingMode.Ignore;
        }
        if (_quitConfirmPanel != null)
        {
            _quitConfirmPanel.style.display = DisplayStyle.None;
            _quitConfirmPanel.pickingMode = PickingMode.Ignore;
        }
    }




    private void OnOpenSettings()
    {
        Debug.Log("Opening Settings");
        HideMainMenu();
        HideAllOverlayPanels();

        if (_settingsPanel != null)
        {
            _settingsPanel.style.display = DisplayStyle.Flex;
            _settingsPanel.pickingMode = PickingMode.Position;
        }
    }

    private void OnOpenCredits()
    {
        Debug.Log("Opening Credits");
        HideMainMenu();
        HideAllOverlayPanels();

        if (_creditsPanel != null)
        {
            _creditsPanel.style.display = DisplayStyle.Flex;
            _creditsPanel.pickingMode = PickingMode.Position;
        }
    }

    private void OnExitGameClick()
    {
        Debug.Log("Exit clicked - showing confirmation");
        HideMainMenu();
        HideAllOverlayPanels();

        if (_quitConfirmPanel != null)
        {
            _quitConfirmPanel.style.display = DisplayStyle.Flex;
            _quitConfirmPanel.pickingMode = PickingMode.Position;
            _quitConfirmPanel.style.position = Position.Absolute;
            _quitConfirmPanel.style.top = 0;
            _quitConfirmPanel.style.left = 0;
            _quitConfirmPanel.style.right = 0;
            _quitConfirmPanel.style.bottom = 0;
            _quitConfirmPanel.style.justifyContent = Justify.Center;
            _quitConfirmPanel.style.alignItems = Align.Center;
        }
    }

    private void OnCloseSettings()
    {
        Debug.Log("Closing Settings");
        if (_settingsPanel != null)
        {
            _settingsPanel.style.display = DisplayStyle.None;
            _settingsPanel.pickingMode = PickingMode.Ignore;
        }
        SaveSettings();
        ShowMainMenu();
    }

    private void OnCloseCredits()
    {
        Debug.Log("Closing Credits");
        if (_creditsPanel != null)
        {
            _creditsPanel.style.display = DisplayStyle.None;
            _creditsPanel.pickingMode = PickingMode.Ignore;
        }
        ShowMainMenu();
    }

    private void OnPlayGameClick()
    {
        Debug.Log("Starting game...");

        // Try to find menuManager if Instance is null
        if (menuManager.Instance == null)
        {
            Debug.Log("menuManager.Instance was null, trying to find it in scene...");

            // Try to find existing menuManager
            menuManager foundManager = FindObjectOfType<menuManager>();

            if (foundManager != null)
            {
                Debug.Log("Found menuManager in scene, but Instance was null. This shouldn't happen!");
                // Use the found manager directly
                foundManager.StartGame();
                return;
            }
            else
            {
                // Create menuManager on the fly as last resort
                Debug.Log("No menuManager found, creating one dynamically...");
                GameObject managerGO = new GameObject("menuManager_Dynamic");
                menuManager newManager = managerGO.AddComponent<menuManager>();
                DontDestroyOnLoad(managerGO);
                newManager.StartGame();
                return;
            }
        }

        // Normal flow
        menuManager.Instance.StartGame();
    }

    private void OnConfirmQuit()
    {
        Debug.Log("Quitting Game...");

        // FIXED: Changed GameManager to menuManager
        if (menuManager.Instance != null)
        {
            menuManager.Instance.QuitGame();
        }
        else
        {
            // Fallback
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void OnCancelQuit()
    {
        Debug.Log("Cancel Quit");
        if (_quitConfirmPanel != null)
        {
            _quitConfirmPanel.style.display = DisplayStyle.None;
            _quitConfirmPanel.pickingMode = PickingMode.Ignore;
        }
        ShowMainMenu();
    }

    private void OnVolumeChanged(ChangeEvent<float> evt)
    {
        AudioListener.volume = evt.newValue;
    }

    private void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        Screen.fullScreen = evt.newValue;
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (_volumeSlider != null)
            _volumeSlider.value = savedVolume;
        if (_fullscreenToggle != null)
            _fullscreenToggle.value = savedFullscreen;

        AudioListener.volume = savedVolume;
        Screen.fullScreen = savedFullscreen;
    }

    private void SaveSettings()
    {
        if (_volumeSlider != null)
            PlayerPrefs.SetFloat("Volume", _volumeSlider.value);
        if (_fullscreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", _fullscreenToggle.value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Escape)
        {
            if (_settingsPanel != null && _settingsPanel.style.display == DisplayStyle.Flex)
                OnCloseSettings();
            else if (_creditsPanel != null && _creditsPanel.style.display == DisplayStyle.Flex)
                OnCloseCredits();
            else if (_quitConfirmPanel != null && _quitConfirmPanel.style.display == DisplayStyle.Flex)
                OnCancelQuit();
        }
    }

    private void OnDisable()
    {
        if (_document == null || _document.rootVisualElement == null)
            return;

        if (_startButton != null)
            _startButton.clicked -= OnPlayGameClick;
        if (_exitButton != null)
            _exitButton.clicked -= OnExitGameClick;
        if (_creditsButton != null)
            _creditsButton.clicked -= OnOpenCredits;
        if (_settingsButton != null)
            _settingsButton.clicked -= OnOpenSettings;
        if (_closeSettingsButton != null)
            _closeSettingsButton.clicked -= OnCloseSettings;
        if (_closeCreditsButton != null)
            _closeCreditsButton.clicked -= OnCloseCredits;
        if (_confirmQuitButton != null)
            _confirmQuitButton.clicked -= OnConfirmQuit;
        if (_cancelQuitButton != null)
            _cancelQuitButton.clicked -= OnCancelQuit;

        if (_document.rootVisualElement != null)
            _document.rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
    }
}