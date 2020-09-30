using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectControlSystem : MonoBehaviour
{
    // Called by animation event once the animation is finished.
    public void OnAnimationDone()
    {
        Destroy(gameObject);
    }
}
