using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Paineis do Menu")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadSlotsButton;
    [SerializeField] private Button quitButton;

    [Header("Botões de Slot")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backToMainButton;

    private void Start()
    {
        // Garante a visibilidade correta dos painéis ao abrir o Menu
        if (mainPanel != null) mainPanel.SetActive(true);
        if (slotsPanel != null) slotsPanel.SetActive(false);

        // Desativa o botão "Continuar" se não houver um Autosave (Slot 0) recente
        if (continueButton != null)
        {
            bool hasAutosave = PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1;
            continueButton.interactable = hasAutosave;
        }

        // Garante que o cursor do rato fica visível e desbloqueado no Menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ==========================================
    // MÉTODOS DO MENU PRINCIPAL
    // ==========================================

    /// <summary>
    /// Chamado pelo botão "Novo Jogo". Limpa a sessão temporária e inicia a Fase 1.
    /// </summary>
    public void OnNewGameClicked()
    {
        // Define o Slot 0 como ativo (Temporário)
        PlayerPrefs.SetInt("CurrentActiveSlot", 0);
        
        // Limpa dados salvos anteriores do Slot 0 para começar do zero
        PlayerPrefs.SetInt("Slot0_HasCheckpoint", 0);
        PlayerPrefs.SetInt("Slot0_Level", 1);
        PlayerPrefs.SetString("Slot0_Scene", "Gameplay");
        PlayerPrefs.SetInt("HasPendingSave", 0);
        
        PlayerPrefs.DeleteKey("Slot0_Coins");
        PlayerPrefs.DeleteKey("Slot0_CoinIDs");
        PlayerPrefs.Save();

        // Reseta o gerenciador de moedas na memória
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        LoadTargetScene("Gameplay");
    }

    /// <summary>
    /// Chamado pelo botão "Continuar". Carregará o progresso mais recente no Slot 0.
    /// </summary>
    public void OnContinueClicked()
    {
        if (PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1)
        {
            PlayerPrefs.SetInt("CurrentActiveSlot", 0);
            PlayerPrefs.SetInt("HasPendingSave", 0);
            PlayerPrefs.Save();

            string savedScene = PlayerPrefs.GetString("Slot0_Scene", "Gameplay");
            LoadTargetScene(savedScene);
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] Nenhum arquivo de Autosave encontrado no Slot 0!");
        }
    }

    /// <summary>
    /// Alterna a exibição para a tela de Seleção de Slots.
    /// </summary>
    public void OnOpenSlotsPanelClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
    }

    /// <summary>
    /// Retorna da tela de Slots para o Menu Principal.
    /// </summary>
    public void OnBackToMainMenuClicked()
    {
        if (slotsPanel != null) slotsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    /// <summary>
    /// Encerra a execução do jogo.
    /// </summary>
    public void OnQuitGameClicked()
    {
        Debug.Log("[MainMenuUI] Fechando o jogo...");
        Application.Quit();
    }

    // ==========================================
    // MÉTODOS DE SELEÇÃO DE SLOTS (1, 2 ou 3)
    // ==========================================

    /// <summary>
    /// Chamado ao selecionar um slot de salvamento no painel de Slots.
    /// </summary>
    /// <param name="slotIndex">Índice do Slot (1, 2 ou 3)</param>
    public void LoadSlot(int slotIndex)
    {
        bool hasSaveInSlot = PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1;

        if (hasSaveInSlot)
        {
            // 1. Define este slot como o ativo para a nova sessão
            PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);

            // 2. Bloqueia novos salvamentos manuais até alcançar um SaveTrigger na nova cena
            PlayerPrefs.SetInt("HasPendingSave", 0);
            PlayerPrefs.Save();

            // 3. Lê o nome da cena salva (ou busca pelo nível numérico)
            string savedScene = PlayerPrefs.GetString($"Slot{slotIndex}_Scene", "");
            if (string.IsNullOrEmpty(savedScene))
            {
                int levelNum = PlayerPrefs.GetInt($"Slot{slotIndex}_Level", 1);
                savedScene = (levelNum == 2) ? "Gameplay 2" : "Gameplay";
            }

            Debug.Log($"[MainMenuUI] Carregando Slot {slotIndex} na cena '{savedScene}'...");
            LoadTargetScene(savedScene);
        }
        else
        {
            Debug.LogWarning($"[MainMenuUI] O Slot {slotIndex} está vazio! Inicie um Novo Jogo ou salve o progresso durante a partida.");
        }
    }

    // Metodos diretos para vinculação nos OnClick do Inspetor (caso prefira não passar parâmetros por código)
    public void LoadSlot1() => LoadSlot(1);
    public void LoadSlot2() => LoadSlot(2);
    public void LoadSlot3() => LoadSlot(3);

    // ==========================================
    // AUXILIARES
    // ==========================================

    private void LoadTargetScene(string sceneName)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}