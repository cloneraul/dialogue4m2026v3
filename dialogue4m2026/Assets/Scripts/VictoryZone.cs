using System.Collections;
using UnityEngine;

public class VictoryZone : MonoBehaviour
{
    [Header("Configurações da Fase")]
    [Tooltip("Marque se esta for a última fase do jogo (Gameplay 2)")]
    [SerializeField] private bool isFinalLevel;

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

        // Busca todas as moedas da cena sem warnings de API obsoleta
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

        // 2. Exibe a interface com a nova instrução
        if (victoryPanel != null) victoryPanel.SetActive(true);
        
        if (victoryCoinsText != null)
        {
            victoryCoinsText.text = $"Moedas Coletadas: {currentCoins} / {totalCoinsInScene}";
        }
        
        if (instructionText != null)
        {
            instructionText.text = isFinalLevel 
                ? "Parabéns! Você concluiu o jogo!" 
                : "Pressione E no objeto verde para avançar para a fase 2";
        }

        // 3. Salva o progresso e autosave no Slot 0
        SaveAutosaveOnVictory();

        // 4. RESETA AS MOEDAS para a nova fase
        if (!isFinalLevel && CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 5. Inicia o temporizador para fechar o painel automaticamente após 5 segundos
        StartCoroutine(HidePanelAfterDelay());

        Debug.Log($"[VictoryZone] Vitória registrada! Moedas: {currentCoins}/{totalCoinsInScene}. Autosave e Reset de moedas efetuados.");
    }

    private IEnumerator HidePanelAfterDelay()
    {
        // Aguarda os 5 segundos com o jogo rodando normalmente
        yield return new WaitForSeconds(displayDuration);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void SaveAutosaveOnVictory()
    {
        int levelToSave = isFinalLevel ? 2 : 1;

        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SetPlayerLevel(levelToSave);
                SaveSystem.Instance.SaveDataInFile();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Gravação no autosave: {e.Message}");
            }
        }

        PlayerPrefs.SetInt("Slot0_Level", levelToSave);
        PlayerPrefs.Save();
    }
}