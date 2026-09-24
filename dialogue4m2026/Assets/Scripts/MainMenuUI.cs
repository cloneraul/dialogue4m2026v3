using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private GameObject noSavesMessagePanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button playButton;     // Botão "Carregar Jogo"
    [SerializeField] private Button newGameButton;  // Botão "Novo Jogo"
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
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnClick_LoadGameButton);
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

    public void OnClick_StartNewGameDirectly()
    {
        Debug.Log("[MainMenuUI] Iniciando Novo Jogo em sessão limpa...");

        // -1 indica Novo Jogo sem slot atrelado
        PlayerPrefs.SetInt("CurrentActiveSlot", -1);

        // Limpa o slot 0 temporário
        PlayerPrefs.DeleteKey("Slot0_HasCheckpoint");
        PlayerPrefs.DeleteKey("Slot0_PosX");
        PlayerPrefs.DeleteKey("Slot0_PosY");
        PlayerPrefs.DeleteKey("Slot0_PosZ");
        PlayerPrefs.DeleteKey("Slot0_Coins");
        PlayerPrefs.DeleteKey("Slot0_CoinIDs");
        PlayerPrefs.SetInt("Slot0_Level", 1);
        PlayerPrefs.SetString("Slot0_Scene", "Gameplay");
        PlayerPrefs.Save();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        LoadScene("Gameplay");
    }

    public void OnClick_LoadGameButton()
    {
        bool hasAnySave = CheckSlotHasSave(1) || CheckSlotHasSave(2) || CheckSlotHasSave(3);

        if (hasAnySave)
        {
            ShowSlotsPanel();
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] Nenhum save encontrado nos Slots 1, 2 ou 3.");
            if (noSavesMessagePanel != null)
            {
                noSavesMessagePanel.SetActive(true);
            }
        }
    }

    public void OnSelectSlotToLoad(int slotIndex)
    {
        if (!CheckSlotHasSave(slotIndex)) return;

        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        string targetScene = PlayerPrefs.GetString($"Slot{slotIndex}_Scene", "");

        if (string.IsNullOrEmpty(targetScene))
        {
            int savedLevel = PlayerPrefs.GetInt($"Slot{slotIndex}_Level", 1);
            targetScene = (savedLevel == 2) ? "Gameplay 2" : "Gameplay";
        }

        Debug.Log($"[MainMenuUI] Carregando Slot {slotIndex} -> Cena: {targetScene}");
        LoadScene(targetScene);
    }

    private void RefreshSlotsInteractability()
    {
        if (slot1Button != null) slot1Button.interactable = CheckSlotHasSave(1);
        if (slot2Button != null) slot2Button.interactable = CheckSlotHasSave(2);
        if (slot3Button != null) slot3Button.interactable = CheckSlotHasSave(3);
    }

    private bool CheckSlotHasSave(int slotIndex)
    {
        if (slotIndex <= 0) return false;

        bool hasPrefs = PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1 ||
                        PlayerPrefs.HasKey($"Slot{slotIndex}_Level") ||
                        PlayerPrefs.HasKey($"Slot{slotIndex}_Scene");

        bool hasFile = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile(slotIndex);

        return hasPrefs || hasFile;
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
        if (noSavesMessagePanel != null) noSavesMessagePanel.SetActive(false);
    }

    public void ShowSlotsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
        if (noSavesMessagePanel != null) noSavesMessagePanel.SetActive(false);

        RefreshSlotsInteractability();
    }
}