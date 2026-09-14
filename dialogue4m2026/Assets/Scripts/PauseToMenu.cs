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
    [SerializeField] private Button resumeButton;      // Continuar Jogando
    [SerializeField] private Button saveButton;        // Abrir Painel de Saves
    [SerializeField] private Button mainMenuButton;    // Voltar ao Menu Principal

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
        // Garante que o menu de pause inicie fechado
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);

        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (saveButton != null) saveButton.onClick.AddListener(OpenSaveSlots);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => SaveGameToSlot(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => SaveGameToSlot(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => SaveGameToSlot(3));
        if (backToPauseButton != null) backToPauseButton.onClick.AddListener(OpenPauseMenu);
    }

    private void Update()
    {
        // Ativa/Desativa o Pause ao pressionar a tecla P
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pausa o tempo do jogo

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OpenPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Retoma o tempo do jogo

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

    /// <summary>
    /// Vincula a sessão atual ao Slot escolhido e realiza um save manual na posição atual
    /// </summary>
    private void SaveGameToSlot(int slotIndex)
    {
        // 1. Define o slot escolhido como o ativo para a partida
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);
        PlayerPrefs.Save();

        // 2. Procura um SaveTrigger presente na cena usando a API moderna da Unity
        SaveTrigger activeTrigger = Object.FindAnyObjectByType<SaveTrigger>();
        if (activeTrigger != null)
        {
            activeTrigger.ExecuteSave();
        }
        else
        {
            // Gravação de emergência caso não haja um totem/checkpoint ativo na cena
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                int currentLevel = (SceneManager.GetActiveScene().name == "Gameplay 2") ? 2 : 1;

                PlayerPrefs.SetFloat($"Slot{slotIndex}_PosX", player.transform.position.x);
                PlayerPrefs.SetFloat($"Slot{slotIndex}_PosY", player.transform.position.y);
                PlayerPrefs.SetFloat($"Slot{slotIndex}_PosZ", player.transform.position.z);
                PlayerPrefs.SetInt($"Slot{slotIndex}_HasCheckpoint", 1);
                PlayerPrefs.SetInt($"Slot{slotIndex}_Level", currentLevel);

                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.SaveCheckpointCoins(slotIndex);
                }

                PlayerPrefs.Save();
            }
        }

        Debug.Log($"[PauseController] Progresso fixado e salvo com SUCESSO no Slot {slotIndex}!");
        
        // Retorna ao painel de pause
        OpenPauseMenu();
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