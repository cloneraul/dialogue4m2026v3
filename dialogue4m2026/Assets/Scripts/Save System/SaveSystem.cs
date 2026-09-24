using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    private string dataPath;

    // Lista que armazena os dados dos 4 slots em memória (0, 1, 2 e 3)
    [SerializeField] private List<SaveData> saveDatas = new List<SaveData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Caminho base para salvar os arquivos com / para não grudar no nome da pasta
            dataPath = Path.Combine(Application.persistentDataPath, "save_");

            // Inicializa a lista com 4 Slots em memória (Slot 0, 1, 2 e 3)
            saveDatas = new List<SaveData>();
            for (int i = 0; i < 4; i++)
            {
                saveDatas.Add(new SaveData());
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- MÉTODOS DE ACESSO AO SAVE EM MEMÓRIA ---

    public SaveData GetSaveData(int slot = 0)
    {
        if (slot < 0 || slot >= saveDatas.Count) return null;
        return saveDatas[slot];
    }

    public void SetSaveData(SaveData data, int slot = 0)
    {
        if (slot < 0 || slot >= saveDatas.Count) return;
        saveDatas[slot] = data;
    }

    // --- SALVAMENTO E CARREGAMENTO EM ARQUIVO (CRIPTOGRAFADO) ---

    /// <summary>
    /// Salva os dados do slot especificado no disco e replica no Slot 0 (Autosave).
    /// </summary>
    public void SaveDataInFile(int slot = 0)
    {
        if (slot < 0 || slot >= saveDatas.Count) return;

        string path = dataPath + slot + ".dat";
        string json = saveDatas[slot].ToJson();
        string encryptedData = Encryptor.Encrypt(json);

        File.WriteAllText(path, encryptedData);
        Debug.Log($"[SaveSystem] Slot {slot} salvo com sucesso em: {path}");

        // Regra do trabalho: Se salvou em um slot manual (1, 2 ou 3), replica ao mesmo tempo no Slot 0
        if (slot != 0)
        {
            saveDatas[0] = saveDatas[slot];
            string autoSavePath = dataPath + "0.dat";
            File.WriteAllText(autoSavePath, encryptedData);
            Debug.Log("[SaveSystem] Progresso replicado automaticamente no Slot 0 (Autosave).");
        }
    }

    /// <summary>
    /// Carrega os dados do arquivo para a memória e replica no Slot 0 caso seja um slot manual.
    /// </summary>
    public bool LoadDataInFile(int slot = 0)
    {
        string path = dataPath + slot + ".dat";

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] Arquivo do Slot {slot} não existe em: {path}");
            return false;
        }

        try
        {
            string encryptedData = File.ReadAllText(path);
            string json = Encryptor.Decrypted(encryptedData);

            if (saveDatas[slot] == null) saveDatas[slot] = new SaveData();
            saveDatas[slot].FromJson(json);

            Debug.Log($"[SaveSystem] Slot {slot} carregado do disco com sucesso!");

            // Regra do trabalho: Ao carregar um slot manual, copia esse estado para o Slot 0
            if (slot != 0)
            {
                saveDatas[0] = saveDatas[slot];
                SaveDataInFile(0);
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Erro ao carregar/decriptar o Slot {slot}: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifica se existe arquivo de save criado para determinado slot
    /// </summary>
    public bool HasSaveFile(int slot)
    {
        string path = dataPath + slot + ".dat";
        return File.Exists(path);
    }

    // --- ENCRIPTADOR AES MANTIDO INTEGRALMENTE ---

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