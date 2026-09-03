using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins = 0;
    private HashSet<string> collectedCoinIDs = new HashSet<string>();

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Carrega o progresso das moedas ao iniciar a cena
        LoadCoinData();
    }

    public void CollectCoin(Coin coin)
    {
        if (coin == null)
            return;

        currentCoins += coin.Value;

        // Adiciona o ID na lista de coletadas
        if (!string.IsNullOrEmpty(coin.CoinID))
        {
            collectedCoinIDs.Add(coin.CoinID);
        }

        Debug.Log("Moeda coletada! Total: " + currentCoins);

        // Salva o progresso atualizado no Slot 0 (Autosave)
        SaveCoinData();
    }

    public bool IsCoinCollected(string coinID)
    {
        if (string.IsNullOrEmpty(coinID)) return false;
        return collectedCoinIDs.Contains(coinID);
    }

    public void ResetCoins()
    {
        currentCoins = 0;
        collectedCoinIDs.Clear();
        Debug.Log("Moedas resetadas.");
    }

    // --- MÉTODOS DE SAVE E LOAD DAS MOEDAS ---

    private void SaveCoinData()
    {
        string json = PlayerPrefs.GetString("SaveSlot_0", "");
        SaveData data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json);

        data.totalCoins = currentCoins;
        data.collectedCoinIDs = new List<string>(collectedCoinIDs);

        string updatedJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("SaveSlot_0", updatedJson);
        PlayerPrefs.Save();
    }

    private void LoadCoinData()
    {
        string json = PlayerPrefs.GetString("SaveSlot_0", "");
        if (!string.IsNullOrEmpty(json))
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            currentCoins = data.totalCoins;
            collectedCoinIDs = new HashSet<string>(data.collectedCoinIDs);
        }
    }
}