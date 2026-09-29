using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    [Header("Identificação do Checkpoint")]
    [Tooltip("ID ÚNICO para este checkpoint na cena (Exemplo: Checkpoint_Fase1_01)")]
    [SerializeField] private string checkpointID;

    private bool canTrigger;
    private Collider triggerCollider;

    private void Awake()
    {
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = gameObject.name;
        }

        triggerCollider = GetComponent<Collider>();
    }

    private IEnumerator Start()
    {
        // Pega o slot que está sendo usado no momento
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        // 1. Se o checkpoint já consta como USADO no Slot ativo OU no Slot 0 temporário, desativa na hora
        if (IsCheckpointAlreadyUsed(activeSlot, checkpointID) || IsCheckpointAlreadyUsed(0, checkpointID))
        {
            DisableCheckpointPermanently();
            yield break;
        }

        // 2. Aguarda um pequeno tempo ao iniciar a cena para evitar falsas colisões no Spawn
        canTrigger = false;
        yield return new WaitForSecondsRealtime(0.5f);
        canTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canTrigger) return;
        if (!other.CompareTag("Player")) return;

        // Posição exata do jogador e dados do nível
        Vector3 playerPos = other.transform.position;
        string currentScene = SceneManager.GetActiveScene().name;
        int levelNum = currentScene.EndsWith("2") ? 2 : 1;

        // 1. Grava os dados do Autosave temporário no Slot 0
        PlayerPrefs.SetFloat("Slot0_PosX", playerPos.x);
        PlayerPrefs.SetFloat("Slot0_PosY", playerPos.y);
        PlayerPrefs.SetFloat("Slot0_PosZ", playerPos.z);
        PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
        PlayerPrefs.SetInt("Slot0_Level", levelNum);
        PlayerPrefs.SetString("Slot0_Scene", currentScene);

        // Salva moedas do checkpoint no Slot 0
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(0);
        }

        // 2. Libera a permissão para salvar manualmente no Menu de Pause
        PlayerPrefs.SetInt("HasPendingSave", 1);

        // 3. Marca este checkpoint como USADO imediatamente no Slot 0 e no Slot Ativo
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        MarkCheckpointAsUsed(0, checkpointID);
        if (activeSlot != 0)
        {
            MarkCheckpointAsUsed(activeSlot, checkpointID);
        }

        PlayerPrefs.Save();

        Debug.Log($"[SaveTrigger] Checkpoint '{checkpointID}' consumido e desativado com sucesso!");

        // 4. Desativa o checkpoint IMEDIATAMENTE para que ele nunca mais reative nesta sessão
        DisableCheckpointPermanently();
    }

    private void DisableCheckpointPermanently()
    {
        canTrigger = false;

        // Desabilita o colisor
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        // Opcional: Se quiser esconder o objeto visualmente da cena
        // gameObject.SetActive(false);
    }

    #region MÉTODOS DE PERSISTÊNCIA

    private bool IsCheckpointAlreadyUsed(int slot, string id)
    {
        string usedCheckpoints = PlayerPrefs.GetString($"Slot{slot}_UsedCheckpoints", "");
        return usedCheckpoints.Contains($"[{id}]");
    }

    private void MarkCheckpointAsUsed(int slot, string id)
    {
        string usedCheckpoints = PlayerPrefs.GetString($"Slot{slot}_UsedCheckpoints", "");
        if (!usedCheckpoints.Contains($"[{id}]"))
        {
            usedCheckpoints += $"[{id}]";
            PlayerPrefs.SetString($"Slot{slot}_UsedCheckpoints", usedCheckpoints);
        }
    }

    #endregion
}