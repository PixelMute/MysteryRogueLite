using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFollowPath : MonoBehaviour
{
    public string pathName;
    [SerializeField] private float unitsPerSecond = 0; // Time it takes
    [SerializeField] public Vector3[] waypoints;
    [SerializeField] private bool skipLoop = false;
    public iTween.EaseType easeType = iTween.EaseType.linear;
    private int currentTarget = 0;
    private ParticleSystem particles;
    private ParticleSystem.ShapeModule shape;
    private ParticleSystem.EmissionModule em;

    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        shape = particles.shape;
        em = particles.emission;
        DisableParticlePath();
    }


    void MoveToNextTarget()
    {
        currentTarget++;
        if (currentTarget == waypoints.Length)
        {
            if (skipLoop)
            {
                currentTarget = 1;
                shape.position = waypoints[0];
            }
            else
                currentTarget = 0;
        }
            

        float distance = Vector3.Distance(shape.position, waypoints[currentTarget]);
        float time = distance * unitsPerSecond; // U * (s/U)
        //iTween.MoveTo(gameObject, iTween.Hash("position", waypoints[currentTarget], "easetype", easeType, "time", time, "oncomplete", "MoveToNextTarget", "islocal", true));
        iTween.ValueTo(gameObject, iTween.Hash("from", shape.position, "to", waypoints[currentTarget], "speed", unitsPerSecond, "easetype", easeType, "oncomplete", "MoveToNextTarget", "onupdate", "UpdateShapePosition"));
        //DOTween.To(() =>shape.position, x=> shape.position = x, waypoints[currentTarget], time);
    }

    void UpdateShapePosition(Vector3 input)
    {
        shape.position = input;
    }

    // Disables the emission and resets position.
    public void DisableParticlePath()
    {
        em.enabled = false;
        currentTarget = 0;
        UpdateShapePosition(waypoints[0]);
        iTween.Stop(gameObject);
    }

    public void StartMovement()
    {
        //Debug.Log("Particle path starting");
        em.enabled = true;
        MoveToNextTarget();

        for (int i = 0; i < waypoints.Length; i++)
        {
            //Debug.Log("Waypoint " + i + ": " + waypoints[i]);
        }
    }
}
