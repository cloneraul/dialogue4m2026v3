using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject saveSlotsPanel;

    [Header("Botões do Menu de Pause")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Botões do Painel de Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backToPauseButton;

    [Header("Configurações de Cena")]
    [SerializeField] private string menuSceneName = "Menu";

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);

        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (saveButton != null) saveButton.onClick.AddListener(OpenSaveSlots);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => SaveProgressToTargetSlot(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => SaveProgressToTargetSlot(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => SaveProgressToTargetSlot(3));
        if (backToPauseButton != null) backToPauseButton.onClick.AddListener(OpenPauseMenu);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. APENAS a tecla 'P' ativa/desativa o Pause
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
            return;
        }

        if (!isPaused) return;

        // 2. Atalhos no Menu Principal de Pause
        if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
        {
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                ResumeGame();
            }
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                OpenSaveSlots();
            }
            else if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ReturnToMainMenu();
            }
        }
        // 3. Atalhos no Painel de Slots (Teclas 1, 2, 3 e B)
        else if (saveSlotsPanel != null && saveSlotsPanel.activeSelf)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                SaveProgressToTargetSlot(1);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                SaveProgressToTargetSlot(2);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                SaveProgressToTargetSlot(3);
            }
            else if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OpenPauseMenu();
            }
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
    }

    public void OpenPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);

        // Bloqueia ou libera o botão visualmente de acordo com o status do checkpoint
        bool canSave = PlayerPrefs.GetInt("HasPendingSave", 0) == 1;
        if (saveButton != null)
        {
            saveButton.interactable = canSave;
        }
    }

    public void OpenSaveSlots()
    {
        // Trava: Se não tem checkpoint pendente, impede a abertura da tela de slots
        if (PlayerPrefs.GetInt("HasPendingSave", 0) != 1) return;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
    }

    private void SaveProgressToTargetSlot(int targetSlot)
    {
        // Trava de segurança extra para as teclas 1, 2 e 3
        if (PlayerPrefs.GetInt("HasPendingSave", 0) != 1) return;

        // Recupera os dados do checkpoint armazenados no Slot 0
        float posX = PlayerPrefs.GetFloat("Slot0_PosX", 0f);
        float posY = PlayerPrefs.GetFloat("Slot0_PosY", 0f);
        float posZ = PlayerPrefs.GetFloat("Slot0_PosZ", 0f);
        Vector3 checkpointPos = new Vector3(posX, posY, posZ);
        string currentScene = SceneManager.GetActiveScene().name;

        // Atualiza a sessão para o Slot escolhido
        PlayerPrefs.SetInt("CurrentActiveSlot", targetSlot);

        // Grava nos PlayerPrefs do slot definitivo
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosX", posX);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosY", posY);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosZ", posZ);
        PlayerPrefs.SetInt($"Slot{targetSlot}_HasCheckpoint", 1);
        PlayerPrefs.SetString($"Slot{targetSlot}_Scene", currentScene);

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(targetSlot);
        }

        // Grava no arquivo .dat físico encriptado
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

        // Consome a permissão para proibir novos salvamentos
        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        Debug.Log($"[PauseController] Progresso gravado no Slot {targetSlot}! Permissão consumida.");
        ResumeGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

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