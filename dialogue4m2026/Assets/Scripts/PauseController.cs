using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject saveSlotsPanel;
    [SerializeField] private GameObject loadSlotsPanel;

    [Header("Botões do Menu de Pause Principal")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Botões do Painel de Salvamento (Save)")]
    [SerializeField] private Button saveSlot1Button;
    [SerializeField] private Button saveSlot2Button;
    [SerializeField] private Button saveSlot3Button;
    [SerializeField] private Button backFromSaveButton;

    [Header("Botões do Painel de Carregamento (Load)")]
    [SerializeField] private Button loadSlot1Button;
    [SerializeField] private Button loadSlot2Button;
    [SerializeField] private Button loadSlot3Button;
    [SerializeField] private Button backFromLoadButton;

    [Header("Configurações de Cena")]
    [SerializeField] private string menuSceneName = "Menu";

    private bool isPaused;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (loadSlotsPanel != null) loadSlotsPanel.SetActive(false);

        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        // Menu Principal
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (saveButton != null) saveButton.onClick.AddListener(OpenSaveSlots);
        if (loadButton != null) loadButton.onClick.AddListener(OpenLoadSlots);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Slots de Salvamento (Save)
        if (saveSlot1Button != null) saveSlot1Button.onClick.AddListener(() => SaveProgressToTargetSlot(1));
        if (saveSlot2Button != null) saveSlot2Button.onClick.AddListener(() => SaveProgressToTargetSlot(2));
        if (saveSlot3Button != null) saveSlot3Button.onClick.AddListener(() => SaveProgressToTargetSlot(3));
        if (backFromSaveButton != null) backFromSaveButton.onClick.AddListener(OpenPauseMenu);

        // Slots de Carregamento (Load)
        if (loadSlot1Button != null) loadSlot1Button.onClick.AddListener(() => LoadGameFromSlot(1));
        if (loadSlot2Button != null) loadSlot2Button.onClick.AddListener(() => LoadGameFromSlot(2));
        if (loadSlot3Button != null) loadSlot3Button.onClick.AddListener(() => LoadGameFromSlot(3));
        if (backFromLoadButton != null) backFromLoadButton.onClick.AddListener(OpenPauseMenu);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // P para alternar Pause
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
            return;
        }

        if (!isPaused) return;

        // 1. Menu Principal (C, S, G, M)
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            if (Keyboard.current.cKey.wasPressedThisFrame) ResumeGame();
            else if (Keyboard.current.sKey.wasPressedThisFrame) OpenSaveSlots();
            else if (Keyboard.current.gKey.wasPressedThisFrame) OpenLoadSlots();
            else if (Keyboard.current.mKey.wasPressedThisFrame) ReturnToMainMenu();
        }
        // 2. Painel Save (1, 2, 3 para Salvar | 0 para Voltar)
        else if (saveSlotsPanel != null && saveSlotsPanel.activeSelf)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) SaveProgressToTargetSlot(1);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) SaveProgressToTargetSlot(2);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) SaveProgressToTargetSlot(3);
            else if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame) OpenPauseMenu();
        }
        // 3. Painel Load (U, I, O para Carregar | 0 para Voltar)
        else if (loadSlotsPanel != null && loadSlotsPanel.activeSelf)
        {
            if (Keyboard.current.uKey.wasPressedThisFrame) LoadGameFromSlot(1);
            else if (Keyboard.current.iKey.wasPressedThisFrame) LoadGameFromSlot(2);
            else if (Keyboard.current.oKey.wasPressedThisFrame) LoadGameFromSlot(3);
            else if (Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame) OpenPauseMenu();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OpenPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (loadSlotsPanel != null) loadSlotsPanel.SetActive(false);
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (loadSlotsPanel != null) loadSlotsPanel.SetActive(false);

        // O botão de Salvar fica SEMPRE interativo
        if (saveButton != null) 
        {
            saveButton.interactable = true;
        }
    }

    public void OpenSaveSlots()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
        if (loadSlotsPanel != null) loadSlotsPanel.SetActive(false);
    }

    public void OpenLoadSlots()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (loadSlotsPanel != null) loadSlotsPanel.SetActive(true);

        UpdateLoadButtonsInteractability();
    }

    private void UpdateLoadButtonsInteractability()
    {
        if (loadSlot1Button != null) loadSlot1Button.interactable = PlayerPrefs.GetInt("Slot1_HasCheckpoint", 0) == 1;
        if (loadSlot2Button != null) loadSlot2Button.interactable = PlayerPrefs.GetInt("Slot2_HasCheckpoint", 0) == 1;
        if (loadSlot3Button != null) loadSlot3Button.interactable = PlayerPrefs.GetInt("Slot3_HasCheckpoint", 0) == 1;
    }

    private void SaveProgressToTargetSlot(int targetSlot)
    {
        // Verifica se há um salvamento pendente acumulado do checkpoint ativo
        bool hasPendingSave = PlayerPrefs.GetInt("HasPendingSave", 0) == 1;

        if (!hasPendingSave)
        {
            Debug.LogWarning($"[PauseController] Tentativa de salvar no Slot {targetSlot} recusada! Você não passou por nenhum checkpoint ativo para registrar um novo progresso.");
            ResumeGame();
            return;
        }

        // Se tem pendência válida de checkpoint, prossegue gravando no slot desejado
        float posX = PlayerPrefs.GetFloat("Slot0_PosX", 0f);
        float posY = PlayerPrefs.GetFloat("Slot0_PosY", 0f);
        float posZ = PlayerPrefs.GetFloat("Slot0_PosZ", 0f);
        Vector3 checkpointPos = new Vector3(posX, posY, posZ);
        string currentScene = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetInt("CurrentActiveSlot", targetSlot);

        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosX", posX);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosY", posY);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosZ", posZ);
        PlayerPrefs.SetInt($"Slot{targetSlot}_HasCheckpoint", 1);
        PlayerPrefs.SetString($"Slot{targetSlot}_Scene", currentScene);

        string slot0Checkpoints = PlayerPrefs.GetString("Slot0_UsedCheckpoints", "");
        PlayerPrefs.SetString($"Slot{targetSlot}_UsedCheckpoints", slot0Checkpoints);

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(targetSlot);
        }

        if (SaveSystem.Instance != null)
        {
            SaveData data = new SaveData();
            data.SetPlayerPosition(checkpointPos);
            data.currentSceneName = currentScene;

            if (CoinManager.Instance != null)
            {
                data.totalCoins = CoinManager.Instance.CurrentCoins;
            }

            SaveSystem.Instance.SetSaveData(data, targetSlot);
            SaveSystem.Instance.SaveDataInFile(targetSlot);
        }

        // Reseta o salvamento pendente para impedir que salve novamente no mesmo checkpoint desativado
        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        Debug.Log($"[PauseController] Progresso salvo com sucesso no Slot {targetSlot}!");
        ResumeGame();
    }

    private void LoadGameFromSlot(int targetSlot)
    {
        if (PlayerPrefs.GetInt($"Slot{targetSlot}_HasCheckpoint", 0) != 1)
        {
            Debug.LogWarning($"[PauseController] O Slot {targetSlot} está vazio!");
            return;
        }

        // Prepara as variáveis no Slot 0 temporário
        float posX = PlayerPrefs.GetFloat($"Slot{targetSlot}_PosX", 0f);
        float posY = PlayerPrefs.GetFloat($"Slot{targetSlot}_PosY", 0f);
        float posZ = PlayerPrefs.GetFloat($"Slot{targetSlot}_PosZ", 0f);
        string usedCheckpoints = PlayerPrefs.GetString($"Slot{targetSlot}_UsedCheckpoints", "");

        PlayerPrefs.SetInt("CurrentActiveSlot", targetSlot);
        PlayerPrefs.SetFloat("Slot0_PosX", posX);
        PlayerPrefs.SetFloat("Slot0_PosY", posY);
        PlayerPrefs.SetFloat("Slot0_PosZ", posZ);
        PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
        PlayerPrefs.SetString("Slot0_UsedCheckpoints", usedCheckpoints);

        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(targetSlot);
        }

        Time.timeScale = 1f;

        string targetScene = PlayerPrefs.GetString($"Slot{targetSlot}_Scene", "");
        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = SceneManager.GetActiveScene().name;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMenuScene();
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}