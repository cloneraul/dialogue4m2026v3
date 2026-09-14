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

        // 1. Identifica qual o Slot ativo da sessão atual
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        // 2. Se for 0 (Novo Jogo) ou se o Slot não tiver Checkpoint salvo, NÃO teletransporta o jogador
        if (activeSlot == 0 || PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 0)
        {
            Debug.Log($"[PlayerSaveLoader] Novo Jogo detectado (Slot {activeSlot}). Jogador mantido na posição inicial da cena e moedas zeradas.");
            
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.ResetCoinsForNewLevel();
            }
            yield break;
        }

        // 3. Se for um Slot válido (1, 2 ou 3) com checkpoint gravado, carrega a posição salva
        float posX = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX", transform.position.x);
        float posY = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY", transform.position.y);
        float posZ = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ", transform.position.z);

        Vector3 targetPosition = new Vector3(posX, posY, posZ);

        // Desativa temporariamente o CharacterController (se existir) para permitir o teletransporte sem colisão
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        transform.position = targetPosition;

        if (controller != null) controller.enabled = true;

        // Carrega também o contador de moedas salvo para este slot
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(activeSlot);
        }

        Debug.Log($"[PlayerSaveLoader] Slot {activeSlot} carregado com SUCESSO! Posição: {targetPosition}");
    }
}