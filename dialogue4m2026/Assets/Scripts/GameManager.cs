using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadMenuScene();
    }

    // Carrega a cena do Menu Inicial
    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    // Carrega uma fase de Gameplay especificada e adiciona a GUI/HUD por cima
    public void LoadGameScene(string sceneName = "Gameplay")
    {
        SceneManager.LoadScene(sceneName);
        SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
    }

    // Método utilitário para carregar por índice do Build Settings, se preferir
    public void LoadGameScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
    }

    // Fecha a aplicação
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}