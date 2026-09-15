using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject saveSlotsPanel;

    [Header("Configurações de Cena")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool isPaused = false;
    private bool inSaveSlots = false;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Tecla P: Alterna o Pause
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
            return;
        }

        if (!isPaused) return;

        // 2. Comandos do Menu Principal de Pause
        if (!inSaveSlots)
        {
            if (Keyboard.current.cKey.wasPressedThisFrame) ResumeGame();
            else if (Keyboard.current.sKey.wasPressedThisFrame) OpenSaveSlots();
            else if (Keyboard.current.mKey.wasPressedThisFrame) ReturnToMainMenu();
        }
        // 3. Comandos na Seleção de Slots (1, 2 ou 3)
        else
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) SaveGameToSlot(1);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) SaveGameToSlot(2);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) SaveGameToSlot(3);
            else if (Keyboard.current.bKey.wasPressedThisFrame) OpenPauseMenu();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        OpenPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        inSaveSlots = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    public void OpenPauseMenu()
    {
        inSaveSlots = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    public void OpenSaveSlots()
    {
        inSaveSlots = true;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(true);
    }

    private void SaveGameToSlot(int slotIndex)
    {
        PlayerPrefs.SetInt("CurrentActiveSlot", slotIndex);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            int currentLevel = (SceneManager.GetActiveScene().name == "Gameplay 2") ? 2 : 1;

            // Save de coordenadas e estado no PlayerPrefs
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

            // Chamada silenciosa do SaveSystem
            if (SaveSystem.Instance != null)
            {
                try
                {
                    SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                    SaveSystem.Instance.SaveDataInFile(slotIndex);
                }
                catch (System.Exception)
                {
                    // Exceção capturada e ignorada com segurança
                }
            }
        }

        Debug.Log($"[PauseController] Progresso salvo com SUCESSO no Slot {slotIndex}!");
        ResumeGame();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

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