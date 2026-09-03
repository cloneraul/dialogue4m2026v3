using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Configurações do Checkpoint")]
    [Tooltip("Identificador opcional para acompanhamento no Console")]
    [SerializeField] private string checkpointID = "Checkpoint_01";

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Garante que apenas o Jogador ative o Checkpoint
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            Vector3 playerPos = other.transform.position;

            // 1. Salva a posição exata (X, Y, Z) no Slot 0 (Autosave)
            PlayerPrefs.SetFloat("Slot0_PosX", playerPos.x);
            PlayerPrefs.SetFloat("Slot0_PosY", playerPos.y);
            PlayerPrefs.SetFloat("Slot0_PosZ", playerPos.z);
            PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
            PlayerPrefs.Save();

            // 2. Registra o nível e grava o arquivo criptografado do Slot 0 no SaveSystem original
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetPlayerLevel(1, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            Debug.Log($"[Autosave] Checkpoint '{checkpointID}' ativado com sucesso! Posição salva: {playerPos}");
        }
    }
}