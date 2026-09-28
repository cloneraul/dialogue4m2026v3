using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    [Header("Identificação do Checkpoint")]
    [Tooltip("ID ÚNICO para este checkpoint na cena")]
    [SerializeField] private string checkpointID;

    private bool hasBeenTriggeredInSession;

    private void Awake()
    {
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = gameObject.name;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Se já foi ativado nesta caminhada/sessão, não repete
        if (hasBeenTriggeredInSession) return;

        hasBeenTriggeredInSession = true;

        // CAPTURA A POSIÇÃO EXATA DO JOGADOR NO MOMENTO DO CONTATO
        Vector3 playerPos = other.transform.position;
        string currentScene = SceneManager.GetActiveScene().name;
        int levelNum = currentScene.EndsWith("2") ? 2 : 1;

        // 1. Grava no Autosave temporário (Slot 0) com as coordenadas REAIS do jogador
        PlayerPrefs.SetFloat("Slot0_PosX", playerPos.x);
        PlayerPrefs.SetFloat("Slot0_PosY", playerPos.y);
        PlayerPrefs.SetFloat("Slot0_PosZ", playerPos.z);
        PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
        PlayerPrefs.SetInt("Slot0_Level", levelNum);
        PlayerPrefs.SetString("Slot0_Scene", currentScene);

        // Salva o estado das moedas no Slot 0
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(0);
        }

        // 2. Libera a permissão de salvar manualmente no Pause
        PlayerPrefs.SetInt("HasPendingSave", 1);
        PlayerPrefs.Save();

        Debug.Log($"[SaveTrigger] Checkpoint '{checkpointID}' ativado na posição do Player: {playerPos}");
    }

    public void ResetTriggerSession()
    {
        hasBeenTriggeredInSession = false;
    }
}