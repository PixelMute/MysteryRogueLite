using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    private Animation animator;
    public GameObject ParticleSystem;

    public void Start()
    {
        animator = GetComponent<Animation>();
    }
    public void SetLocation(Vector3 bossLoc, Vector3 targetLoc)
    {
        transform.position = new Vector3(bossLoc.x + 3.1f, 0, bossLoc.z + .6f);
        var firePoint = GetFirePoint();
        var angle = Helpers.GetAngleBetween(firePoint, targetLoc);
        transform.RotateAround(firePoint, Vector3.up, 180 - angle);
    }


    public IEnumerator ParticleEffect(Vector3 targetLoc)
    {
        yield return new WaitForSeconds(.8f);
        var obj = Instantiate(ParticleSystem, transform);
        obj.transform.position = targetLoc;
        obj.GetComponent<ParticleSystem>().Play();
    }

    private Vector3 GetFirePoint()
    {
        var xOffset = (-150f + 52f) / (32f);
        var yOffset = (-50f + 66.5f) / (32f);
        return new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z + yOffset);
    }
}
