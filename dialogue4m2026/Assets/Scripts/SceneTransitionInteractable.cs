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

    private bool isPlayerInside = false;

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

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        // 1. Reseta as moedas para a nova fase
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 2. Limpa dados de posição antiga para o jogador nascer no Spawn inicial da nova fase
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_HasCheckpoint");
        PlayerPrefs.DeleteKey($"Slot0_HasCheckpoint");
        PlayerPrefs.SetString($"Slot{activeSlot}_Scene", targetSceneName);
        PlayerPrefs.SetString($"Slot0_Scene", targetSceneName);
        PlayerPrefs.Save();

        // 3. Atualiza o SaveSystem para a nova cena no Slot 0 (Autosave) e Slot Ativo
        if (SaveSystem.Instance != null)
        {
            SaveData data = new SaveData { currentSceneName = targetSceneName };
            SaveSystem.Instance.SetSaveData(data, 0);
            SaveSystem.Instance.SaveDataInFile(0);

            if (activeSlot > 0)
            {
                SaveSystem.Instance.SetSaveData(data, activeSlot);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
        }

        // 4. Transição de cena
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