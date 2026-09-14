using UnityEngine;
using UnityEngine.InputSystem;

public class PauseToMenu : MonoBehaviour
{
    private void Update()
    {
        // Pressionar a tecla P
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            ReturnToMainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        // Restaura a escala de tempo do jogo se estiver pausado
        Time.timeScale = 1f;

        // Libera o cursor do mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Chama o GameManager para cuidar de carregar o Menu e limpar a GUI com segurança
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMenuScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}