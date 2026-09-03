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
        // Exibe o painel principal
        ShowMainPanel();

        // Regra: "Continuar" só aparece se o Slot 0 (Autosave) tiver arquivo salvo
        CheckAutoSaveSlot();

        // Associa os cliques dos botões aos métodos
        SetupButtonListeners();
    }

    private void CheckAutoSaveSlot()
    {
        if (SaveSystem.Instance != null && continueButton != null)
        {
            // Tenta carregar o arquivo do Slot 0 (Autosave)
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

    // Botão Continuar: Carrega o Slot 0
    private void OnClick_Continue()
    {
        if (SaveSystem.Instance.LoadDataInFile(0))
        {
            LoadSavedPhase(0);
        }
    }

    // Botão Novo Jogo: Cria progresso limpo no Slot 0 e carrega a fase 1
    private void OnClick_NewGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetPlayerLevel(1, 0);
            SaveSystem.Instance.SaveDataInFile(0);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene("Gameplay");
        }
    }

    // Seleção de Slot (1, 2 ou 3) na tela de Carregar Jogo
    private void OnClick_SelectSlot(int slotIndex)
    {
        if (SaveSystem.Instance.LoadDataInFile(slotIndex))
        {
            // Copia o nível do slot escolhido para o Slot 0 (Autosave ativo)
            int level = SaveSystem.Instance.GetPlayerLevel(slotIndex);
            SaveSystem.Instance.SetPlayerLevel(level, 0);
            SaveSystem.Instance.SaveDataInFile(0);

            LoadSavedPhase(0);
        }
        else
        {
            Debug.LogWarning($"O Slot {slotIndex} está vazio!");
        }
    }

    // Carrega a cena baseada no playerLevel salvo no SaveSystem
    private void LoadSavedPhase(int slotIndex)
    {
        int phaseLevel = SaveSystem.Instance.GetPlayerLevel(slotIndex);
        string targetScene = (phaseLevel == 2) ? "Gameplay 2" : "Gameplay";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetScene);
        }
    }

    private void OnClick_Quit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
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