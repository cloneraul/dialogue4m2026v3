using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        LoadAndApplySavePosition();
    }

    public void LoadAndApplySavePosition()
    {
        // Verifica se existe um checkpoint gravado para o Slot 0
        if (PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("Slot0_PosX");
            float y = PlayerPrefs.GetFloat("Slot0_PosY");
            float z = PlayerPrefs.GetFloat("Slot0_PosZ");

            Vector3 savedPosition = new Vector3(x, y, z);

            // Desativa o CharacterController temporariamente para aplicar o transporte sem conflito de física
            CharacterController characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            // Reposiciona o jogador
            transform.position = savedPosition;

            if (characterController != null)
            {
                characterController.enabled = true;
            }

            Debug.Log($"[Autosave] Jogador reposicionado para a posição do Checkpoint: {savedPosition}");
        }
        else
        {
            Debug.Log("[Autosave] Nenhum checkpoint salvo encontrado no Slot 0. Mantendo a posição inicial da cena.");
        }
    }
}