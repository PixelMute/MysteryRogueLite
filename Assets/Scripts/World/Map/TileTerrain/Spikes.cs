using System.Collections;
using UnityEngine;

class Spikes : Trap
{
    private Animator Animation;

    public void Awake()
    {
        Animation = GetComponent<Animator>();
    }

    public override void Activate()
    {
        if (Invisible)
        {
            Sprite.enabled = true;
            Invisible = false;
        }
        IsActive = true;
        StartCoroutine(ActivateTrap());
    }

    IEnumerator ActivateTrap()
    {
        Animation.Play("Spikes");
        yield return new WaitForSeconds(.4f);
        base.Activate();
        IsActive = false;
    }
}

