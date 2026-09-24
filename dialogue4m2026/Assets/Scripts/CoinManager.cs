using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // Sempre que entra no Menu ou Boot, garante limpeza do estado temporário
        if (scene.name == "Menu" || scene.name == "_Boot")
        {
            ResetCoinsForNewLevel();
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

        Debug.Log($"[Moeda] Coletada: {coin.CoinID} | Total Atual: {currentCoins}");
    }

    /// <summary>
    /// Verifica se uma moeda já foi coletada no estado ativado/salvo no cenário.
    /// </summary>
    public bool IsCoinCollected(string coinID)
    {
        if (string.IsNullOrEmpty(coinID)) return false;
        // Verifica se o ID consta nas moedas já confirmadas pelo save
        return checkpointCoinIDs.Contains(coinID);
    }

    /// <summary>
    /// Confirma e grava o estado das moedas atuais permanentemente para um Slot específico.
    /// </summary>
    public void SaveCheckpointCoins(int slotIndex)
    {
        checkpointCoins = currentCoins;
        checkpointCoinIDs = new HashSet<string>(collectedCoinIDs);

        PlayerPrefs.SetInt($"Slot{slotIndex}_Coins", checkpointCoins);

        string idsFormatted = string.Join(",", checkpointCoinIDs);
        PlayerPrefs.SetString($"Slot{slotIndex}_CoinIDs", idsFormatted);
        PlayerPrefs.Save();

        Debug.Log($"[CoinManager] Moedas confirmadas e salvas no Slot {slotIndex}! Total: {checkpointCoins}");
    }

    /// <summary>
    /// Carrega o estado de moedas salvas de um Slot e descarta qualquer progresso temporário não salvo.
    /// </summary>
    public void LoadCheckpointCoins(int slotIndex)
    {
        checkpointCoins = PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", 0);
        currentCoins = checkpointCoins;

        string idsFormatted = PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", "");
        if (!string.IsNullOrEmpty(idsFormatted))
        {
            string[] ids = idsFormatted.Split(',');
            checkpointCoinIDs = new HashSet<string>(ids);
            // Restaura collectedCoinIDs apenas com o que estava salvo no disco (descartando moedas não salvas)
            collectedCoinIDs = new HashSet<string>(ids);
        }
        else
        {
            checkpointCoinIDs.Clear();
            collectedCoinIDs.Clear();
        }

        Debug.Log($"[CoinManager] Estado de moedas restaurante com sucesso do Slot {slotIndex}: {currentCoins} moedas.");
    }

    public int GetCheckpointCoins(int slotIndex)
    {
        return PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", 0);
    }

    public string GetCheckpointCoinIDs(int slotIndex)
    {
        return PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", "");
    }

    public void ResetCoinsForNewLevel()
    {
        currentCoins = 0;
        checkpointCoins = 0;
        collectedCoinIDs.Clear();
        checkpointCoinIDs.Clear();
        Debug.Log("[CoinManager] Contador de moedas totalmente redefinido.");
    }
}