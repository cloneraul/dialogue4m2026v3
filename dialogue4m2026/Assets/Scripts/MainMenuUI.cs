using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button playButton; // Botão "Carregar Jogo" (Abre os slots)
    [SerializeField] private Button newGameButton; // Botão "Novo Jogo" (Inicia direto)
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
        if (playButton != null) playButton.onClick.AddListener(ShowSlotsPanel);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClick_StartNewGameDirectly);
        if (quitButton != null) quitButton.onClick.AddListener(OnClick_Quit);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnSelectSlotToLoad(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnSelectSlotToLoad(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => OnSelectSlotToLoad(3));
        if (backButton != null) backButton.onClick.AddListener(ShowMainPanel);
    }

    /// <summary>
    /// Inicia uma nova partida do zero na Fase 1 sem vincular a um slot ainda.
    /// </summary>
    private void OnClick_StartNewGameDirectly()
    {
        Debug.Log("[MainMenuUI] Iniciando Novo Jogo diretamente na Fase 1...");

        // Define o slot ativo como 0 (sessão temporária sem slot fixo definido)
        PlayerPrefs.SetInt("CurrentActiveSlot", 0);
        PlayerPrefs.Save();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        LoadScene("Gameplay");
    }

    /// <summary>
    /// Carrega o progresso de um dos slots selecionados no menu.
    /// </summary>
    private void OnSelectSlotToLoad(int slotIndex)
    {
        bool hasSave = PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1 || 
                       PlayerPrefs.HasKey($"Slot{slotIndex}_Level");

        if (!hasSave)
        {
            Debug.LogWarning($"[MainMenuUI] Slot {slotIndex} está vazio!");
            return;
        }

        // Define este slot como o slot ativo para a sessão
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        // Lê a fase gravada especificamente neste slot
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