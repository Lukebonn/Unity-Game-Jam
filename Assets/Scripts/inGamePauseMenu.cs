using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class InGamePauseMenu : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _pausePanel;
    private VisualElement _controlsPanel;
    private bool isPaused = false;
    private bool isInitialized = false;

    private Button _resumeButton;
    private Button _controlButton;
    private Button _quitButton;
    private Button _closeControlButton;

    private void Start()
    {
        InitializeUI();

        // Ensure cursor starts in correct state for gameplay
        SetGameCursorState();

        // Debug: Check if EventSystem exists 
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.LogWarning("No EventSystem found! Creating one for UI Toolkit input.");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private void InitializeUI()
    {
        _document = GetComponent<UIDocument>();
        if (_document == null)
        {
            Debug.LogError("UIDocument component not found!");
            return;
        }

        var root = _document.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Root VisualElement not found!");
            return;
        }

        // Find panels
        _pausePanel = root.Q<VisualElement>("PausePanel");
        _controlsPanel = root.Q<VisualElement>("ControlsPanel");

        // Find buttons
        _resumeButton = root.Q<Button>("ResumeButton");
        _controlButton = root.Q<Button>("ControlButton");
        _quitButton = root.Q<Button>("QuitButton");

        if (_controlsPanel != null)
        {
            _closeControlButton = _controlsPanel.Q<Button>("CloseControlButton");
        }

        // Initially hide all panels and set picking modes
        if (_pausePanel != null)
        {
            _pausePanel.style.display = DisplayStyle.None;
            _pausePanel.pickingMode = PickingMode.Ignore;
            Debug.Log("Pause panel initialized - hidden");
        }
        else
        {
            Debug.LogError("PausePanel not found! Make sure the element name is exactly 'PausePanel'");
        }

        if (_controlsPanel != null)
        {
            _controlsPanel.style.display = DisplayStyle.None;
            _controlsPanel.pickingMode = PickingMode.Ignore;
            Debug.Log("Controls panel initialized - hidden");
        }
        else
        {
            Debug.LogWarning("ControlsPanel not found");
        }

        // Set all buttons to be clickable
        var allButtons = root.Query<Button>().Build();
        foreach (var button in allButtons)
        {
            button.pickingMode = PickingMode.Position;
        }

        // Register button events
        if (_resumeButton != null)
            _resumeButton.clicked += OnResumeClick;
        else
            Debug.LogWarning("ResumeButton not found!");

        if (_controlButton != null)
            _controlButton.clicked += OnOpenControls;
        else
            Debug.LogWarning("ControlButton not found!");

        if (_quitButton != null)
            _quitButton.clicked += OnQuitClick;
        else
            Debug.LogWarning("QuitButton not found!");

        if (_closeControlButton != null)
            _closeControlButton.clicked += OnCloseControls;
        else
            Debug.LogWarning("CloseControlButton not found in ControlsPanel!");

        // Register keyboard callback 
        root.RegisterCallback<KeyDownEvent>(OnKeyDown);
        root.focusable = true; // Make sure root can receive keyboard events
        root.Focus(); // Focus the root element

        isInitialized = true;
        Debug.Log("InGamePauseMenu initialized - Escape and Q keys should now work");
    }

    private void Update()
    {
        // Fallback: Also check input in Update() 
        if (!isInitialized) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"Update() detected key press: {(Input.GetKeyDown(KeyCode.Escape) ? "Escape" : "Q")}");
            TogglePause();
        }
    }
    //Ties buttons in pause menue to script
    private void OnDestroy()
    {
        if (_document != null && _document.rootVisualElement != null)
        {
            var root = _document.rootVisualElement;

            if (_resumeButton != null)
                _resumeButton.clicked -= OnResumeClick;
            if (_controlButton != null)
                _controlButton.clicked -= OnOpenControls;
            if (_quitButton != null)
                _quitButton.clicked -= OnQuitClick;
            if (_closeControlButton != null)
                _closeControlButton.clicked -= OnCloseControls;

            root.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }
    }
    //adds escape button functionality
    private void OnKeyDown(KeyDownEvent evt)
    {
        Debug.Log($"KeyDownEvent received: {evt.keyCode}");

        if (evt.keyCode == KeyCode.Escape || evt.keyCode == KeyCode.Q)
        {
            Debug.Log($"Valid pause key pressed: {evt.keyCode}");
            TogglePause();
            evt.StopPropagation(); // Prevent other handlers from processing
        }
    }

    private void TogglePause()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("TogglePause called but not initialized!");
            return;
        }

        Debug.Log($"TogglePause called. Current isPaused: {isPaused}");

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        if (!isInitialized || isPaused) return;

        Debug.Log("Pausing game...");
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (menuManager.Instance != null)
            menuManager.Instance.IsGamePaused = true;

        // Set cursor for UI menu
        SetMenuCursorState();

        // Show pause panel only (controls panel stays hidden)
        ShowPauseMenu();
    }

    private void ResumeGame()
    {
        if (!isInitialized || !isPaused) return;

        Debug.Log("Resuming game...");
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (menuManager.Instance != null)
            menuManager.Instance.IsGamePaused = false;

        // Restore cursor to game mode
        SetGameCursorState();

        // Hide all panels
        HidePauseMenu();
        HideControlsPanel();
    }

    private void OnResumeClick()
    {
        Debug.Log("Resume button clicked");
        ResumeGame();
    }

    private void OnOpenControls()
    {
        Debug.Log("Opening Controls - hiding pause panel, showing controls panel");

        // Hide pause panel
        HidePauseMenu();

        // Show controls panel
        ShowControlsPanel();
    }

    private void OnCloseControls()
    {
        Debug.Log("Closing Controls - hiding controls panel, showing pause panel");

        // Hide controls panel
        HideControlsPanel();

        // Show pause panel
        ShowPauseMenu();
    }

    private void OnQuitClick()
    {
        Debug.Log("Quit button clicked - returning to main menu");

        // Reset game state
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Set cursor for main menu
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        isPaused = false;

        // Hide all panels before leaving
        HidePauseMenu();
        HideControlsPanel();

        if (menuManager.Instance != null)
        {
            menuManager.Instance.IsGamePaused = false;
            menuManager.Instance.ReturnToMainMenu();
        }
        else
        {
            Debug.LogWarning("menuManager not found, loading MainMenu directly");
            SceneManager.LoadScene("MainMenu");
        }
    }
    //shows the pause menue when called
    private void ShowPauseMenu()
    {
        if (_pausePanel != null)
        {
            _pausePanel.style.display = DisplayStyle.Flex;
            _pausePanel.pickingMode = PickingMode.Position;
            Debug.Log("Pause panel shown");
        }
        else
        {
            Debug.LogError("Cannot show pause panel - _pausePanel is null!");
        }
    }
    //hidding the pause menue when called
    private void HidePauseMenu()
    {
        if (_pausePanel != null)
        {
            _pausePanel.style.display = DisplayStyle.None;
            _pausePanel.pickingMode = PickingMode.Ignore;
            Debug.Log("Pause panel hidden");
        }
    }
    //when clicking the controls pannel int he pause menue
    private void ShowControlsPanel()
    {
        if (_controlsPanel != null)
        {
            _controlsPanel.style.display = DisplayStyle.Flex;
            _controlsPanel.pickingMode = PickingMode.Position;
            Debug.Log("Controls panel shown");
        }
        else
        {
            Debug.LogWarning("Cannot show controls panel - _controlsPanel is null!");
        }
    }
    //hidding the control panel when called
    private void HideControlsPanel()
    {
        if (_controlsPanel != null)
        {
            _controlsPanel.style.display = DisplayStyle.None;
            _controlsPanel.pickingMode = PickingMode.Ignore;
            Debug.Log("Controls panel hidden");
        }
    }
    //locking game cursor so nothignis clicked while in pause menue
    private void SetGameCursorState()
    {
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
    //Enables cursor in the pause menue
    private void SetMenuCursorState()
    {
        UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }
}