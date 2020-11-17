using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator Animator;

    public void StartWalkingAnimation()
    {
        Animator.Play("PlayerWalking");
    }

    public void StartIdleAnimation()
    {
        Animator.Play("PlayerIdle");
    }
}
