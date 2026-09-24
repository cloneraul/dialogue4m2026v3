using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SaveTrigger : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Dispara o checkpoint automático assim que o Player entra na área pela primeira vez
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            ExecuteAutomaticSave(other.transform.position);
        }
    }

    private void ExecuteAutomaticSave(Vector3 playerPosition)
    {
        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0); // 0 = Autosave, 1-3 = Slot Manual Ativo
        string currentScene = SceneManager.GetActiveScene().name;

        // 1. Grava os dados no PlayerPrefs para o Slot 0 (Autosave) e para o Slot Ativo
        SaveToPlayerPrefs(0, playerPosition, currentScene);
        if (activeSlot > 0)
        {
            SaveToPlayerPrefs(activeSlot, playerPosition, currentScene);
        }

        // 2. Grava as moedas coletadas até este checkpoint
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SaveCheckpointCoins(0);
            if (activeSlot > 0)
            {
                CoinManager.Instance.SaveCheckpointCoins(activeSlot);
            }
        }

        PlayerPrefs.Save();

        // 3. Atualiza o arquivo físico encriptado no SaveSystem
        if (SaveSystem.Instance != null)
        {
            SaveData data = SaveSystem.Instance.GetSaveData(0) ?? new SaveData();
            data.SetPlayerPosition(playerPosition);
            data.currentSceneName = currentScene;

            if (CoinManager.Instance != null)
            {
                data.totalCoins = CoinManager.Instance.GetCheckpointCoins(0);
            }

            // Grava no Slot 0 (Autosave)
            SaveSystem.Instance.SetSaveData(data, 0);
            SaveSystem.Instance.SaveDataInFile(0);

            // Replicando no slot ativo se houver
            if (activeSlot > 0)
            {
                SaveSystem.Instance.SetSaveData(data, activeSlot);
                SaveSystem.Instance.SaveDataInFile(activeSlot);
            }
        }

        Debug.Log($"[SaveTrigger Automático] Checkpoint alcançado! Posição salva: {playerPosition} | Slot Ativo: {activeSlot}");
    }

    private void SaveToPlayerPrefs(int slot, Vector3 pos, string sceneName)
    {
        PlayerPrefs.SetFloat($"Slot{slot}_PosX", pos.x);
        PlayerPrefs.SetFloat($"Slot{slot}_PosY", pos.y);
        PlayerPrefs.SetFloat($"Slot{slot}_PosZ", pos.z);
        PlayerPrefs.SetInt($"Slot{slot}_HasCheckpoint", 1);
        PlayerPrefs.SetString($"Slot{slot}_Scene", sceneName);
    }
}