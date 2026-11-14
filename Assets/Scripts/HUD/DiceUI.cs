using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// One d6 in the tray. It plays the rolling animation, then lands on a face.
public class DiceUI : MonoBehaviour
{
    [SerializeField] private Animator animator;   // plays the 2D_d6_temp roll
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] faces;      // faces[0] shows 1 ... faces[5] shows 6
    [SerializeField] private float rollTime = 2f;
    [SerializeField] private Color luckyColor = new Color(1f, 0.8f, 0.2f);

    // What the die is worth when you spend it (10x its number on a lucky TimesTen)
    public int Value { get; private set; }
    public bool HasLanded => Value > 0;
    public LuckyEffect Effect { get; private set; }

    public void Roll()
    {
        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        animator.enabled = true;
        yield return new WaitForSeconds(rollTime);

        // An enabled Animator sets the sprite every frame, so switch it off before showing the face
        animator.enabled = false;

        int rolled = Random.Range(1, LuckyNumber.Max + 1);   // 1 to 6
        image.sprite = faces[rolled - 1];

        if (rolled == LuckyNumber.Value)
        {
            Effect = (LuckyEffect)Random.Range(1, 4);   // KillAll, TimesTen or Instakill
            image.color = luckyColor;
        }

        Value = Effect == LuckyEffect.TimesTen ? rolled * 10 : rolled;

        // The lucky effects listen for this
        SignalBus.InvokeDieLanded(rolled, Effect);
    }
}
