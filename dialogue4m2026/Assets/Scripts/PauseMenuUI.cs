using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pausePanel;

    [Header("Botão do HUD (Na Tela)")]
    [SerializeField] private Button pauseHUDButton;

    [Header("Botões do Painel de Pausa")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMenuButton;

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        SetupButtons();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    private void SetupButtons()
    {
        if (pauseHUDButton != null)
            pauseHUDButton.onClick.AddListener(PauseGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnClick_BackToMenu);
    }

    // --- MÉTODOS PÚBLICOS (APARECEM NO ON CLICK DO INSPECTOR) ---

    public void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseHUDButton != null) pauseHUDButton.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseHUDButton != null) pauseHUDButton.gameObject.SetActive(true);

        Time.timeScale = 1f;
    }

    public void OnClick_BackToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadMenuScene();
    }
}