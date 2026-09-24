using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", -1);

        // Se for uma nova sessão temporária (-1 ou 0 sem checkpoint), ignora o carregamento do disco
        if (activeSlot <= 0)
        {
            Debug.Log("[PlayerSaveLoader] Sessão temporária iniciada. O Jogador permanecerá na posição padrão do mapa.");
            return;
        }

        LoadPlayerFromSlot(activeSlot);
    }

    private void LoadPlayerFromSlot(int slotIndex)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 targetPosition = player.transform.position;
        bool positionFound = false;

        // 1. Tenta carregar do SaveSystem
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile(slotIndex))
        {
            SaveSystem.Instance.LoadDataInFile(slotIndex);
            SaveData data = SaveSystem.Instance.GetSaveData(slotIndex);
            if (data != null)
            {
                targetPosition = data.GetPlayerPosition();
                positionFound = true;
            }
        }

        // 2. Fallback para PlayerPrefs
        if (!positionFound && PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1)
        {
            targetPosition.x = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosX", targetPosition.x);
            targetPosition.y = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosY", targetPosition.y);
            targetPosition.z = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosZ", targetPosition.z);
            positionFound = true;
        }

        if (positionFound)
        {
            // Se tiver Rigidbody, desativa a física durante o teleporte para evitar bugs
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.transform.position = targetPosition;
            Debug.Log($"[PlayerSaveLoader] Jogador posicionado com SUCESSO no Slot {slotIndex}: {targetPosition}");
        }
    }
}