using UnityEngine;

public class UnrolledDiceUI : MonoBehaviour
{
    [SerializeField] private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Roll()
    {
        animator.SetTrigger("rolling");
    }
}
