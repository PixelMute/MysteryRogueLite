using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject SmallChest;
    public GameObject MoneyPrefab;

    public static ItemSpawner Instance;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameObject SpawnSmallChest(Vector2Int spawnLoc)
    {
        return Instance.SpawnItem(Instance.SmallChest, spawnLoc);
    }

    public static GameObject SpawnMoney(Vector2Int spawnLoc)
    {
        return Instance.SpawnItem(Instance.MoneyPrefab, spawnLoc);
    }

    private GameObject SpawnItem(GameObject itemPrefab, Vector2Int spawnLoc)
    {
        return Instantiate(itemPrefab, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), itemPrefab.transform.rotation, transform);
    }
}
