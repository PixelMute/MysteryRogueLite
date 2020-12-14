using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomRunCanvas : MonoBehaviour
{
    [NonSerialized]
    public CustomRun CustomRun;
    public TMP_InputField FloorNumber;
    public TMP_InputField RoomNumber;
    public TMP_InputField EnemyNumber;
    public TMP_InputField PrizeCost;
    public TMP_InputField BossNumber;

    public void Awake()
    {
        CustomRun = new CustomRun();
    }

    public void ResetText()
    {
        CustomRun = new CustomRun();
        FloorNumber.text = CustomRun.NumberOfFloors.ToString();
        RoomNumber.text = CustomRun.RoomsPerFloor.ToString();
        EnemyNumber.text = CustomRun.NumEnemies.ToString();
        PrizeCost.text = CustomRun.PrizeCost.ToString();
        BossNumber.text = CustomRun.NumberOfBosses.ToString();
    }

    public void Submit()
    {
        CustomRun.SetBossFloors();
        CustomRun.instance = CustomRun;
        SaveGameSystem.instance = null;
        SceneManager.LoadScene(SceneConstants.PlayGame);
    }


    public void FloorChanged(string input)
    {
        if (input.Length > 0 && input[0] == '-')
        {
            input = input.Remove(0, 1);
        }
        if (int.TryParse(input, out int res))
        {
            if (res > 0)
            {
                CustomRun.NumberOfFloors = Math.Max(res, 3); //Make sure we always have atleast 3 rooms
                return;
            }
        }

        CustomRun.NumberOfFloors = 5;

    }

    public void RoomChanged(string input)
    {
        if (input.Length > 0 && input[0] == '-')
        {
            input = input.Remove(0, 1);
        }
        if (int.TryParse(input, out int res))
        {
            if (res > 0)
            {
                CustomRun.RoomsPerFloor = res;
                return;
            }
        }


        CustomRun.RoomsPerFloor = 6;

    }

    public void EnemiesChanged(string input)
    {
        if (input.Length > 0 && input[0] == '-')
        {
            input = input.Remove(0, 1);
        }
        if (int.TryParse(input, out int res))
        {
            if (res >= 0)
            {
                CustomRun.NumEnemies = res;
                return;
            }
        }

        CustomRun.NumEnemies = 4;

    }

    public void PriceRewardChanged(string input)
    {
        if (input.Length > 0 && input[0] == '-')
        {
            input = input.Remove(0, 1);
        }
        if (int.TryParse(input, out int res))
        {
            if (res > 0)
            {
                CustomRun.PrizeCost = res;
                return;
            }
        }

        CustomRun.PrizeCost = 30;

    }

    public void BossFloorsChanged(string input)
    {
        if (input.Length > 0 && input[0] == '-')
        {
            input = input.Remove(0, 1);
        }
        if (int.TryParse(input, out int res))
        {
            if (res > 0)
            {
                CustomRun.NumberOfBosses = res;
                return;
            }
        }

        CustomRun.NumberOfBosses = 1;

    }
}

