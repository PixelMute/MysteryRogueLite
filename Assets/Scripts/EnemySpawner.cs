﻿using UnityEngine;

class EnemySpawner : MonoBehaviour
{
    public GameObject BasicMeleeEnemy;
    public GameObject Archer;
    public GameObject Brute;

    public static EnemySpawner Instance;

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

    public static GameObject SpawnBrute(Vector2Int spawnLoc)
    {
        return Instance.SpawnEnemy(Instance.Brute, spawnLoc);
    }

    public static GameObject SpawnBasicMeleeEnemy(Vector2Int spawnLoc)
    {
        return Instance.SpawnEnemy(Instance.BasicMeleeEnemy, spawnLoc);
    }

    public static GameObject SpawnArcher(Vector2Int spawnLoc)
    {
        return Instance.SpawnEnemy(Instance.Archer, spawnLoc);
    }

    private GameObject SpawnEnemy(GameObject enemyPrefab, Vector2Int spawnLoc)
    {
        return Instantiate(enemyPrefab, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), Quaternion.identity, transform);
    }
}
