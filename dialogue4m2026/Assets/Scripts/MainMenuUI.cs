using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Botões dos Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backButton;

    private void Start()
    {
        // Garante que o painel principal esteja ativo e o de slots inativo
        ShowMainPanel();

        // Regra do enunciado: "Continuar" só aparece se o Slot 0 (Autosave) tiver dados
        CheckAutoSaveSlot();

        // Associa os cliques dos botões aos seus respectivos métodos
        SetupButtonListeners();
    }

    private void CheckAutoSaveSlot()
    {
        if (SaveSystem.Instance != null && continueButton != null)
        {
            bool hasAutoSave = SaveSystem.Instance.LoadDataInFile(0);
            continueButton.gameObject.SetActive(hasAutoSave);
        }
    }

    private void SetupButtonListeners()
    {
        // Menu Principal
        if (continueButton != null) continueButton.onClick.AddListener(OnClick_Continue);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClick_NewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(ShowSlotsPanel);
        if (quitButton != null) quitButton.onClick.AddListener(OnClick_Quit);

        // Tela de Slots
        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnClick_SelectSlot(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnClick_SelectSlot(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => OnClick_SelectSlot(3));
        if (backButton != null) backButton.onClick.AddListener(ShowMainPanel);
    }

    // --- AÇÕES DOS BOTÕES ---

    // Botão Continuar: Carrega o Slot 0 (Autosave)
    private void OnClick_Continue()
    {
        if (SaveSystem.Instance.LoadDataInFile(0))
        {
            LoadSavedPhase(0);
        }
    }

    // Botão Novo Jogo: Inicia uma partida limpa sempre do início da primeira fase
    private void OnClick_NewGame()
    {
        // Reseta os dados para o novo jogo
        SaveSystem.Instance.SetPlayerLevel(1, 0);
        GameManager.Instance.LoadGameScene("Gameplay");
    }

    // Seleção de Slot (1, 2 ou 3) na tela de Carregar Jogo
    private void OnClick_SelectSlot(int slotIndex)
    {
        if (SaveSystem.Instance.LoadDataInFile(slotIndex))
        {
            // Réplica o save carregado no Slot 0 (Regra do enunciado)
            SaveSystem.Instance.SaveDataInFile(0);
            LoadSavedPhase(slotIndex);
        }
        else
        {
            Debug.LogWarning($"O Slot {slotIndex} está vazio!");
        }
    }

    // Auxiliar para carregar a cena com base na fase gravada no save
    private void LoadSavedPhase(int slotIndex)
    {
        int phaseLevel = SaveSystem.Instance.GetPlayerLevel(slotIndex);
        string targetScene = (phaseLevel == 2) ? "Gameplay 2" : "Gameplay";

        GameManager.Instance.LoadGameScene(targetScene);
    }

    private void OnClick_Quit()
    {
        GameManager.Instance.QuitGame();
    }

    // --- NAVEGAÇÃO DE PAINÉIS ---

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (slotsPanel != null) slotsPanel.SetActive(false);
    }

    public void ShowSlotsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
    }
}