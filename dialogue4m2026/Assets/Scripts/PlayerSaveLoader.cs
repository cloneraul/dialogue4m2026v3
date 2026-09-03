using System.Collections;
using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        // Aguarda a cena e o motor de física inicializarem antes de mover
        StartCoroutine(ApplySavedPositionNextFrame());
    }

    private IEnumerator ApplySavedPositionNextFrame()
    {
        // Espera o final do frame atual da Unity para garantir prioridade de execução
        yield return new WaitForEndOfFrame();

        // Checa se há posição de checkpoint registrada para o Slot 0
        if (PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("Slot0_PosX");
            float y = PlayerPrefs.GetFloat("Slot0_PosY");
            float z = PlayerPrefs.GetFloat("Slot0_PosZ");

            Vector3 savedPosition = new Vector3(x, y, z);

            // Desativa os componentes de física temporariamente para o teletransporte
            CharacterController cc = GetComponent<CharacterController>();
            Rigidbody rb = GetComponent<Rigidbody>();

            if (cc != null) cc.enabled = false;
            if (rb != null) rb.isKinematic = true;

            // Aplica a posição salva
            transform.position = savedPosition;

            // Garante 1 frame de espera com o colisor desativado
            yield return null;

            // Reativa a física e colisão
            if (rb != null) rb.isKinematic = false;
            if (cc != null) cc.enabled = true;

            Debug.Log($"[SaveLoader] Jogador reposicionado com sucesso para: {savedPosition}");
        }
        else
        {
            Debug.Log("[SaveLoader] Nenhum checkpoint salvo. Mantendo spawn padrão.");
        }
    }
}