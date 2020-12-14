using System.Collections.Generic;

public class CustomRun
{
    public static CustomRun instance;

    public int NumberOfFloors = 5;
    public int NumEnemies = 4;
    public int PrizeCost = 30;
    public int RoomsPerFloor = 6;
    public int NumberOfBosses = 1;

    private List<int> bossFloors;

    public CustomRun()
    {
        SetBossFloors();
    }

    public void SetBossFloors()
    {
        bossFloors = GetBossFloors();

    }

    public bool IsBossFloor(int floorIndex)
    {
        return bossFloors.Contains(floorIndex);
    }

    public int GetNumberOfEnemies(int floorIndex)
    {
        return NumEnemies + floorIndex;
    }

    public bool IsLastFloor(int floorIndex)
    {
        return floorIndex + 1 == NumberOfFloors;
    }

    public void Remove()
    {
        instance = new CustomRun();
    }

    private List<int> GetBossFloors()
    {
        //Last floor should always be a boss
        var floors = NumberOfFloors - 1;
        var bosses = NumberOfBosses - 1;
        var res = new List<int>();
        while (bosses > 0 && res.Count != floors)
        {
            var randomInt = SeededRandom.Range(0, floors);
            if (!res.Contains(randomInt))
            {
                res.Add(randomInt);
                bosses--;
            }
        }

        res.Add(NumberOfFloors - 1);
        return res;
    }
}

