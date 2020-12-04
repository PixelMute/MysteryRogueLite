using UnityEngine;

public class BloodSplatter : MonoBehaviour
{
    public ParticleSystem ParticleSystem { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Launches the particle effects based on where we are getting attacked from
    /// </summary>
    /// <param name="attackLocation">Location that we are getting attacked from</param>
    public void Play(Vector2Int attackLocation)
    {
        var angle = ((float)Mathf.Atan2(transform.position.z - attackLocation.y, transform.position.x - attackLocation.x) * 180 / Mathf.PI) % 360;
        Debug.Log($"Angle of attack: {angle}");
        var shape = ParticleSystem.shape;
        shape.rotation = new Vector3(0, -angle + 45, 0);
        ParticleSystem.Play();
    }
}
