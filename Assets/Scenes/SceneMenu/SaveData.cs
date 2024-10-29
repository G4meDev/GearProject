using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveData : MonoBehaviour
{
    public string PlayerName;

    private static string dataDirPath = Application.persistentDataPath;
    private static string dataFileName = "SaveData.sav";

    public SaveData()
    {
        PlayerName = "New Player";
    }

    public void Save()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        string dataToStore = JsonUtility.ToJson(this, false);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
    }

    public static SaveData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        string dataToLoad = "";

        SaveData loadedData;

        if (File.Exists(fullPath))
        {
            using (FileStream stream = new FileStream(fullPath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    dataToLoad = reader.ReadToEnd();
                }
            }

            loadedData = JsonUtility.FromJson<SaveData>(dataToLoad);
        }

        else
        {
            loadedData = new SaveData();
        }

        //validate

        return loadedData;
    }
}
