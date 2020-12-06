using System.Collections;
using UnityEngine;

class BossLaserAttack : Attack
{
    public int MinDamage;
    public int MaxDamage;
    public GameObject Laser;
    public int Range = 4;

    public override void ActivateAttack(Vector2Int target)
    {
        IsAttackDone = false;
        StartCoroutine(FireLaser(target));
    }

    public override bool IsTargetInRange(Vector2Int target)
    {
        return IsTargetInRange(target, Range);
    }

    IEnumerator FireLaser(Vector2Int target)
    {
        yield return new WaitForSeconds(.45f);
        var laser = Instantiate(Laser).GetComponent<Laser>();
        laser.SetLocation(transform.position, BattleManager.player.transform.position);
        StartCoroutine(laser.ParticleEffect(BattleManager.player.transform.position));
        yield return new WaitForSeconds(2f);
        Destroy(laser.gameObject);
        IsAttackDone = true;
        AttackTarget(target);
    }

    private void AttackTarget(Vector2Int target)
    {
        var random = new System.Random();
        var damage = random.Next(MinDamage, MaxDamage + 1);
        BattleGrid.instance.StrikeTile(target, BattleManager.ConvertVector(transform.position), damage);
    }
}

