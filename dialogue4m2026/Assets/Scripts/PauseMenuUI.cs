using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Compatível com o New Input System

public class PauseMenuUI : MonoBehaviour
{
    [Header("Painel de Pausa")]
    [SerializeField] private GameObject pausePanel;

    [Header("Botões do Painel")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button backToMenuButton;

    private bool isPaused = false;

    private void Start()
    {
        // Garante que o painel comece escondido e o tempo normal
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        // Configura os botões de dentro do painel
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnClick_BackToMenu);
    }

    private void Update()
    {
        // Detecta o clique na tecla ESC ou P via New Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
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
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f; // Congela o jogo
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f; // Volta o jogo ao normal
    }

    public void OnClick_BackToMenu()
    {
        Time.timeScale = 1f; // OBRIGATÓRIO: Descongela o tempo antes de trocar de cena

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMenuScene();
        }
    }
}