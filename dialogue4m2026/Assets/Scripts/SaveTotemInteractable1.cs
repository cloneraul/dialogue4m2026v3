using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveTotemInteractable1 : MonoBehaviour
{
    [Header("Configurações do Save")]
    [Tooltip("Slot de destino para o salvamento manual (Slot 2)")]
    [SerializeField] private int targetSlot = 2;

    [Header("Posição do Botão 'E'")]
    [Tooltip("Deslocamento de altura para o botão 'E' flutuar em cima do Totem")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 1.5f, 0);

    private bool isPlayerInside = false;
    private Transform playerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerTransform = other.transform;

            // 1. Envia a posição 3D do Totem para o botão "E" se posicionar na tela
            NotifyInteractPosition(transform.position + buttonOffset);

            // 2. Notifica o sistema para mostrar o botão "E" (ativa visibilidade)
            NotifyInteractable(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerTransform = null;

            // Oculta o botão "E" ao se afastar do Totem
            NotifyInteractable(false);
        }
    }

    private void Update()
    {
        // Ao estar próximo e pressionar 'E', salva no Slot 2
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteSave();
        }
    }

    private void ExecuteSave()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;

            // Grava a posição no Slot 2 e espelha no Slot 0 (sessão ativa)
            SavePositionForSlot(targetSlot, playerPos);
            SavePositionForSlot(0, playerPos);

            // Grava os dados de progresso no SaveSystem
            if (SaveSystem.Instance != null)
            {
                int currentLevel = SaveSystem.Instance.GetPlayerLevel(0);
                SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            Debug.Log($"[Totem Save] Jogo salvo no Slot {targetSlot} com sucesso! Posição gravada: {playerPos}");
        }
    }

    private void SavePositionForSlot(int slotIndex, Vector3 pos)
    {
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosX", pos.x);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosY", pos.y);
        PlayerPrefs.SetFloat($"Slot{slotIndex}_PosZ", pos.z);
        PlayerPrefs.SetInt($"Slot{slotIndex}_HasCheckpoint", 1);
        PlayerPrefs.Save();
    }

    // --- MÉTODOS AUXILIARES PARA INVOCAR EVENTOS DO INTERACTOM ---

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