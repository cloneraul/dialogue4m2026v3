using System.Collections;
using UnityEngine;

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

        // Identifica qual o Slot ativo da sessão atual (0 = Autosave, 1, 2 ou 3 = Manual)
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        Vector3 targetPosition = Vector3.zero;
        bool hasSavedPosition = false;

        // 1. Tenta carregar a posição do arquivo do SaveSystem encriptado
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile(activeSlot))
        {
            SaveSystem.Instance.LoadDataInFile(activeSlot);
            SaveData data = SaveSystem.Instance.GetSaveData(activeSlot);

            if (data != null && data.playerPosition != null && data.playerPosition.Length == 3)
            {
                targetPosition = data.GetPlayerPosition();
                
                // Se a posição gravada não for a origem default (0,0,0)
                if (targetPosition != Vector3.zero)
                {
                    hasSavedPosition = true;
                }
            }
        }

        // 2. Backup: Se não achou no arquivo físico, busca no PlayerPrefs
        if (!hasSavedPosition && PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 1)
        {
            float posX = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX", transform.position.x);
            float posY = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY", transform.position.y);
            float posZ = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ", transform.position.z);

            targetPosition = new Vector3(posX, posY, posZ);
            hasSavedPosition = true;
        }

        // 3. Se não houver nenhum checkpoint salvo (Novo Jogo do zero), mantém no início e reseta as moedas
        if (!hasSavedPosition)
        {
            Debug.Log($"[PlayerSaveLoader] Nenhum checkpoint salvo no Slot {activeSlot}. Mantendo no início da fase.");
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.ResetCoinsForNewLevel();
            }
            yield break;
        }

        // 4. Aplica o Teleporte de Forma Segura
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPosition;
        }

        transform.position = targetPosition;

        if (controller != null) controller.enabled = true;

        // 5. Carrega o estado das moedas vinculadas ao slot carregado
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(activeSlot);
        }

        Debug.Log($"[PlayerSaveLoader] Slot {activeSlot} carregado com SUCESSO! Posição carregada: {targetPosition}");
    }
}