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
            
            // Inscreve no evento de troca de cena para controlar a GUI automaticamente
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        // Se for iniciado direto da cena _Boot, carrega o Menu
        if (SceneManager.GetActiveScene().name == "_Boot")
        {
            LoadMenuScene();
        }
    }

    // Chamado automaticamente SEMPRE que uma cena termina de carregar
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Se estiver no Menu (ou _Boot), garante que a cena GUI seja descarregada
        if (scene.name == "Menu" || scene.name == "_Boot")
        {
            UnloadGUISceneIfLoaded();
        }
        // 2. Se estiver em qualquer fase de Gameplay (Gameplay, Gameplay 2, etc.)
        else if (scene.name.StartsWith("Gameplay"))
        {
            EnsureGUISceneIsLoaded();
        }
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("Menu");
    }

    public void LoadGameScene(string sceneName = "Gameplay")
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadGameScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    private void EnsureGUISceneIsLoaded()
    {
        Scene guiScene = SceneManager.GetSceneByName("GUI");
        if (!guiScene.isLoaded)
        {
            SceneManager.LoadSceneAsync("GUI", LoadSceneMode.Additive);
        }
    }

    private void UnloadGUISceneIfLoaded()
    {
        Scene guiScene = SceneManager.GetSceneByName("GUI");
        if (guiScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync("GUI");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}