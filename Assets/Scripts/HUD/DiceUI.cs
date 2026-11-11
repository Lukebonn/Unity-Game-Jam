using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DiceUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] faces;
    [SerializeField] private int rollTime;

    public int Value {  get; private set; }
    public bool HasLanded => Value > 0;

    public void Roll()
    {
        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        animator.enabled = true;
        yield return new WaitForSeconds(rollTime);
        animator.enabled = false;

        Value = Random.Range(1, 7);
        image.sprite = faces[Value];
    }
}
