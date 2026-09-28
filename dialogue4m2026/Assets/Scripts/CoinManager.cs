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
        // 1. Se voltou ao Menu ou Boot, limpa a contagem
        if (scene.name == "Menu" || scene.name == "_Boot")
        {
            ResetCoinsForNewLevel();
        }
        // 2. Se entrou em qualquer cena de Gameplay
        else if (scene.name.StartsWith("Gameplay"))
        {
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

            // Lê a última cena registrada no Slot
            string savedScene = PlayerPrefs.GetString($"Slot{activeSlot}_Scene", "");

            // SE for um Novo Jogo (slot < 0) OU se a cena atual for DIFERENTE da cena salva no Slot
            if (activeSlot < 0 || (!string.IsNullOrEmpty(savedScene) && savedScene != scene.name))
            {
                ResetCoinsForNewLevel();

                // Atualiza o PlayerPrefs para a nova cena sem moedas antigas acumuladas
                PlayerPrefs.SetString($"Slot{activeSlot}_Scene", scene.name);
                SaveCheckpointCoins(activeSlot);

                Debug.Log($"[CoinManager] Nova Fase detectada ({scene.name}). Contador de moedas zerado com sucesso!");
            }
            else
            {
                // Se for a MESMA cena (ex: morreu e recarregou ou carregou o Save do Menu), lê as moedas do checkpoint
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

        Debug.Log($"[Moeda] Coletada: {coin.CoinID} | Total Atual: {currentCoins}");
    }

    /// <summary>
    /// Verifica se a moeda já foi pega nesta sessão OU se já estava salva em um checkpoint.
    /// </summary>
    public bool IsCoinCollected(string coinID)
    {
        if (string.IsNullOrEmpty(coinID)) return false;
        
        // Verifica tanto no checkpoint quanto nas moedas coletadas na sessão atual
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

        Debug.Log($"[CoinManager] Moedas salvas no Slot {slotIndex}! Total: {checkpointCoins}");
    }

    public void LoadCheckpointCoins(int slotIndex)
    {
        checkpointCoins = PlayerPrefs.GetInt($"Slot{slotIndex}_Coins", 0);
        
        // Mantém a maior contagem de moedas caso já tenha coletado mais na sessão
        if (currentCoins < checkpointCoins)
        {
            currentCoins = checkpointCoins;
        }

        string idsFormatted = PlayerPrefs.GetString($"Slot{slotIndex}_CoinIDs", "");
        if (!string.IsNullOrEmpty(idsFormatted))
        {
            string[] ids = idsFormatted.Split(',');
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    checkpointCoinIDs.Add(id);
                    collectedCoinIDs.Add(id); // Garante que a lista de coletadas na sessão mantenha as do checkpoint
                }
            }
        }

        Debug.Log($"[CoinManager] Moedas carregadas do Slot {slotIndex}: {currentCoins}");
    }

    public void ResetCoinsForNewLevel()
    {
        currentCoins = 0;
        checkpointCoins = 0;
        collectedCoinIDs.Clear();
        checkpointCoinIDs.Clear();
        Debug.Log("[CoinManager] Contador de moedas redefinido.");
    }
}