using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveTotemInteractableSlot3 : MonoBehaviour
{
    [Header("Configurações do Save")]
    [Tooltip("Slot de destino para o salvamento manual (Slot 3)")]
    [SerializeField] private int targetSlot = 3;

    [Tooltip("Nível da fase atual (ex: 2 para Gameplay 2)")]
    [SerializeField] private int currentLevel = 2;

    [Header("Posição do Botão 'E'")]
    [Tooltip("Deslocamento de altura para o botão 'E' flutuar em cima do Totem")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0);

    private bool isPlayerInside = false;
    private Transform playerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerTransform = other.transform;

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
            playerTransform = null;

            // Oculta o botão "E" ao se afastar
            NotifyInteractable(false);
        }
    }

    private void Update()
    {
        // Se estiver perto do Totem e pressionar 'E', executa o save no Slot 3
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

            // 1. Grava as coordenadas nos PlayerPrefs do Slot 3 e espelha no Slot 0 (sessão ativa)
            SavePositionForSlot(targetSlot, playerPos);
            SavePositionForSlot(0, playerPos);

            // 2. Grava o progresso do Nível 2 via SaveSystem no Slot 0
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            Debug.Log($"[Totem Gameplay 2] Jogo salvo com SUCESSO no Slot {targetSlot} (Fase {currentLevel}) na posição: {playerPos}");
        }
        else
        {
            Debug.LogWarning("[Totem Gameplay 2] Não foi possível encontrar a referência do Jogador!");
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