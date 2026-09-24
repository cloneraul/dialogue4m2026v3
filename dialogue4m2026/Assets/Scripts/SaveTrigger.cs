using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 pos = other.transform.position;
            string currentScene = SceneManager.GetActiveScene().name;
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", -1);

            // 1. Atualiza temporariamente na memória (Slot 0)
            PlayerPrefs.SetFloat("Slot0_PosX", pos.x);
            PlayerPrefs.SetFloat("Slot0_PosY", pos.y);
            PlayerPrefs.SetFloat("Slot0_PosZ", pos.z);
            PlayerPrefs.SetInt("Slot0_HasCheckpoint", 1);
            PlayerPrefs.Save();

            // 2. Se a sessão JÁ estiver vinculada a um slot fixo (1, 2 ou 3), atualiza ele automaticamente
            if (activeSlot > 0)
            {
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosX", pos.x);
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosY", pos.y);
                PlayerPrefs.SetFloat($"Slot{activeSlot}_PosZ", pos.z);
                PlayerPrefs.SetInt($"Slot{activeSlot}_HasCheckpoint", 1);
                PlayerPrefs.SetString($"Slot{activeSlot}_Scene", currentScene);
                PlayerPrefs.Save();

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

                Debug.Log($"[SaveTrigger] Checkpoint atualizado com sucesso no Slot {activeSlot}!");
            }
            else
            {
                Debug.Log("[SaveTrigger] Checkpoint alcançado (registrado na memória temporária).");
            }
        }
    }
}