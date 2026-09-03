using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseToMenu : MonoBehaviour
{
    [Header("Configurações de Cena")]
    [Tooltip("Nome exato da cena do Menu Principal")]
    [SerializeField] private string menuSceneName = "Menu";

    [Tooltip("Nome da cena de UI/Moedas que deve ser fechada ao voltar pro menu")]
    [SerializeField] private string guiSceneName = "GUI";

    private void Update()
    {
        // Pressionar a tecla ESC no New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[Pause] Pressionou ESC. Descarregando UI e retornando ao Menu...");

        // 1. Libera o cursor do mouse para interagir com o menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Descarrega a cena da GUI caso ela esteja aberta
        Scene guiScene = SceneManager.GetSceneByName(guiSceneName);
        if (guiScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(guiSceneName);
        }

        // 3. Retorna ao Menu Principal pelo GameManager (ou SceneManager como fallback)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}