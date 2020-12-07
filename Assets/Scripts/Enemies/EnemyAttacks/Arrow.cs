using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float DistanceToStop = .01f;
    public float Speed = .5f;
    public delegate void Effect(Vector2Int target);

    public void ShootAtTarget(Vector2Int target, Effect effect)
    {
        var angle = GetAngleToTarget(target);
        AudioManager.PlayGun();
        transform.rotation = Quaternion.Euler(new Vector3(90, -angle + 90, 0));
        StartCoroutine(GoTowardsTargetAndActivateEffect(target, effect));
    }

    private IEnumerator GoTowardsTargetAndActivateEffect(Vector2Int target, Effect effect)
    {
        while (Vector2.Distance(target, BattleManager.ConvertVector(transform.position)) > DistanceToStop)
        {
            transform.position = Vector3.MoveTowards(transform.position, BattleManager.ConvertVector(target, transform.position.y), Speed);
            yield return null;
        }
        //Once we are close enough, activate the effect and destroy object
        effect(target);
        Destroy(gameObject);
    }

    public float GetAngleToTarget(Vector2Int target)
    {
        var angle = ((float)Mathf.Atan2(transform.position.z - target.y, transform.position.x - target.x) * 180 / Mathf.PI) % 360;
        return angle < 0 ? angle + 360 : angle;
    }
}
