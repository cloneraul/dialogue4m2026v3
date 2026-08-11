using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using File = System.IO.File;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveDatas = new List<SaveData>();
            saveDatas.Add(new SaveData());
            dataPath = Application.persistentDataPath+"save";
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private string dataPath;
    private List<SaveData> saveDatas;
    

    public void SetPlayerLevel(int level, int slot = 0)
    {
        saveDatas[slot].PlayerLevel = level;
    }

    public void SaveDataInFile(int slot = 0)
    {
        File.WriteAllText(dataPath+slot,saveDatas[slot].ToJson());
    }

    public bool LoadDataInFile(int slot = 0)
    {
        if (!File.Exists(dataPath + slot)) return false;
        return saveDatas[slot].FromJson(File.ReadAllText(dataPath + slot));
    }
    
    [Serializable]
    public class SaveData
    {
        private int playerLevel;
        public int PlayerLevel { 
            get => playerLevel; 
            set => playerLevel = value; }
        
        public SaveData(int playerLevel=1)
        {
            this.playerLevel = playerLevel;
        }
        
        public string ToJson(){
            return JsonUtility.ToJson(this);
        }

        public bool FromJson(string json)
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null) return false;
            playerLevel = saveData.playerLevel;
            return true;
        }
    }
    
    
    


}
