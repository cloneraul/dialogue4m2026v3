using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins = 0; // Moedas coletadas na corrida atual
    private int checkpointCoins = 0; // Moedas confirmadas no último checkpoint
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
    }

    private void Start()
    {
        // Carrega as moedas salvas no Slot 0 ao iniciar
        LoadCheckpointCoins(0);
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
        // Confere se a moeda já foi confirmada no checkpoint carregado
        return checkpointCoinIDs.Contains(coinID);
    }

    /// <summary>
    /// Salva o estado atual das moedas no Slot especificado quando o jogador toca em um Checkpoint ou Totem.
    /// </summary>
    public void SaveCheckpointCoins(int slotIndex)
    {
        checkpointCoins = currentCoins;
        checkpointCoinIDs = new HashSet<string>(collectedCoinIDs);

        PlayerPrefs.SetInt($"Slot{slotIndex}_Coins", checkpointCoins);

        // Converte o HashSet de IDs em string separada por vírgula
        string idsFormatted = string.Join(",", checkpointCoinIDs);
        PlayerPrefs.SetString($"Slot{slotIndex}_CoinIDs", idsFormatted);
        PlayerPrefs.Save();

        Debug.Log($"[CoinManager] Moedas salvas no Slot {slotIndex}! Total: {checkpointCoins}");
    }

    /// <summary>
    /// Carrega as moedas registradas no Slot especificado.
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
            collectedCoinIDs = new HashSet<string>(ids);
        }
        else
        {
            checkpointCoinIDs.Clear();
            collectedCoinIDs.Clear();
        }

        Debug.Log($"[CoinManager] Moedas carregadas do Slot {slotIndex}: {currentCoins}");
    }

    /// <summary>
    /// Zera o contador de moedas ao iniciar uma nova fase.
    /// </summary>
    public void ResetCoinsForNewLevel()
    {
        currentCoins = 0;
        checkpointCoins = 0;
        collectedCoinIDs.Clear();
        checkpointCoinIDs.Clear();
        Debug.Log("[CoinManager] Contador de moedas resetado para nova fase.");
    }
}