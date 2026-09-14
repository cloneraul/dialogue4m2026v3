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

    // Carrega a cena do Menu Inicial e remove a GUI se estiver aberta
    public void LoadMenuScene()
    {
        // Se a cena GUI estiver carregada, descarrega ela primeiro
        Scene guiScene = SceneManager.GetSceneByName("GUI");
        if (guiScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync("GUI");
        }

        SceneManager.LoadScene("Menu");
    }

    // Carrega uma fase de Gameplay especificada e adiciona a GUI/HUD por cima obrigatoriamente
    public void LoadGameScene(string sceneName = "Gameplay")
    {
        SceneManager.LoadScene(sceneName);

        // Garante que a GUI só é carregada de forma aditiva se já não estiver na memória
        Scene guiScene = SceneManager.GetSceneByName("GUI");
        if (!guiScene.isLoaded)
        {
            SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
        }
    }

    // Método utilitário para carregar por índice do Build Settings
    public void LoadGameScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);

        Scene guiScene = SceneManager.GetSceneByName("GUI");
        if (!guiScene.isLoaded)
        {
            SceneManager.LoadScene("GUI", LoadSceneMode.Additive);
        }
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