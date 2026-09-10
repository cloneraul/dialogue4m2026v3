using System.Collections;
using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    [Header("UI de Vitória")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMPro.TextMeshProUGUI victoryCoinsText;
    [SerializeField] private TMPro.TextMeshProUGUI instructionText;

    [Header("Tempo de Exibição")]
    [Tooltip("Tempo em segundos que o painel de vitória fica visível na tela")]
    [SerializeField] private float displayDuration = 5f;

    private bool hasWon;
    private int totalCoinsInScene;

    private void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Busca todas as moedas ativas na cena
        totalCoinsInScene = FindObjectsByType<Coin>(FindObjectsInactive.Exclude).Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;
            ExecuteVictoryLogic();
        }
    }

    private void ExecuteVictoryLogic()
    {
        // 1. Coleta moedas atuais do CoinManager
        int currentCoins = (CoinManager.Instance != null) ? CoinManager.Instance.CurrentCoins : 0;

        // 2. Exibe a interface com a instrução para o objeto verde
        if (victoryPanel != null) victoryPanel.SetActive(true);
        
        if (victoryCoinsText != null)
        {
            victoryCoinsText.text = $"Moedas Coletadas: {currentCoins} / {totalCoinsInScene}";
        }
        
        if (instructionText != null)
        {
            instructionText.text = "Pressione E no objeto verde para prosseguir";
        }

        // 3. Salva o Autosave no Slot 0 para manter o estado da vitória
        SaveAutosaveOnVictory();

        // 4. Inicia o temporizador para fechar o painel após 5 segundos
        StartCoroutine(HidePanelAfterDelay());

        Debug.Log($"[VictoryZone] Vitória registrada! Moedas: {currentCoins}/{totalCoinsInScene}. Autosave efetuado.");
    }

    private IEnumerator HidePanelAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void SaveAutosaveOnVictory()
    {
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SaveDataInFile(0);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Gravação no autosave: {e.Message}");
            }
        }

        PlayerPrefs.Save();
    }
}