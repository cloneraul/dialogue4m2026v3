using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransitionInteractable : MonoBehaviour
{
    [Header("Configurações de Transição")]
    [Tooltip("Nome exato da cena de destino no Build Settings")]
    [SerializeField] private string targetSceneName = "Gameplay 2";

    [Tooltip("Número da próxima fase (Fase 2)")]
    [SerializeField] private int nextLevelNumber = 2;

    [Header("Posição do Botão 'E'")]
    [Tooltip("Deslocamento de altura para o botão 'E' flutuar em cima do objeto")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0);

    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            // Envia a posição e ativa o botão "E" flutuante
            NotifyInteractPosition(transform.position + buttonOffset);
            NotifyInteractable(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Oculta o botão "E" ao se afastar
            NotifyInteractable(false);
        }
    }

    private void Update()
    {
        // Se estiver perto do objeto e pressionar 'E', avança para a Fase 2
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteTransitionToLevel2();
        }
    }

    private void ExecuteTransitionToLevel2()
    {
        // Oculta o botão de interação antes de carregar a cena
        NotifyInteractable(false);

        Debug.Log($"[Transição] Interação ativada na Fase 1! Carregando: {targetSceneName}");

        // 1. Reseta moedas no CoinManager no momento exato da troca de fase
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 2. Limpa dados do checkpoint da Fase 1 do Slot 0 para o jogador iniciar no início da Fase 2
        PlayerPrefs.DeleteKey("Slot0_HasCheckpoint");
        PlayerPrefs.DeleteKey("Slot0_PosX");
        PlayerPrefs.DeleteKey("Slot0_PosY");
        PlayerPrefs.DeleteKey("Slot0_PosZ");
        PlayerPrefs.SetInt("Slot0_Level", nextLevelNumber);
        PlayerPrefs.Save();

        // 3. Atualiza o progresso para Nível 2 no SaveSystem
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SetPlayerLevel(nextLevelNumber, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SceneTransition] Erro ao salvar no SaveSystem: {e.Message}");
            }
        }

        // 4. Carrega a cena Gameplay 2
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
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