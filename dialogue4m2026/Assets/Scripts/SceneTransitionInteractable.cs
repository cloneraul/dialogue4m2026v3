using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTransitionInteractable : MonoBehaviour
{
    [Header("Configurações de Cena")]
    [Tooltip("Nome exato da cena de destino no Build Settings")]
    [SerializeField] private string targetSceneName = "Gameplay 2";

    [Header("Posição do Botão 'E'")]
    [Tooltip("Deslocamento de altura para o botão 'E' flutuar em cima da porta")]
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
        // Se estiver perto da porta e pressionar 'E', troca de cena
        if (isPlayerInside && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ChangeScene();
        }
    }

    private void ChangeScene()
    {
        // Oculta o botão de interação antes de mudar de cena
        NotifyInteractable(false);

        Debug.Log($"[Porta] Interação ativada! Transicionando para a cena: {targetSceneName}");

        // Atualiza o nível do jogador no SaveSystem para salvar que ele avançou de fase (Fase 2)
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetPlayerLevel(2, 0);
            SaveSystem.Instance.SaveDataInFile(0);
        }

        // Carrega a nova cena usando o GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
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