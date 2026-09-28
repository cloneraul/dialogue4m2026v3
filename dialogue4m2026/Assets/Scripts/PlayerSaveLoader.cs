using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoader : MonoBehaviour
{
    [Header("Configurações de Spawn")]
    [Tooltip("Tempo de espera (em segundos) para a cena carregar totalmente antes de aplicar a posição")]
    [SerializeField] private float delayBeforeApply = 0.1f;

    private void Start()
    {
        StartCoroutine(ApplySavedPositionRoutine());
    }

    private IEnumerator ApplySavedPositionRoutine()
    {
        yield return new WaitForSeconds(delayBeforeApply);

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        string currentScene = SceneManager.GetActiveScene().name;

        // Verifica qual cena está associada ao checkpoint gravado neste Slot
        string savedScene = PlayerPrefs.GetString($"Slot{activeSlot}_Scene", "");
        bool hasCheckpoint = PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 1;

        // SE for Slot 0 (novo/temporário), SE não tiver checkpoint OU SE a cena salva for DIFERENTE da cena atual (Transição para Nível Novo)
        if (activeSlot == 0 || !hasCheckpoint || (!string.IsNullOrEmpty(savedScene) && savedScene != currentScene))
        {
            Debug.Log($"[PlayerSaveLoader] Início de fase/nova cena detectada ({currentScene}). Mantendo o jogador na posição inicial padrão.");
            
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.ResetCoinsForNewLevel();
            }
            yield break;
        }

        // Se for a MESMA CENA onde o checkpoint foi salvo, aplica as coordenadas salvas
        float posX = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX", transform.position.x);
        float posY = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY", transform.position.y);
        float posZ = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ", transform.position.z);

        Vector3 targetPosition = new Vector3(posX, posY, posZ);

        ForcePlayerPosition(targetPosition);

        // Carrega também as moedas gravadas no checkpoint desta cena
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(activeSlot);
        }

        Debug.Log($"[PlayerSaveLoader] Checkpoint da cena {currentScene} carregado com SUCESSO! Posição: {targetPosition}");
    }

    /// <summary>
    /// Teletransporta o jogador com segurança desativando temporariamente os componentes de física
    /// </summary>
    private void ForcePlayerPosition(Vector3 targetPosition)
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        transform.position = targetPosition;

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}