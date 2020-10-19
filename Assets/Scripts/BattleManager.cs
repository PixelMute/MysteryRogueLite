using DG.Tweening.Plugins;
using NesScripts.Controls.PathFind;
using Roguelike;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Global level manager. Should be attached to an empty object or the camera.
// The all seeing camera shall judge your sins.
public class BattleManager : MonoBehaviour
{
    public static BattleManager instance; // Singleton

    public static PlayerController player;
    public static Camera mainCamera;
    [HideInInspector] public enum TurnPhase { player, enemy, waiting }; // Whose turn it currently is
    public static TurnPhase currentTurn = TurnPhase.player;

    public enum StatusEffectEnum { defence, momentum, insight };
    // This should be set as an array of sprites that corresponds to each enum in order
    [SerializeField] public Sprite[] statusEffectReference;

    [HideInInspector] public BattleGrid map;

    [HideInInspector] public static CardStackTracker cardResolveStack = new CardStackTracker();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            try
            {
                CardFactory.LoadCards();
            }
            catch (Exception e)
            {
                //Handle exception if we can't load cards
            }
        }
        else if (instance != this)
        {
            // Sodoku
            Destroy(gameObject);
        }

        player = GameObject.Find("PlayerHolder").GetComponent<PlayerController>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        CreateFloor();
    }

    public void CreateFloor()
    {
        map = FindObjectOfType<BattleGrid>();

        map.GenerateFloor(60, 60);
    }

    // Called when the player ends their turn
    public void EndOfTurn()
    {
        currentTurn = TurnPhase.enemy;
        map.ProcessEnemyTurn();
        currentTurn = TurnPhase.player;
    }

    public Tile GetTileAtLocation(int xPos, int zPos)
    {
        return map.map[xPos, zPos];
    }

    // ANDs the values of two bool arrays together.
    // Returns an array the size of the smaller array.
    public static bool[,] ANDArray(bool[,] array1, bool[,] array2)
    {
        // First, find which array is the larger, and which is the smaller.

        int smallerX;
        int smallerY;
        int largerX;
        int largerY;

        int array1X = array1.GetLength(0);
        int array1Y = array1.GetLength(1);
        int array2X = array2.GetLength(0);
        int array2Y = array2.GetLength(1);

        int xOffset1 = 0;
        int yOffset1 = 0;
        int xOffset2 = 0;
        int yOffset2 = 0;

        if (array1X < array2X)
        {
            smallerX = array1X;
            largerX = array2X;
            // Apply offset to array 2's
            xOffset2 = (int)((largerX - smallerX) / 2);
        }
        else
        {
            smallerX = array2X;
            largerX = array1X;
            xOffset1 = (int)((largerX - smallerX) / 2);
        }

        if (array1Y < array2Y)
        {
            smallerY = array1Y;
            largerY = array2Y;
            yOffset2 = (int)((largerY - smallerY) / 2);
        }
        else
        {
            smallerY = array2Y;
            largerY = array1Y;
            yOffset1 = (int)((largerY - smallerY) / 2);
        }

        // Want to AND with the smaller array inset in the larger one.
        bool[,] array = new bool[smallerX, smallerY];

        for (int i = 0; i < smallerX; i++)
        {
            for (int j = 0; j < smallerY; j++)
            {
                array[i, j] = array1[i + xOffset1, j + yOffset1] && array2[i + xOffset2, j + yOffset2];
            }
        }
        return array;
    }

    // Destroys this gameobject and all its children recursively
    public static void RecursivelyEliminateObject(Transform toEliminate)
    {
        int childCount = toEliminate.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RecursivelyEliminateObject(toEliminate.GetChild(i));
        }

        Destroy(toEliminate.gameObject);
    }

    // Returns true if the given vector is all non-negative.
    public static bool IsVectorNonNeg(Vector2Int input)
    {
        return (input.x >= 0 && input.y >= 0);
    }
    // Doesn't bother to check Y
    public static bool IsVectorNonNeg(Vector3 input)
    {
        return (input.x >= 0 && input.z >= 0);
    }

    // Converts a vector3 in unity space to the corresponding tile.
    public static Vector2Int ConvertVector(Vector3 input)
    {
        return new Vector2Int((int)input.x, (int)input.z);
    }

    // Converts a tile location to its corresponding location in unity space.
    public static Vector3 ConvertVector(Vector2Int input, float y)
    {
        return new Vector3(input.x, y, input.y);
    }

    public static Vector2Int ConvertPoint(Point input)
    {
        return new Vector2Int(input.x, input.y);
    }

    // Give this method an array of the names of objects you want to search and which types it should look for.
    // Searches all children too.
    // Only one looked for component per object.
    public static Component[] RecursiveVariableAssign(GameObject obj, string[] gameObjNames, Type[] lookedForTypes, bool searchMultipleTimesPerObj = false)
    {
        if (gameObjNames.Length != lookedForTypes.Length)
            throw new ArgumentException("BattleManager--RecursiveVariableAssign()::The two provided arrays do not match in length.");

        Component[] returnVal = null; // This is only non-null if we find at least one thing.

        for (int i = 0; i < gameObjNames.Length; i++)
        {
            if (obj.name.Equals(gameObjNames[i]))
            {
                var component = obj.GetComponent(lookedForTypes[i]);
                if (component == null)
                {
                    // While this matches the name, it doesn't have the correct component
                }
                else
                {
                    //Debug.Log("Currently on " + obj.name + ", found " + lookedForTypes[i].ToString()) ;
                    if (returnVal == null)
                        returnVal = new Component[lookedForTypes.Length];

                    if (returnVal[i] == null)
                        returnVal[i] = component;
                    if (!searchMultipleTimesPerObj)
                        break;
                }
            }
        }

        // Now call on all children
        foreach (Transform x in obj.transform)
        {
            Component[] childVal = RecursiveVariableAssign(x.gameObject, gameObjNames, lookedForTypes, searchMultipleTimesPerObj);
            returnVal = MergeArrays(returnVal, childVal);
        }

        return returnVal;
    }

    // a1 will override values in a2
    public static Component[] MergeArrays(Component[] a1, Component[] a2)
    {
        if (a2 == null)
            return a1;
        else if (a1 == null)
            return a2;

        for (int i = 0; i < a2.Length; i++)
        {
            if (a1[i] == null)
                a1[i] = a2[i];
        }

        return a1;
    }

    // Enables and disables tracking of things that happen when a card is resolved.
    public void StartCardTracking(Card input)
    {
        cardResolveStack.ResetTracker();
        cardResolveStack.AddCardToTracker(input);
    }

    public void StopCardTracking()
    {
        cardResolveStack.ResetTracker();
    }
}
