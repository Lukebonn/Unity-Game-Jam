using System.Collections;
using UnityEngine;
using TMPro;

// Sketch spot F. When a die lands, numbers shuffle BG3-style, then the real one pops.
// Between rolls it shows your lucky number, dimmed.
public class RollPopHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text number;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float shuffleTime = 0.35f;
    [SerializeField] private float holdTime = 1f;
    [SerializeField] private float idleAlpha = 0.35f;
    [SerializeField] private Color rollColor = Color.white;
    [SerializeField] private Color luckyColor = new Color(1f, 0.8f, 0.2f);

    private Coroutine popping;

    private void OnEnable() => SignalBus.OnDieLanded += HandleDieLanded;
    private void OnDisable() => SignalBus.OnDieLanded -= HandleDieLanded;

    private void Start()
    {
        ShowIdle();
    }

    private void HandleDieLanded(int value, LuckyEffect effect)
    {
        // A newer die interrupts the pop that's still playing
        if (popping != null) StopCoroutine(popping);
        popping = StartCoroutine(Pop(value, effect != LuckyEffect.None));
    }

    private IEnumerator Pop(int value, bool lucky)
    {
        group.alpha = 1f;
        transform.localScale = Vector3.one;
        number.color = lucky ? luckyColor : rollColor;

        // Shuffle random numbers before landing on the real one
        for (float t = 0f; t < shuffleTime; t += 0.05f)
        {
            number.text = Random.Range(1, LuckyNumber.Max + 1).ToString();
            yield return new WaitForSeconds(0.05f);
        }
        number.text = value.ToString();

        // Punch: start big and shrink back to normal
        float punch = lucky ? 2f : 1.5f;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(punch, 1f, t);
            yield return null;
        }
        transform.localScale = Vector3.one;

        yield return new WaitForSeconds(holdTime);
        ShowIdle();
        popping = null;
    }

    private void ShowIdle()
    {
        group.alpha = idleAlpha;
        number.color = luckyColor;
        number.text = LuckyNumber.Value.ToString();
    }
}
