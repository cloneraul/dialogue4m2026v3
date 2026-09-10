using System.Collections;
using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        // Inicia o processo de verificação e reposicionamento
        StartCoroutine(ApplySavedPositionRoutine());
    }

    private IEnumerator ApplySavedPositionRoutine()
    {
        // 1. Aguarda 2 frames para garantir que a Unity carregou todos os GameObjects da Fase 2
        yield return null;
        yield return new WaitForEndOfFrame();

        // 2. Checa se existe checkpoint registrado no Slot 0
        if (PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1)
        {
            float x = PlayerPrefs.GetFloat("Slot0_PosX");
            float y = PlayerPrefs.GetFloat("Slot0_PosY");
            float z = PlayerPrefs.GetFloat("Slot0_PosZ");

            Vector3 savedPosition = new Vector3(x, y, z);

            CharacterController cc = GetComponent<CharacterController>();
            Rigidbody rb = GetComponent<Rigidbody>();

            // 3. DESATIVA OS COMPONENTES DE FÍSICA E MOVIMENTO
            if (cc != null) cc.enabled = false;
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero; // Zeramos qualquer inércia/velocidade
            }

            // 4. APLICA A POSIÇÃO DIRETAMENTE
            transform.position = savedPosition;

            // Espera mais 1 frame com o colisor desativado para a Unity assentar a física
            yield return null;

            // Garante a posição novamente para evitar sobrescrita de scripts de Spawn
            transform.position = savedPosition;

            // 5. REATIVA A FÍSICA
            if (rb != null) rb.isKinematic = false;
            if (cc != null) cc.enabled = true;

            // 6. CARREGA AS MOEDAS DO SLOT 0
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.LoadCheckpointCoins(0);
            }

            Debug.Log($"[PlayerSaveLoader] Teletransporte concluído com SUCESSO! Posição carregada: {savedPosition}");
        }
        else
        {
            Debug.Log("[PlayerSaveLoader] Nenhum checkpoint ativo no Slot 0. Mantendo posição de spawn padrão da cena.");
        }
    }
}