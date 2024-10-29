using System;
using System.IO;
using UnityEngine;

public static class SaveDataDefaults
{
    public static string defaultPlayerName = "New Player";

//    public static string dataDirPath = Application.persistentDataPath;
    public static string dataFileName = "SaveData.sav";
}

[Serializable]
public class SaveData
{
    public string playerName;

    public SaveData()
    {
        playerName = SaveDataDefaults.defaultPlayerName;
    }

    public void Save()
    {
        string dataDirPath = Application.persistentDataPath;
        string fullPath = Path.Combine(dataDirPath, SaveDataDefaults.dataFileName);
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
        string dataDirPath = Application.persistentDataPath;
        string fullPath = Path.Combine(dataDirPath, SaveDataDefaults.dataFileName);
        string dataToLoad = "";

        SaveData loadedData = null;

        if (File.Exists(fullPath))
        {
            try
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

            catch (Exception e) 
            {
                Debug.Log(e);
            }
        }

        if (loadedData == null)
        {
            loadedData = new SaveData();
        }

        loadedData.Validate();

        return loadedData;
    }

    public void Validate()
    {
        if (playerName.Length < 3 || playerName.Length > 8)
        {
            playerName = SaveDataDefaults.defaultPlayerName;
        }
    }

    public void SetPlayerName(string name)
    {
        if (name.Length < 3)
        {
            return;
        }

        else if (name.Length > 8)
        {
            playerName = name.Substring(0, 8);
            return;
        }

        else
        {
            playerName = name;
            return;
        }
    }
}
