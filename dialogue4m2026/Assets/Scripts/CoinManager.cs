using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins = 0;
    private int checkpointCoins = 0;
    private HashSet<string> collectedCoinIDs = new HashSet<string>();
    private HashSet<string> checkpointCoinIDs = new HashSet<string>();

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Pega o slot ativo da sessão atual (1, 2, 3 ou 0 para Novo Jogo)
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        LoadCheckpointCoins(activeSlot);
    }

    public void CollectCoin(Coin coin)
    {
        if (coin == null) return;

        currentCoins += coin.Value;

        if (!string.IsNullOrEmpty(coin.CoinID))
        {
            collectedCoinIDs.Add(coin.CoinID);
        }

        Debug.Log($"[Moeda] Coletada: {coin.CoinID} | Total Atual: {currentCoins}");
    }

    public bool IsCoinCollected(string coinID)
    {
        if (string.IsNullOrEmpty(coinID)) return false;
        return checkpointCoinIDs.Contains(coinID);
    }

    public void SaveCheckpointCoins(int slotIndex)
    {
        checkpointCoins = currentCoins;
        checkpointCoinIDs = new HashSet<string>(collectedCoinIDs);

        PlayerPrefs.SetInt($"Slot{slotIndex}_Coins", checkpointCoins);

        string idsFormatted = string.Join(",", checkpointCoinIDs);
        PlayerPrefs.SetString($"Slot{slotIndex}_CoinIDs", idsFormatted);
        PlayerPrefs.Save();

        Debug.Log($"[CoinManager] Moedas salvas no Slot {slotIndex}! Total: {checkpointCoins}");
    }

    public void LoadCheckpointCoins(int slotIndex)
    {
        checkpointCoins = PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", 0);
        currentCoins = checkpointCoins;

        string idsFormatted = PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", "");
        if (!string.IsNullOrEmpty(idsFormatted))
        {
            string[] ids = idsFormatted.Split(',');
            checkpointCoinIDs = new HashSet<string>(ids);
            collectedCoinIDs = new HashSet<string>(ids);
        }
        else
        {
            checkpointCoinIDs.Clear();
            collectedCoinIDs.Clear();
        }

        Debug.Log($"[CoinManager] Moedas carregadas do Slot {slotIndex}: {currentCoins}");
    }

    // --- MÉTODOS ADICIONADOS PARA INTEGRAÇÃO COM SAVESYSTEM E PAUSECONTROLLER ---

    /// <summary>
    /// Retorna a quantidade de moedas salvas para um slot específico.
    /// </summary>
    public int GetCheckpointCoins(int slotIndex)
    {
        return PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", currentCoins);
    }

    /// <summary>
    /// Retorna a lista de IDs de moedas salvas para um slot específico.
    /// </summary>
    public string GetCheckpointCoinIDs(int slotIndex)
    {
        return PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", string.Join(",", collectedCoinIDs));
    }

    public void ResetCoinsForNewLevel()
    {
        currentCoins = 0;
        checkpointCoins = 0;
        collectedCoinIDs.Clear();
        checkpointCoinIDs.Clear();
        Debug.Log("[CoinManager] Contador de moedas resetado.");
    }
}