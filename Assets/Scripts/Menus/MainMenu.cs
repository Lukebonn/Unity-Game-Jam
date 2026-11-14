using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text luckyValue;

    private void Start()
    {
        ShowLucky();
    }

    // Hooked up to the buttons' On Click () lists in the Inspector
    public void Play()
    {
        SceneManager.LoadScene(1);   // scene 1 in Build Profiles is the game
    }

    public void Quit()
    {
        Debug.Log("Quit only works in a build");
        Application.Quit();
    }

    public void LuckyUp()
    {
        LuckyNumber.Value += 1;   // the setter keeps it between 1 and 20
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
