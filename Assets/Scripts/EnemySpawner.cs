using UnityEngine;

class EnemySpawner : MonoBehaviour
{
    public GameObject BasicMeleeEnemy;
    public GameObject Archer;
    public GameObject Brute;
    public GameObject Boss;
    public GameObject Minion;
    public GameObject Spikes;


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

    public static GameObject SpawnMinion(Vector2Int spawnLoc)
    {
        return Instance.SpawnEnemy(Instance.Minion, spawnLoc);
    }

    public static GameObject SpawnBoss(Vector2Int spawnLoc)
    {
        return Instance.SpawnEnemy(Instance.Boss, spawnLoc);
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

    public static GameObject SpawnSpikes(Vector2Int spawnLoc)
    {
        var spikes = Instance.SpawnEnemy(Instance.Spikes, spawnLoc);
        spikes.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        return spikes;
    }

    private GameObject SpawnEnemy(GameObject enemyPrefab, Vector2Int spawnLoc)
    {
        return Instantiate(enemyPrefab, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), Quaternion.identity, transform);
    }
}

