using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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
    [SerializeField] private List<SaveData> saveDatas;
    

    public void SetPlayerLevel(int level, int slot = 0)
    {
        saveDatas[slot].playerLevel = level;
    }

    public void SetPlayerName(string playerName, int slot = 0)
    {
        saveDatas[slot].playerName = playerName;
    }

    public void SaveDataInFile(int slot = 0)
    {
        File.WriteAllText(dataPath+slot,Encryptor.Encrypt(saveDatas[slot].ToJson()));
    }

    public bool LoadDataInFile(int slot = 0)
    {
        if (!File.Exists(dataPath + slot)) return false;
        saveDatas[slot].FromJson(Encryptor.Decrypted(File.ReadAllText(dataPath + slot)));
        return true;
    }
    
    [Serializable]
    public class SaveData
    {
        public int playerLevel;
        public string playerName;
        
        public SaveData(int playerLevel=1, string playerName="")
        {
            this.playerLevel = playerLevel;
            this.playerName = playerName;
        }
        
        public string ToJson(){
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
    
    private class Encryptor
    {
        public static string IV = "1a1a1a1a1a1a1a1a";
        public static string Key = "1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a13";

        public static string Encrypt(string decrypted)
        {
            byte[] textbytes = ASCIIEncoding.ASCII.GetBytes(decrypted);
            AesCryptoServiceProvider endec = new AesCryptoServiceProvider();
            endec.BlockSize = 128;
            endec.KeySize = 256;
            endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            endec.Padding = PaddingMode.PKCS7;
            endec.Mode = CipherMode.CBC;
            ICryptoTransform icrypt = endec.CreateEncryptor(endec.Key, endec.IV);
            byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
            icrypt.Dispose();
            return Convert.ToBase64String(enc);
        }

        public static string Decrypted(string encrypted)
        {
            byte[] textbytes = Convert.FromBase64String(encrypted);
            AesCryptoServiceProvider endec = new AesCryptoServiceProvider();
            endec.BlockSize = 128;
            endec.KeySize = 256;
            endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            endec.Padding = PaddingMode.PKCS7;
            endec.Mode = CipherMode.CBC;
            ICryptoTransform icrypt = endec.CreateDecryptor(endec.Key, endec.IV);
            byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
            icrypt.Dispose();
            return System.Text.ASCIIEncoding.ASCII.GetString(enc);
        }
    }
    


}
