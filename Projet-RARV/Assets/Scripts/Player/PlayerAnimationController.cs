using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetMovement(float normalizedSpeed, bool isGrounded)
    {
        animator.SetFloat("Speed", normalizedSpeed);
        animator.SetBool("IsGrounded", isGrounded);
    }

    public void PlayJump()
    {
        animator.SetTrigger("Jump");
    }

    public void PlayRoll()
    {
        animator.SetTrigger("Roll");
    }
}