using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        LoadPlayerPosition();
    }

    public void LoadPlayerPosition()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        // Verifica se há um checkpoint/posição registrada para o slot ativo
        if (PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX");
            float y = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY");
            float z = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ");

            Vector3 targetPosition = new Vector3(x, y, z);

            // Se o Player usa CharacterController, precisa desativar antes de teleportar
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                transform.position = targetPosition;
                controller.enabled = true;
            }
            else
            {
                transform.position = targetPosition;
            }

            Debug.Log($"[PlayerSaveLoader] Jogador carregado no Slot {activeSlot} na posição exata: {targetPosition}");

            // Recarrega as moedas do slot ativo
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadCheckpointCoins(activeSlot);
            }
        }
    }
}