using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    [Header("Painéis de UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject saveSlotsPanel;

    [Header("Configurações de Cena")]
    [SerializeField] private string menuSceneName = "Menu";

    private bool isPaused = false;
    private bool inSaveSlots = false;

    private void Start()
    {
        // Garante que a UI inicie fechada
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 1. Tecla P: Alterna o estado do Pause (Abre/Fecha)
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
            return;
        }

        // Se o jogo NÃO estiver pausado, ignora as outras teclas
        if (!isPaused) return;

        // 2. Comandos quando o menu de Pause Principal estiver aberto
        if (!inSaveSlots)
        {
            // Tecla C: Continuar Jogando
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                ResumeGame();
            }
            // Tecla S: Abrir Painel de Saves (Slots)
            else if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                OpenSaveSlots();
            }
            // Tecla M: Voltar ao Menu Principal
            else if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                ReturnToMainMenu();
            }
        }
        // 3. Comandos quando o painel de SLOTS estiver aberto
        else
        {
            // Tecla 1, 2 ou 3 para salvar no Slot correspondente
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                SaveGameToSlot(1);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                SaveGameToSlot(2);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                SaveGameToSlot(3);
            }
            // Tecla B: Voltar para o menu de pause anterior
            else if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                OpenPauseMenu();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pausa a física do jogo
        OpenPauseMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;
        inSaveSlots = false;
        Time.timeScale = 1f; // Volta o tempo do jogo

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
        PlayerPrefs.Save();

        SaveTrigger activeTrigger = Object.FindAnyObjectByType<SaveTrigger>();
        if (activeTrigger != null)
        {
            activeTrigger.ExecuteSave();
        }
        else
        {
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

        Debug.Log($"[PauseController] Progresso salvo no Slot {slotIndex} via teclado!");

        // Retorna ao menu de pause principal após salvar
        OpenPauseMenu();
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