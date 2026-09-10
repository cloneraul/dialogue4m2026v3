using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveTotemInteractable1 : MonoBehaviour
{
    [Header("Configurações do Save")]
    [Tooltip("Slot de destino para o salvamento manual (Slot 2)")]
    [SerializeField] private int targetSlot = 2;

    [Header("Posição do Botão 'E' e Spawn")]
    [Tooltip("Deslocamento de altura para o botão 'E' flutuar em cima do Totem")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 1.5f, 0);

    [Tooltip("Elevação leve para o jogador nascer no centro do totem sem prender no chão")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

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
            ExecuteSave();
        }
    }

    private void ExecuteSave()
    {
        // Pega a posição central do próprio Totem
        Vector3 centerPos = transform.position + spawnOffset;

        // 1. Grava a posição no Slot Alvo e espelha no Slot 0
        SavePositionForSlot(targetSlot, centerPos);
        SavePositionForSlot(0, centerPos);

        // 2. Grava as moedas no Slot Alvo e espelha no Slot 0
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(targetSlot);
            CoinManager.Instance.SaveCheckpointCoins(0);
        }

        // 3. Grava o progresso no SaveSystem
        if (SaveSystem.Instance != null)
        {
            try
            {
                int currentLevel = SaveSystem.Instance.GetPlayerLevel(0);
                SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Gravação interna ignorada: {e.Message}");
            }
        }

        Debug.Log($"[Totem Save] Jogo e moedas salvos no Slot {targetSlot} e Slot 0! Posição CENTRAL: {centerPos}");
    }

    private void SavePositionForSlot(int slotIndex, Vector3 pos)
    {
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosX", pos.x);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosY", pos.y);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosZ", pos.z);
        PlayerPrefs.SetInt($"Slot{slotIndex}_HasCheckpoint", 1);
        PlayerPrefs.Save();
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