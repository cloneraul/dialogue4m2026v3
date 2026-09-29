using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins;
    private int checkpointCoins;
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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu" || scene.name == "_Boot")
        {
            ResetCoinsForNewLevel();
        }
        else if (scene.name.StartsWith("Gameplay"))
        {
            // Pega o slot ativo da sessão (Slot 0 é o temporário/autosave)
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

            // SE for Novo Jogo explícito (slot -1), limpa tudo
            if (activeSlot < 0)
            {
                ResetCoinsForNewLevel();
            }
            else
            {
                // Carrega as moedas e a lista de IDs do Slot ativo
                LoadCheckpointCoins(activeSlot);
            }
        }
    }

    public void CollectCoin(Coin coin)
    {
        if (coin == null) return;

        currentCoins += coin.Value;

        if (!string.IsNullOrEmpty(coin.CoinID))
        {
            collectedCoinIDs.Add(coin.CoinID);
        }

        Debug.Log($"[CoinManager] Moeda coletada: {coin.CoinID} | Total Atual: {currentCoins}");
    }

    public bool IsCoinCollected(string coinID)
    {
        if (string.IsNullOrEmpty(coinID)) return false;
        return checkpointCoinIDs.Contains(coinID) || collectedCoinIDs.Contains(coinID);
    }

    public void SaveCheckpointCoins(int slotIndex)
    {
        checkpointCoins = currentCoins;
        checkpointCoinIDs = new HashSet<string>(collectedCoinIDs);

        PlayerPrefs.SetInt($"Slot{slotIndex}_Coins", checkpointCoins);

        string idsFormatted = string.Join(",", checkpointCoinIDs);
        PlayerPrefs.SetString($"Slot{slotIndex}_CoinIDs", idsFormatted);
        PlayerPrefs.Save();

        Debug.Log($"[CoinManager] Moedas e IDs salvas no Slot {slotIndex}! Total: {checkpointCoins} | IDs: {idsFormatted}");
    }

    public void LoadCheckpointCoins(int slotIndex)
    {
        checkpointCoins = PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", 0);
        currentCoins = checkpointCoins;

        checkpointCoinIDs.Clear();
        collectedCoinIDs.Clear();

        string idsFormatted = PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", "");
        if (!string.IsNullOrEmpty(idsFormatted))
        {
            string[] ids = idsFormatted.Split(',');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    checkpointCoinIDs.Add(id);
                    collectedCoinIDs.Add(id);
                }
            }
        }

        Debug.Log($"[CoinManager] Carregado Slot {slotIndex}: {currentCoins} moedas | {checkpointCoinIDs.Count} moedas já coletadas.");
    }

    /// <summary>
    /// Alias para manter compatibilidade com chamadas de carregamento de slot
    /// </summary>
    public void LoadCoinsFromSlot(int slotIndex)
    {
        LoadCheckpointCoins(slotIndex);
    }

    public void ResetCoinsForNewLevel()
    {
        currentCoins = 0;
        checkpointCoins = 0;
        collectedCoinIDs.Clear();
        checkpointCoinIDs.Clear();
        Debug.Log("[CoinManager] Moedas resetadas.");
    }
}