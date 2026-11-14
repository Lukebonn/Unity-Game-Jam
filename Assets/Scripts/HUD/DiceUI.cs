using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One d20 in the tray. It shuffles numbers while it rolls, then lands on one.
public class DiceUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private float rollTime = 1.2f;
    [SerializeField] private Color luckyColor = new Color(1f, 0.8f, 0.2f);

    // What the die is worth when you spend it (10x its number on a lucky TimesTen)
    public int Value { get; private set; }
    public bool HasLanded => Value > 0;

    public void Roll()
    {
        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        // Shuffle random faces while it rolls
        for (float t = 0f; t < rollTime; t += 0.08f)
        {
            valueText.text = Random.Range(1, LuckyNumber.Max + 1).ToString();
            yield return new WaitForSeconds(0.08f);
        }

        int rolled = Random.Range(1, LuckyNumber.Max + 1);

        LuckyEffect effect = LuckyEffect.None;
        if (rolled == LuckyNumber.Value)
        {
            effect = (LuckyEffect)Random.Range(1, 4);   // KillAll, TimesTen or Instakill
            image.color = luckyColor;
        }

        Value = effect == LuckyEffect.TimesTen ? rolled * 10 : rolled;
        valueText.text = Value.ToString();

        // The roll pop (sketch F) and the lucky effects listen for this
        SignalBus.InvokeDieLanded(rolled, effect);
    }
}
