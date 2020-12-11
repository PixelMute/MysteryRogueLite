using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

class SaveGameSystem
{
    private static GameMetaData[] MetaDatas;
    public static SaveGameSystem instance;
    public Game Game;
    public GameMetaData MetaData;

    public SaveGameSystem(GameMetaData data)
    {
        MetaData = data;
    }

    private async Task<Game> LoadGameFromFileAsync()
    {
        var path = GetSavePath(MetaData.GameSlot);
        return await Task.Run(() => LoadGameFromFile(path));
    }

    private Game LoadGameFromFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    var bf = new BinaryFormatter();
                    return bf.Deserialize(fileStream) as Game;
                }
            }
        }
        catch (Exception e)
        {
            if (MetaData != null)
            {
                Debug.LogError($"Couldn't load game from file for slot: {MetaData.GameSlot}");
            }
        }
        return null;
    }

    public static IEnumerator StartGameScene(int index)
    {
        if (MetaDatas[index] == null)
        {
            MetaDatas[index] = new GameMetaData()
            {
                GameSlot = index,
                LastPlayed = DateTime.Now,
                FloorNumber = 1
            };
        }
        instance = new SaveGameSystem(MetaDatas[index]);
        var game = instance.LoadGameFromFileAsync();
        var async = SceneManager.LoadSceneAsync(SceneConstants.PlayGame);
        async.allowSceneActivation = false;

        while (!async.isDone && !game.IsCompleted)
        {
            yield return null;
        }
        instance.Game = game.Result;
        async.allowSceneActivation = true;
    }

    public SaveGameSystem()
    {
        LoadMetaDatasFromFile();
    }

    public bool HasGameToLoad()
    {
        return Game != null;
    }

    public void LoadGame()
    {
        Game.LoadGame();
    }

    public void SaveGame()
    {
        var game = Game.SaveGame();
        MetaData.FloorNumber = BattleGrid.instance.CurrentFloor.FloorNumber + 1;
        MetaData.LastPlayed = DateTime.Now;
        using (var fileStream = new FileStream(GetSavePath(MetaData.GameSlot), FileMode.OpenOrCreate))
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, game);
        }
        SaveMetaDatasToFile();
        Game = game;
    }

    public void DeleteGame()
    {
        try
        {
            var path = GetSavePath(MetaData.GameSlot);
            File.Delete(path);
            MetaDatas[MetaData.GameSlot] = null;
            SaveMetaDatasToFile();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete game");
        }

    }

    private static string GetSavePath(int index)
    {
        return Path.Combine(Application.persistentDataPath, $"{index}.save");
    }

    private static string GetMetaDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "SaveGames.json");
    }

    private static void LoadMetaDatasFromFile()
    {
        if (MetaDatas == null)
        {
            MetaDatas = new GameMetaData[6];
            try
            {
                var path = GetMetaDataPath();
                var text = File.ReadAllText(GetMetaDataPath());
                MetaDatas = JsonConvert.DeserializeObject<GameMetaData[]>(text);
            }
            catch (Exception)
            {
                Debug.LogError("Failed to open meta data file");
                SaveMetaDatasToFile();
            }
        }
    }

    private static void SaveMetaDatasToFile(bool tryAgain = true)
    {
        try
        {
            var contents = JsonConvert.SerializeObject(MetaDatas);
            File.WriteAllText(GetMetaDataPath(), contents);
        }
        catch (Exception e)
        {
            if (tryAgain)
            {
                //Try to save the meta datas again
                SaveMetaDatasToFile(false);
            }
            else
            {
                Debug.LogError("Failed to save game meta data");
            }
        }
    }

    public GameMetaData GetMetaData(int index, bool regGame = true)
    {
        if (index < 0 || index > 2)
        {
            throw new ArgumentException($"Index out of range for meta data. Index: {index}");
        }
        if (regGame)
        {
            return MetaDatas[index];
        }
        else
        {
            return MetaDatas[index + 3];
        }
    }

    public void DeleteMetaData(int index, bool regGame = true)
    {
        if (index < 0 || index > 2)
        {
            throw new ArgumentException($"Index out of range for meta data. Index: {index}");
        }
        if (regGame)
        {
            MetaDatas[index] = null;
        }
        else
        {
            MetaDatas[index + 3] = null;
        }
        SaveMetaDatasToFile();
    }

    public static void ExitGame()
    {
        instance?.SaveGame();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


}

