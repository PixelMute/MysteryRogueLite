using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A subclass of TileEntity that represents living, moving entities on the battlegrid.
public abstract class TileCreature : TileEntity
{
    public int visionRange = 5;
    protected Dictionary<BattleManager.StatusEffectEnum, StatusEffectDataHolder> statusEffects = null; // Dictionary that stores what status effects are currently on this entity.

    public abstract override float GetPathfindingCost();
    public abstract override bool GetPlayerWalkable();
    public abstract override void TakeDamage(int amount);

    public abstract int GetStatusEffectValue(BattleManager.StatusEffectEnum status);

    public abstract void ApplyStatusEffect(BattleManager.StatusEffectEnum status, int amount);

    public float TimeToMove = .25f;

    public bool IsMoving { get; private set; } = false;

    /// <summary>
    /// Move the game object to the given x and z coordinates staying at the same y value
    /// </summary>
    /// <param name="destination">Destination we are moving to</param>
    public void MoveToPosition(Vector2Int destination)
    {
        MoveToPosition(destination, TimeToMove);
    }

    /// <summary>
    /// Move the game object to the given x and z coordinates staying at the same y value
    /// </summary>
    /// <param name="destination">Destination we are moving to</param>
    /// <param name="timeToMove">How long it takes to move in seconds</param>
    public virtual void MoveToPosition(Vector2Int destination, float timeToMove)
    {
        BattleGrid.instance.MoveObjectTo(destination, this);
        MoveToPosition(BattleManager.ConvertVector(destination, transform.position.y), timeToMove);
    }

    /// <summary>
    /// Move the game object to the given destination
    /// </summary>
    /// <param name="destination">Destination we are moving to</param>
    /// <param name="timeToMove">How long it takes to move in seconds</param>
    private void MoveToPosition(Vector3 destination, float timeToMove)
    {
        IsMoving = true;
        StartCoroutine(MoveToPositionCoRoutine(destination, timeToMove));
    }

    /// <summary>
    /// Lerp towards target every frame
    /// </summary>
    /// <param name="destination"></param>
    /// <param name="timeToMove"></param>
    /// <returns></returns>
    private IEnumerator MoveToPositionCoRoutine(Vector3 destination, float timeToMove)
    {
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;
            if (t >= 1)
            {
                IsMoving = false;
                t = 1;
                OnStopMoving();
            }
            transform.position = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }
    }

    /// <summary>
    /// Function that gets called when movement is over
    /// </summary>
    protected virtual void OnStopMoving()
    {
        return;
    }

}
