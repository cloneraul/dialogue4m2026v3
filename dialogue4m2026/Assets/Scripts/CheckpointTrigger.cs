using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveTrigger : MonoBehaviour
{
    public enum SaveType { CheckpointAutomatico, TotemInterativo }

    [Header("Tipo de Salvamento")]
    [SerializeField] private SaveType saveType = SaveType.CheckpointAutomatico;

    [Header("Configurações da Fase")]
    [Tooltip("1 para Gameplay (Fase 1), 2 para Gameplay 2 (Fase 2)")]
    [SerializeField] private int currentLevel = 1;

    [Header("Ajustes de Spawn e Botão 'E'")]
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);

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

    /// <summary>
    /// Salva o progresso no Slot que o jogador escolheu ao iniciar/carregar
    /// </summary>
    public void ExecuteSave()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 1);

        // Se o jogador iniciou em Novo Jogo (Slot 0), o primeiro Save fixa automaticamente no Slot 1 como padrão (ou no slot que você definir)
        if (activeSlot == 0)
        {
            activeSlot = 1;
            PlayerPrefs.SetInt("CurrentActiveSlot", activeSlot);
        }

        Vector3 centerPos = transform.position + spawnOffset;

        // 1. Grava a Posição e Nível no Slot Ativo
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosX", centerPos.x);
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosY", centerPos.y);
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosZ", centerPos.z);
        PlayerPrefs.SetInt($"Slot{activeSlot}_HasCheckpoint", 1);
        PlayerPrefs.SetInt($"Slot{activeSlot}_Level", currentLevel);

        // 2. Grava as Moedas no Slot Ativo
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(activeSlot);
        }

        PlayerPrefs.Save();

        // 3. Atualiza o SaveSystem
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SetPlayerLevel(currentLevel, activeSlot);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Erro ao gravar arquivo: {e.Message}");
            }
        }

        Debug.Log($"[SaveTrigger] Progresso salvo com SUCESSO no Slot {activeSlot}! Posição: {centerPos} | Fase: {currentLevel}");
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