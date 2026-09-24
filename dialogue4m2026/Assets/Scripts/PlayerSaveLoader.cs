using System.Collections;
using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Aguarda 1 frame para garantir que os Singletons da cena iniciaram corretamente
        yield return null;

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", -1);

        // Se for Novo Jogo (slot -1), não altera o jogador e deixa-o no spawn inicial do mapa
        if (activeSlot < 0)
        {
            Debug.Log("[PlayerSaveLoader] Novo Jogo detectado. Jogador mantido na posição inicial.");
            yield break;
        }

        LoadPlayerFromSlot(activeSlot);
    }

    private void LoadPlayerFromSlot(int slotIndex)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[PlayerSaveLoader] Jogador não encontrado na cena!");
            return;
        }

        Vector3 targetPosition = Vector3.zero;
        bool positionFound = false;

        // 1. PRIORIDADE: Checa PlayerPrefs (Mais recente)
        if (PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosX");
            float y = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosY");
            float z = PlayerPrefs.GetFloat($"Slot{slotIndex}_PosZ");

            if (x != 0f || y != 0f || z != 0f)
            {
                targetPosition = new Vector3(x, y, z);
                positionFound = true;
            }
        }

        // 2. FALLBACK: SaveSystem em arquivo de disco
        if (!positionFound && SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile(slotIndex))
        {
            SaveSystem.Instance.LoadDataInFile(slotIndex);
            SaveData data = SaveSystem.Instance.GetSaveData(slotIndex);

            if (data != null && data.GetPlayerPosition() != Vector3.zero)
            {
                targetPosition = data.GetPlayerPosition();
                positionFound = true;
            }
        }

        if (positionFound)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            player.transform.position = targetPosition;

            if (cc != null) cc.enabled = true;

            Debug.Log($"[PlayerSaveLoader] Jogador reposicionado no Slot {slotIndex} em: {targetPosition}");
        }
    }
}