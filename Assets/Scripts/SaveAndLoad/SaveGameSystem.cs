using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

class SaveGameSystem : MonoBehaviour
{
    public void SaveGame()
    {
        var game = Game.SaveGame();
        Debug.Log(GetSavePath("0"));
        using (var fileStream = new FileStream(GetSavePath("0"), FileMode.Create))
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, game);
        }
    }

    private static string GetSavePath(string index)
    {
        return Path.Combine(Application.persistentDataPath, $"{index}.save");

    }

}

