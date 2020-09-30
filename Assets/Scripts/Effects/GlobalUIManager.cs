using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages UI things that aren't directly player related.
public class GlobalUIManager : MonoBehaviour
{
    public static GlobalUIManager instance;

    // Particles
    public GameObject effectParticlePrefab;
    private Animator effectParticleAnimator;
    public enum ParticleType { sword };
    public GameObject particleHolder;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (particleHolder == null)
            particleHolder = GameObject.Find("ParticleHolder");
    }

    // Spawns a requested particle at position
    public void SpawnParticleAt(ParticleType type, Vector3 position)
    {
        Debug.Log("Making particle");
        GameObject particle = ReadyParticle(position);
        switch(type)
        {
            case ParticleType.sword:
                effectParticleAnimator.SetTrigger("SwordTrigger");
                break;
            default:
                Debug.LogError("ParticleEffectSpawner::SpawnParticleAt(" + type.ToString() + ", " + position.ToString() + ") -- Unknown particle type.");
                Destroy(particle);
                break;
        }
    }

    // Spawns in a new particle.
    private GameObject ReadyParticle(Vector3 position)
    {
        GameObject go = Instantiate(effectParticlePrefab, position, Quaternion.AngleAxis(90, Vector3.right), particleHolder.transform);
        effectParticleAnimator = go.GetComponent<Animator>();
        return go;
    }
}
