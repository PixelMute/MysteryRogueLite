using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float TimeToMove = 1f;

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
    public void MoveToPosition(Vector2Int destination, float timeToMove)
    {
        MoveToPosition(BattleManager.ConvertVector(destination, transform.position.y), timeToMove);
    }

    /// <summary>
    /// Move the game object to the given destination
    /// </summary>
    /// <param name="destination">Destination we are moving to</param>
    /// <param name="timeToMove">How long it takes to move in seconds</param>
    protected void MoveToPosition(Vector3 destination, float timeToMove)
    {
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
            transform.position = Vector3.Lerp(currentPos, destination, t);
            yield return null;
        }
        //Make sure to finish the movement
        transform.position = Vector3.Lerp(currentPos, destination, 1);
        yield return null;
    }
}