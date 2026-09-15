using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button playButton; // Botão "Carregar Jogo"
    [SerializeField] private Button newGameButton; // Botão "Novo Jogo"
    [SerializeField] private Button quitButton;

    [Header("Botões dos Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backButton;

    private void Start()
    {
        Time.timeScale = 1f;
        ShowMainPanel();
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        // Limpa ouvintes anteriores para evitar chamadas duplicadas
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(ShowSlotsPanel);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveAllListeners();
            newGameButton.onClick.AddListener(OnClick_StartNewGameDirectly);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnClick_Quit);
        }

        if (slot1Button != null)
        {
            slot1Button.onClick.RemoveAllListeners();
            slot1Button.onClick.AddListener(() => OnSelectSlotToLoad(1));
        }

        if (slot2Button != null)
        {
            slot2Button.onClick.RemoveAllListeners();
            slot2Button.onClick.AddListener(() => OnSelectSlotToLoad(2));
        }

        if (slot3Button != null)
        {
            slot3Button.onClick.RemoveAllListeners();
            slot3Button.onClick.AddListener(() => OnSelectSlotToLoad(3));
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ShowMainPanel);
        }
    }

    /// <summary>
    /// Inicia Novo Jogo diretamente na Fase 1 sem vincular a um slot de arquivo.
    /// </summary>
    public void OnClick_StartNewGameDirectly()
    {
        Debug.Log("[MainMenuUI] Iniciando Novo Jogo na Fase 1...");

        PlayerPrefs.SetInt("CurrentActiveSlot", 0);
        PlayerPrefs.Save();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        LoadScene("Gameplay");
    }

    /// <summary>
    /// Seleciona o Slot para carregar. Se o slot estiver vazio, nada acontece.
    /// </summary>
    public void OnSelectSlotToLoad(int slotIndex)
    {
        // Verifica se há dados salvos para o slot escolhido
        bool hasSave = PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1 || 
                       PlayerPrefs.HasKey($"Slot{slotIndex}_Level");

        if (!hasSave)
        {
            Debug.LogWarning($"[MainMenuUI] Slot {slotIndex} está vazio! Nenhuma ação realizada.");
            return; // Bloqueia o carregamento e mantém no painel de slots
        }

        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        int savedLevel = PlayerPrefs.GetInt($"Slot{slotIndex}_Level", 1);
        string targetScene = (savedLevel == 2) ? "Gameplay 2" : "Gameplay";

        Debug.Log($"[MainMenuUI] Carregando Slot {slotIndex} -> Fase: {savedLevel} ({targetScene})");
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

    public void OnClick_Quit()
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