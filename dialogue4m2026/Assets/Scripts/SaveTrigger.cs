using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    public enum SaveType { CheckpointAutomatico, TotemInterativo }

    [Header("Tipo de Salvamento")]
    [SerializeField] private SaveType saveType = SaveType.CheckpointAutomatico;

    [Header("Ajustes da UI do Botão 'E'")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 1.5f, 0);

    private bool isPlayerInside = false;
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (saveType == SaveType.CheckpointAutomatico && !isActivated)
            {
                isActivated = true;
                ExecuteSave();
            }
            else if (saveType == SaveType.TotemInterativo)
            {
                NotifyInteractPosition(transform.position + buttonOffset);
                NotifyInteractable(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && saveType == SaveType.TotemInterativo)
        {
            isPlayerInside = false;
            NotifyInteractable(false);
        }
    }

    private void Update()
    {
        if (saveType == SaveType.TotemInterativo && isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteSave();
        }
    }

    public void ExecuteSave()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0); // 0 = Autosave/Temporário

        Vector3 savePosition = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            savePosition = player.transform.position;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        // 1. Grava dados no PlayerPrefs do Slot 0 (Autosave) e do Slot Ativo se houver
        SaveToPlayerPrefs(0, savePosition, currentScene);
        if (activeSlot > 0)
        {
            SaveToPlayerPrefs(activeSlot, savePosition, currentScene);
        }

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(0);
            if (activeSlot > 0) CoinManager.Instance.SaveCheckpointCoins(activeSlot);
        }

        PlayerPrefs.Save();

        // 2. Grava via SaveSystem no Slot 0 (Autosave) e replica no Slot Ativo
        if (SaveSystem.Instance != null)
        {
            SaveData data = SaveSystem.Instance.GetSaveData(0) ?? new SaveData();
            data.SetPlayerPosition(savePosition);
            data.currentSceneName = currentScene;

            if (CoinManager.Instance != null)
            {
                data.totalCoins = CoinManager.Instance.GetCheckpointCoins(0);
            }

            SaveSystem.Instance.SetSaveData(data, 0);
            SaveSystem.Instance.SaveDataInFile(0);

            if (activeSlot > 0)
            {
                SaveSystem.Instance.SetSaveData(data, activeSlot);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
        }

        Debug.Log($"[SaveTrigger] Checkpoint/Totem salvo com SUCESSO no Autosave (Slot 0) e Slot {activeSlot}!");
    }

    private void SaveToPlayerPrefs(int slot, Vector3 pos, string sceneName)
    {
        PlayerPrefs.SetFloat($"Slot{slot}_PosX", pos.x);
        PlayerPrefs.SetFloat($"Slot{slot}_PosY", pos.y);
        PlayerPrefs.SetFloat($"Slot{slot}_PosZ", pos.z);
        PlayerPrefs.SetInt($"Slot{slot}_HasCheckpoint", 1);
        PlayerPrefs.SetString($"Slot{slot}_Scene", sceneName);
    }

    // --- MÉTODOS AUXILIARES DO INTERACTOM ---

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