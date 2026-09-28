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

    private bool isPaused;

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
        }

        // 2. Atalhos de teclado quando o jogo ESTIVER PAUSADO
        if (isPaused)
        {
            if (pauseMenuPanel != null && pauseMenuPanel.activeSelf)
            {
                if (Keyboard.current.cKey.wasPressedThisFrame) ResumeGame();
                else if (Keyboard.current.sKey.wasPressedThisFrame) OpenSaveSlots();
                else if (Keyboard.current.mKey.wasPressedThisFrame) ReturnToMainMenu();
            }
            else if (saveSlotsPanel != null && saveSlotsPanel.activeSelf)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) SaveProgressToTargetSlot(1);
                else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) SaveProgressToTargetSlot(2);
                else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) SaveProgressToTargetSlot(3);
                else if (Keyboard.current.bKey.wasPressedThisFrame) OpenPauseMenu();
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
    }

    public void OpenSaveSlots()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
    }

    private void SaveProgressToTargetSlot(int targetSlot)
    {
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 savePos = (player != null) ? player.transform.position : Vector3.zero;
        string currentScene = SceneManager.GetActiveScene().name;
        int levelNum = currentScene.EndsWith("2") ? 2 : 1;

        // Se passou por um Checkpoint recente nesta sessão, usa as coordenadas salvas dele
        if (PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1)
        {
            savePos.x = PlayerPrefs.GetFloat("Slot0_PosX", savePos.x);
            savePos.y = PlayerPrefs.GetFloat("Slot0_PosY", savePos.y);
            savePos.z = PlayerPrefs.GetFloat("Slot0_PosZ", savePos.z);
        }

        // Vincula a sessão atual oficialmente ao Slot escolhido
        PlayerPrefs.SetInt("CurrentActiveSlot", targetSlot);

        // Salva rigorosamente nos PlayerPrefs do slot correto
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosX", savePos.x);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosY", savePos.y);
        PlayerPrefs.SetFloat($"Slot{targetSlot}_PosZ", savePos.z);
        PlayerPrefs.SetInt($"Slot{targetSlot}_HasCheckpoint", 1);
        PlayerPrefs.SetInt($"Slot{targetSlot}_Level", levelNum);
        PlayerPrefs.SetString($"Slot{targetSlot}_Scene", currentScene);

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(targetSlot);
        }

        PlayerPrefs.Save();

        // Grava no arquivo físico do SaveSystem usand o índice de memória 0 com segurança
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveData data = new SaveData();
                data.SetPlayerPosition(savePos);
                data.currentSceneName = currentScene;

                if (CoinManager.Instance != null)
                {
                    data.totalCoins = CoinManager.Instance.CurrentCoins;
                }

                SaveSystem.Instance.SetSaveData(data, 0);
                SaveSystem.Instance.SaveDataInFile(targetSlot);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[PauseController] Erro ao gravar ficheiro no SaveSystem: {e.Message}");
            }
        }

        Debug.Log($"[PauseController] Progresso gravado no Slot {targetSlot} | Cena: {currentScene}");
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