﻿using System.Collections;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Animator Animator { get; private set; }
    public EnemyBody EnemyBody { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public bool IsFacingRight { get; private set; } = true;
    public BloodSplatter Splatter;
    // Start is called before the first frame update
    void Start()
    {
        Animator = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>();
        EnemyBody = transform.parent.GetComponent<EnemyBody>();
    }

    public bool IsIdle()
    {
        return IsPlayingClip("Idle");
    }

    public bool IsMoving()
    {

        return IsPlayingClip("Moving");

    }

    protected bool IsPlayingClip(string clipName)
    {
        var clipInfo = Animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo?.Length > 0)
        {
            return clipInfo[0].clip?.name == clipName;
        }
        return false;
    }

    public virtual void Idle()
    {
        Animator.Play("Idle");
    }

    public virtual void Move()
    {
        Animator.Play("Moving");
    }

    public virtual void Die()
    {
        Animator.Play("Die");
    }

    public virtual void Attack()
    {
        switch(EnemyBody.Type)
        {
            case EnemyBody.EnemyType.melee:
                AudioManager.PlayRatAttack();
                break;
            case EnemyBody.EnemyType.brute:
                AudioManager.PlayFoxAttack();
                break;
            case EnemyBody.EnemyType.minion:
                AudioManager.PlaySkeletonAttack();
                break;
            default:
                break;
        }
        Animator.Play("Attack");
    }

    public void FlipDirection()
    {
        if (IsFacingRight)
        {
            Sprite.flipX = true;
        }
        else
        {
            Sprite.flipX = false;
        }
        IsFacingRight = !IsFacingRight;
    }

    public void TurnLeft()
    {
        if (IsFacingRight)
        {
            Sprite.flipX = true;
            IsFacingRight = false;
        }
    }

    public void TurnRight()
    {
        if (!IsFacingRight)
        {
            Sprite.flipX = false;
            IsFacingRight = true;
        }
    }

    public void GetHit(Vector2Int locationOfAttack)
    {
        Splatter.Play(locationOfAttack);
        StartCoroutine(HitColoration());
    }

    private IEnumerator HitColoration(float timeToWait = .05f)
    {
        Sprite.material.shader = BattleManager.instance.ShaderGUItext;
        Sprite.color = Color.white;
        yield return new WaitForSeconds(timeToWait);
        Sprite.material.shader = BattleManager.instance.ShaderSpritesDefault;
        Sprite.color = Color.white;
    }
}

