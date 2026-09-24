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

        // 1. Zera o contador e a lista de moedas na memória do CoinManager
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ResetCoinsForNewLevel();
        }

        // 2. Apaga as moedas salvas no disco para o Slot Ativo e Slot 0
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_Coins");
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_CoinIDs");
        PlayerPrefs.DeleteKey("Slot0_Coins");
        PlayerPrefs.DeleteKey("Slot0_CoinIDs");

        // 3. Apaga a posição de checkpoint antiga da Fase 1 (para nascer no Spawn inicial da Fase 2)
        PlayerPrefs.DeleteKey($"Slot{activeSlot}_HasCheckpoint");
        PlayerPrefs.DeleteKey("Slot0_HasCheckpoint");

        // 4. Atualiza a indicação do nível para a Fase 2
        PlayerPrefs.SetInt($"Slot{activeSlot}_Level", 2);
        PlayerPrefs.SetInt("Slot0_Level", 2);
        PlayerPrefs.SetString($"Slot{activeSlot}_Scene", targetSceneName);
        PlayerPrefs.SetString("Slot0_Scene", targetSceneName);

        PlayerPrefs.Save();

        // 5. Atualiza o SaveSystem para indicar a nova cena
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

        Debug.Log($"[SceneTransition] Mudando para {targetSceneName}. Dados de moedas e checkpoint antigos apagados!");

        // 6. Realiza a transição de cena
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