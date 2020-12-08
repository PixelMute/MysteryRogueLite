using System;

[Serializable]
public class PlayerData
{
    SerializableVector3 Position;
    SerializableQuaternion Rotation;
    float CurrentSpirit;
    int MaxSpirit;
    int SpiritCostPerDiscard;
    int Health;
    int MaxHealth;
    int Money;
    int CardRedrawAmount;
    int MaxHandSize;
    int MaxTurnsUntilDraw;
    int TurnsUntilDraw;
    int CurrentEnergy;
    int EnergyPerTurn;
    int xPos;
    int zPos;

    public static PlayerData SavePlayer(PlayerController player)
    {
        return new PlayerData()
        {
            Position = player.transform.position,
            CurrentSpirit = player.CurrentSpirit,
            MaxSpirit = player.maxSpirit,
            SpiritCostPerDiscard = player.spiritCostPerDiscard,
            Health = player.Health,
            MaxHealth = player.maxHealth,
            Money = player.Money,
            CardRedrawAmount = player.cardRedrawAmount,
            MaxHandSize = player.maxHandSize,
            MaxTurnsUntilDraw = player.maxTurnsUntilDraw,
            TurnsUntilDraw = player.turnsUntilDraw,
            CurrentEnergy = player.CurrentEnergy,
            EnergyPerTurn = player.energyPerTurn,
            xPos = player.xPos,
            zPos = player.zPos
        };
    }

    public static void LoadPlayer(PlayerData data, PlayerController player)
    {
        player.transform.position = data.Position;
        player.CurrentSpirit = data.CurrentSpirit;
        player.maxSpirit = data.MaxSpirit;
        player.spiritCostPerDiscard = data.SpiritCostPerDiscard;
        player.Health = data.Health;
        player.maxHealth = data.MaxHealth;
        player.Money = data.Money;
        player.cardRedrawAmount = data.CardRedrawAmount;
        player.maxHandSize = data.MaxHandSize;
        player.maxTurnsUntilDraw = data.MaxTurnsUntilDraw;
        player.turnsUntilDraw = data.TurnsUntilDraw;
        player.CurrentEnergy = data.CurrentEnergy;
        player.energyPerTurn = data.EnergyPerTurn;
        player.xPos = data.xPos;
        player.zPos = data.zPos;
        player.UpdateLOS();
    }
}

