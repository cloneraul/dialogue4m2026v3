using System;
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
        // Garante a referência do jogador caso o playerTransform esteja nulo
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;

            // 1. Grava as coordenadas X, Y, Z no PlayerPrefs para o Slot Alvo e para o Slot 0
            SavePositionForSlot(targetSlot, playerPos);
            SavePositionForSlot(0, playerPos);

            // 2. Grava as moedas no Slot Alvo e no Slot 0
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.SaveCheckpointCoins(targetSlot);
                CoinManager.Instance.SaveCheckpointCoins(0);
            }

            // 3. Grava o nível no PlayerPrefs diretamente (Garantia de backup)
            PlayerPrefs.SetInt($"Slot{targetSlot}_Level", currentLevel);
            PlayerPrefs.SetInt("Slot0_Level", currentLevel);

            // 4. Tenta salvar no SaveSystem de forma segura sem travar o jogo
            if (SaveSystem.Instance != null)
            {
                try
                {
                    // Tenta atualizar no Slot 0 (Sessão Ativa)
                    SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                    SaveSystem.Instance.SaveDataInFile(0);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Não foi possível salvar no SaveSystem interno, mas o PlayerPrefs gravou com sucesso. Detalhe: {e.Message}");
                }
            }

            Debug.Log($"[Totem Gameplay 2] Jogo e moedas salvos com SUCESSO no Slot {targetSlot}! Posição: {playerPos} | Fase: {currentLevel}");
        }
        else
        {
            Debug.LogError("[Totem Gameplay 2] ERRO CRÍTICO: Objeto com a Tag 'Player' não foi encontrado na cena!");
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