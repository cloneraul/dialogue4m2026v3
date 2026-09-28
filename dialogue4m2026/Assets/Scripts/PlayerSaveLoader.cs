using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoader : MonoBehaviour
{
    [Header("Configurações de Sincronização")]
    [Tooltip("Tempo de espera para estabilização de física e carregamento das cenas aditivas.")]
    [SerializeField] private float loadDelay = 0.15f;

    private void Start()
    {
        StartCoroutine(ApplyPositionWithDelayRoutine());
    }

    private IEnumerator ApplyPositionWithDelayRoutine()
    {
        // 1. Garante que o menu de Pause inicia bloqueado para novos saves nesta nova cena
        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        // 2. Aguarda o tempo de segurança e sincronização de quadros
        yield return new WaitForSeconds(loadDelay);
        yield return new WaitForEndOfFrame();

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        string currentScene = SceneManager.GetActiveScene().name;
        string savedScene = PlayerPrefs.GetString($"Slot{activeSlot}_Scene", "");

        bool hasCheckpoint = PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 1;

        // Se for uma partida sem checkpoint salvo ou transição direta de fase limpa
        if (activeSlot == 0 || !hasCheckpoint || (!string.IsNullOrEmpty(savedScene) && savedScene != currentScene))
        {
            Debug.Log($"[PlayerSaveLoader] Nova cena ({currentScene}) sem save ativo no Slot {activeSlot}. Posição mantida.");
            
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadCheckpointCoins(activeSlot);
            }
            yield break;
        }

        // 3. Posição salva
        float posX = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX", transform.position.x);
        float posY = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY", transform.position.y);
        float posZ = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ", transform.position.z);

        Vector3 targetPosition = new Vector3(posX, posY, posZ);

        // 4. Aplica posicionamento desativando a física no frame
        ForcePlayerPosition(targetPosition);

        // 5. Carrega rigorosamente as moedas do slot
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(activeSlot);
        }

        Debug.Log($"[PlayerSaveLoader] Transição concluída. Slot {activeSlot} posicionado em {targetPosition}.");
    }

    private void ForcePlayerPosition(Vector3 targetPosition)
    {
        CharacterController controller = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        if (controller != null) controller.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = targetPosition;

        if (controller != null) controller.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}