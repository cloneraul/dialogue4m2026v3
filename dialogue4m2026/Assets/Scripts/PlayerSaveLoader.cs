using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSaveLoader : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 1. Garante que o estado de salvamento pendente começa BLOQUEADO nesta nova entrada de cena
        PlayerPrefs.SetInt("HasPendingSave", 0);
        PlayerPrefs.Save();

        // 2. Aguarda até o motor de física do Unity estar 100% pronto e sincronizado
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        string currentScene = SceneManager.GetActiveScene().name;
        string savedScene = PlayerPrefs.GetString($"Slot{activeSlot}_Scene", "");

        bool hasCheckpoint = PlayerPrefs.GetInt($"Slot{activeSlot}_HasCheckpoint", 0) == 1;

        // SE for slot vazio ou cena diferente sem checkpoint gravado
        if (activeSlot == 0 || !hasCheckpoint || (!string.IsNullOrEmpty(savedScene) && savedScene != currentScene))
        {
            Debug.Log($"[PlayerSaveLoader] Início de fase normal na cena '{currentScene}'. Jogador mantido no Spawn.");
            
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadCheckpointCoins(activeSlot);
            }
            yield break;
        }

        // 3. Lê as coordenadas salvas no Slot
        float posX = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosX", transform.position.x);
        float posY = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosY", transform.position.y);
        float posZ = PlayerPrefs.GetFloat($"Slot{activeSlot}_PosZ", transform.position.z);

        Vector3 targetPosition = new Vector3(posX, posY, posZ);

        // 4. Força o posicionamento garantindo que a física não sobrescreva
        yield return StartCoroutine(ForcePlayerPositionRoutine(targetPosition));

        // 5. Sincroniza as moedas do slot
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.LoadCheckpointCoins(activeSlot);
        }

        Debug.Log($"[PlayerSaveLoader] Sucesso! Jogador posicionado com segurança em: {targetPosition}");
    }

    private IEnumerator ForcePlayerPositionRoutine(Vector3 targetPosition)
    {
        CharacterController controller = GetComponent<CharacterController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        // Desativa a física temporariamente para evitar relutância de movimento
        if (controller != null) controller.enabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Aplica a posição
        transform.position = targetPosition;

        // Aguarda 1 frame com a física desativada para a Unity fixar a nova posição
        yield return null;

        // Reativa a física e confirma o posicionamento
        transform.position = targetPosition;

        if (rb != null) rb.isKinematic = false;
        if (controller != null) controller.enabled = true;
    }
}