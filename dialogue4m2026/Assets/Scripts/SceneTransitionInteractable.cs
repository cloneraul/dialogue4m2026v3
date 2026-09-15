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
        // Pressionar 'E' perto do portal/objeto aciona a transição
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteTransitionToLevel2();
        }
    }

    private void ExecuteTransitionToLevel2()
    {
        // Oculta a indicação visual de interação
        NotifyInteractable(false);

        // Identifica o slot atual (0 = Novo Jogo temporário, 1-3 = Slots salvos)
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        Debug.Log($"[Transição] Trocando de fase no Slot {activeSlot}... Indo para: {targetSceneName}");

        // 1. Reseta as moedas coletadas na fase anterior
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 2. Limpa coordenadas legadas da Fase 1 do slot ativo para o player nascer no Spawn natural da Fase 2
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_HasCheckpoint");
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_PosX");
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_PosY");
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_PosZ");

        // Atualiza a fase gravada neste slot
        PlayerPrefs.SetInt($"Slot{activeSlot}_Level", nextLevelNumber);
        PlayerPrefs.Save();

        // 3. Atualiza o arquivo físico via SaveSystem (usando o índice 0 da memória)
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SetPlayerLevel(nextLevelNumber, 0);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SceneTransition] AVISO SaveSystem: {e.Message}");
            }
        }

        // 4. Carrega a nova cena
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