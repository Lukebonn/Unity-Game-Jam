using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text luckyValue;
    [SerializeField] private GameObject[] panels;   // Upgrades, Controls and Credits: Esc closes them

    private void Start()
    {
        ShowLucky();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject panel in panels) panel.SetActive(false);
        }
    }

    // Hooked up to the buttons' On Click () lists in the Inspector.
    // The Play button opens the Upgrades panel; its Start button calls this.
    public void Play()
    {
        SceneManager.LoadScene(1);   // scene 1 in Build Profiles is the game
    }

    public void LuckyUp()
    {
        LuckyNumber.Value += 1;   // the setter keeps it between 1 and 6
        ShowLucky();
    }

    public void LuckyDown()
    {
        LuckyNumber.Value -= 1;
        ShowLucky();
    }

    private void ShowLucky()
    {
        luckyValue.text = LuckyNumber.Value.ToString();
    }
}
