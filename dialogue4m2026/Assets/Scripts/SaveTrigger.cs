using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 pos = transform.position;
            string currentScene = SceneManager.GetActiveScene().name;
            int levelNum = currentScene.EndsWith("2") ? 2 : 1;
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);

            // 1. GRAVA SEMPRE NO SLOT 0 (Autosave / Progresso Atual da Sessão)
            PlayerPrefs.SetFloat("Slot0_PosX", pos.x);
            PlayerPrefs.SetFloat("Slot0_PosY", pos.y);
            PlayerPrefs.SetFloat("Slot0_PosZ", pos.z);
            PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
            PlayerPrefs.SetInt("Slot0_Level", levelNum);
            PlayerPrefs.SetString("Slot0_Scene", currentScene);

            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.SaveCheckpointCoins(0);
            }

            // 2. SE HOUVER UM SLOT PERMANENTE ATIVO (1, 2 ou 3), GRAVA NELE TAMBÉM
            if (activeSlot > 0)
            {
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosX", pos.x);
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosY", pos.y);
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosZ", pos.z);
                PlayerPrefs.SetInt($"Slot{activeSlot}_HasCheckpoint", 1);
                PlayerPrefs.SetInt($"Slot{activeSlot}_Level", levelNum);
                PlayerPrefs.SetString($"Slot{activeSlot}_Scene", currentScene);

                if (CoinManager.Instance != null)
                {
                    CoinManager.Instance.SaveCheckpointCoins(activeSlot);
                }

                if (SaveSystem.Instance != null)
                {
                    SaveData data = new SaveData();
                    data.SetPlayerPosition(pos);
                    data.currentSceneName = currentScene;
                    if (CoinManager.Instance != null)
                    {
                        data.totalCoins = CoinManager.Instance.CurrentCoins;
                    }
                    SaveSystem.Instance.SetSaveData(data, activeSlot);
                    SaveSystem.Instance.SaveDataInFile(activeSlot);
                }
            }

            PlayerPrefs.Save();
            Debug.Log($"[SaveTrigger] Checkpoint salvo no Slot 0 (Autosave) e Slot Ativo ({activeSlot}) na posição: {pos}");
        }
    }
}