using System;
using static BasicEnemyAI;

[Serializable]
class EnemyData
{
    SerializableVector2Int location;
    int health;
    int maxHealth;
    EnemyBody.EnemyType type;
    EnemyAIState state;
    bool engaged;
    SerializableVector2Int wonderTarget;

    public static EnemyData SaveEnemy(EnemyBody body)
    {
        var data = new EnemyData()
        {
            location = new SerializableVector2Int(body.xPos, body.zPos),
            health = body.Health.CurrentHealth,
            maxHealth = body.Health.MaxHealth,
            type = body.Type
        };
        var ai = body.AI as BasicEnemyAI;
        if (ai != null)
        {
            data.state = ai.currentAIState;
            data.engaged = ai.engagedTarget != null;
            data.wonderTarget = ai.wanderTarget;
        }
        return data;
    }

    public static EnemyBody LoadEnemy(EnemyData data)
    {
        EnemyBody body;
        switch (data.type)
        {
            case EnemyBody.EnemyType.melee:
                body = EnemySpawner.SpawnBasicMeleeEnemy(data.location).GetComponent<EnemyBody>();
                break;
            case EnemyBody.EnemyType.archer:
                body = EnemySpawner.SpawnArcher(data.location).GetComponent<EnemyBody>();
                break;
            case EnemyBody.EnemyType.brute:
                body = EnemySpawner.SpawnBrute(data.location).GetComponent<EnemyBody>();
                break;
            case EnemyBody.EnemyType.minion:
                body = EnemySpawner.SpawnMinion(data.location).GetComponent<EnemyBody>();
                break;
            default:
                body = EnemySpawner.SpawnBoss(data.location).GetComponent<EnemyBody>();
                break;
        }

        body.Health.CurrentHealth = data.health;
        body.Health.MaxHealth = data.maxHealth;
        body.EnemyUI.UpdateHealthBar(body.Health);
        BattleGrid.instance.CurrentFloor.PlaceObjectOn(data.location.x, data.location.y, body);
        var ai = body.AI as BasicEnemyAI;
        if (ai != null)
        {
            ai.engagedTarget = data.engaged ? BattleManager.player : null;
            ai.wanderTarget = data.wonderTarget;
            ai.currentAIState = data.state;
        }
        return body;
    }

}

