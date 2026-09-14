using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button playButton; // Botão "Jogar" ou "Carregar Jogo"
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button quitButton;

    [Header("Botões dos Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backButton;

    // Controla se o painel foi aberto para Iniciar Novo Jogo ou Carregar Slot
    private bool isNewGameSelection = false;

    private void Start()
    {
        Time.timeScale = 1f;
        ShowMainPanel();
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (playButton != null) playButton.onClick.AddListener(() => OpenSlotsPanel(false));
        if (newGameButton != null) newGameButton.onClick.AddListener(() => OpenSlotsPanel(true));
        if (quitButton != null) quitButton.onClick.AddListener(OnClick_Quit);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnSelectSlot(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnSelectSlot(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => OnSelectSlot(3));
        if (backButton != null) backButton.onClick.AddListener(ShowMainPanel);
    }

    private void OpenSlotsPanel(bool isNewGame)
    {
        isNewGameSelection = isNewGame;
        ShowSlotsPanel();
    }

    private void OnSelectSlot(int slotIndex)
    {
        // 1. Grava o Slot selecionado como o Slot ativo da sessão atual
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        if (isNewGameSelection)
        {
            StartNewGameOnSlot(slotIndex);
        }
        else
        {
            LoadGameFromSlot(slotIndex);
        }
    }

    private void StartNewGameOnSlot(int slotIndex)
    {
        Debug.Log($"[MainMenuUI] Criando NOVO JOGO no Slot {slotIndex}...");

        // Limpa os dados exclusivamente desse slot
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_HasCheckpoint");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosX");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosY");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosZ");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_Coins");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_CoinIDs");
        PlayerPrefs.SetInt($"Slot{slotIndex}_Level", 1);
        PlayerPrefs.Save();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetPlayerLevel(1, slotIndex);
            SaveSystem.Instance.SaveDataInFile(slotIndex);
        }

        // Novo jogo sempre inicia na Fase 1 ("Gameplay")
        LoadScene("Gameplay");
    }

    private void LoadGameFromSlot(int slotIndex)
    {
        bool hasSave = PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1 || 
                       PlayerPrefs.HasKey($"Slot{slotIndex}_Level");

        if (!hasSave)
        {
            Debug.LogWarning($"[MainMenuUI] Slot {slotIndex} está vazio! Criando novo jogo nele.");
            StartNewGameOnSlot(slotIndex);
            return;
        }

        // Lê a fase gravada especificamente nesse slot
        int savedLevel = PlayerPrefs.GetInt($"Slot{slotIndex}_Level", 1);
        string targetScene = (savedLevel == 2) ? "Gameplay 2" : "Gameplay";

        Debug.Log($"[MainMenuUI] Entrando no Slot {slotIndex} -> Fase: {savedLevel} ({targetScene})");

        LoadScene(targetScene);
    }

    private void LoadScene(string sceneName)
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

    private void OnClick_Quit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            Application.Quit();
        }
    }

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