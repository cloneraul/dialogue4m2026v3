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
            int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", -1);

            // Se estiver em Novo Jogo (slot -1), usa o Slot 0 temporário. Se for um slot escolhido, salva só nele.
            int targetSlot = (activeSlot > 0) ? activeSlot : 0;

            // Grava Posição e Dados no Slot Alvo
            PlayerPrefs.SetFloat($"Slot{targetSlot}_PosX", pos.x);
            PlayerPrefs.SetFloat($"Slot{targetSlot}_PosY", pos.y);
            PlayerPrefs.SetFloat($"Slot{targetSlot}_PosZ", pos.z);
            PlayerPrefs.SetInt($"Slot{targetSlot}_HasCheckpoint", 1);
            PlayerPrefs.SetInt($"Slot{targetSlot}_Level", levelNum);
            PlayerPrefs.SetString($"Slot{targetSlot}_Scene", currentScene);

            // Grava Moedas no Slot Alvo
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.SaveCheckpointCoins(targetSlot);
            }

            // Se for um Slot permanente (1, 2 ou 3), sincroniza com o SaveSystem de arquivo
            if (targetSlot > 0 && SaveSystem.Instance != null)
            {
                SaveData data = new SaveData();
                data.SetPlayerPosition(pos);
                data.currentSceneName = currentScene;
                if (CoinManager.Instance != null)
                {
                    data.totalCoins = CoinManager.Instance.CurrentCoins;
                }
                SaveSystem.Instance.SetSaveData(data, targetSlot);
                SaveSystem.Instance.SaveDataInFile(targetSlot);
            }

            PlayerPrefs.Save();
            Debug.Log($"[SaveTrigger] Checkpoint salvo com SUCESSO no Slot {targetSlot}! Posição: {pos}");
        }
    }
}