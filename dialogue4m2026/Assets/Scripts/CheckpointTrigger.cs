using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Configurações do Checkpoint")]
    [SerializeField] private string checkpointID = "Checkpoint_01";

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            Vector3 playerPos = other.transform.position;

            // 1. Salva a posição nos PlayerPrefs para o Slot 0 e Slot 1
            SavePositionForSlot(0, playerPos);
            SavePositionForSlot(1, playerPos);

            // 2. Chama o SaveSystem APENAS para o Slot 0 (evita o erro de índice fora de alcance)
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetPlayerLevel(1, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            Debug.Log($"[Save Complete] Checkpoint '{checkpointID}' gravou a posição {playerPos} nos Slots 0 e 1!");
        }
    }

    private void SavePositionForSlot(int slotIndex, Vector3 pos)
    {
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosX", pos.x);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosY", pos.y);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosZ", pos.z);
        PlayerPrefs.SetInt($"Slot{slotIndex}_HasCheckpoint", 1);
        PlayerPrefs.Save();
    }
}