using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneTransitionInteractable : MonoBehaviour
{
    [Header("Configurações de Transição")]
    [Tooltip("Nome exato da cena de destino no Build Settings")]
    [SerializeField] private string targetSceneName = "Gameplay 2";

    [Header("Posição do Botão 'E'")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0);

    private bool isPlayerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            NotifyInteractPosition(transform.position + buttonOffset);
            NotifyInteractable(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            NotifyInteractable(false);
        }
    }

    private void Update()
    {
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteTransitionToLevel2();
        }
    }

    private void ExecuteTransitionToLevel2()
    {
        NotifyInteractable(false);

        // 1. Desvincula o Slot Permanente (1, 2 ou 3) da sessão imediata para NÃO sobrescrever os ficheiros dele na transição.
        // A partir deste momento, a transição passa a ser processada unicamente na sessão temporária (Slot 0).
        PlayerPrefs.SetInt("CurrentActiveSlot", 0);

        // 2. Limpa o contador e IDs das moedas na memória do jogo para a Fase 2
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 3. Prepara o Slot 0 (Temporário) para a nova fase
        PlayerPrefs.SetInt("Slot0_HasCheckpoint", 0); // Garante que o jogador surja no Spawn padrão da nova fase
        PlayerPrefs.SetInt("Slot0_Level", 2);
        PlayerPrefs.SetString("Slot0_Scene", targetSceneName);
        PlayerPrefs.DeleteKey("Slot0_Coins");
        PlayerPrefs.DeleteKey("Slot0_CoinIDs");

        PlayerPrefs.Save();

        Debug.Log($"[SceneTransition] Transição para {targetSceneName} realizada com segurança. O Slot manual foi preservado!");

        // 4. Executa a mudança de cena
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetSceneName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // --- MÉTODOS DE INTEGRAÇÃO COM O INTERACTOM ---

    private void NotifyInteractable(bool state)
    {
        Type type = typeof(InteractOM);
        FieldInfo field = type.GetField("OnInteractable", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            MulticastDelegate multicast = field.GetValue(null) as MulticastDelegate;
            if (multicast != null)
            {
                foreach (Delegate del in multicast.GetInvocationList())
                {
                    del.DynamicInvoke(state);
                }
            }
        }
    }

    private void NotifyInteractPosition(Vector3 pos)
    {
        Type type = typeof(InteractOM);
        FieldInfo field = type.GetField("InteractPosition", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null)
        {
            MulticastDelegate multicast = field.GetValue(null) as MulticastDelegate;
            if (multicast != null)
            {
                foreach (Delegate del in multicast.GetInvocationList())
                {
                    del.DynamicInvoke(pos);
                }
            }
        }
    }
}