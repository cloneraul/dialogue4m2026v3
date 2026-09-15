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
        // Ao interagir com o totem via tecla 'E', salva a posição atual do jogador
        if (saveType == SaveType.TotemInterativo && isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExecuteSave();
        }
    }

    public void ExecuteSave()
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

        // Se for Novo Jogo (Slot 0), o progresso não é fixado em arquivo físico até o jogador escolher um slot no Pause
        if (activeSlot <= 0)
        {
            Debug.Log("[SaveTrigger] Checkpoint temporário alcançado. Escolha um Slot no menu Pause para fixar este progresso.");
            return;
        }

        // Captura a posição exata do Jogador na cena
        Vector3 savePosition = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            savePosition = player.transform.position;
        }

        // 1. Grava Posição e Fase no PlayerPrefs do Slot Ativo (1, 2 ou 3)
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosX", savePosition.x);
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosY", savePosition.y);
        PlayerPrefs.SetFloat($"Slot{activeSlot}_PosZ", savePosition.z);
        PlayerPrefs.SetInt($"Slot{activeSlot}_HasCheckpoint", 1);
        PlayerPrefs.SetInt($"Slot{activeSlot}_Level", currentLevel);

        // 2. Grava as Moedas no Slot Ativo
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(activeSlot);
        }

        PlayerPrefs.Save();

        // 3. Persiste no SaveSystem usando SEMPRE o índice 0 da lista em memória, gerando o arquivo individual do Slot (save1, save2, save3)
        if (SaveSystem.Instance != null)
        {
            try
            {
                SaveSystem.Instance.SetPlayerLevel(currentLevel, 0);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Erro ao gravar arquivo no Slot {activeSlot}: {e.Message}");
            }
        }

        Debug.Log($"[SaveTrigger] Progresso salvo com SUCESSO no Slot {activeSlot}! Posição do Jogador: {savePosition} | Fase: {currentLevel}");
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