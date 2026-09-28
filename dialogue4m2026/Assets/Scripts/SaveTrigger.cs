using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    [Header("Identificação do Checkpoint")]
    [Tooltip("ID ÚNICO para este checkpoint na cena. Ex: Checkpoint_F1_01")]
    [SerializeField] private string checkpointID;

    private void Awake()
    {
        // Se ficar vazio no Inspector, usa o nome do GameObject na Hierarchy
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = gameObject.name;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        int activeSlot = PlayerPrefs.GetInt("CurrentActiveSlot", 0);
        string usedCheckpoints = PlayerPrefs.GetString($"Slot{activeSlot}_UsedCheckpoints", "");

        // Se este checkpoint JÁ FOI UTILIZADO neste Slot, não salva mais nada
        if (usedCheckpoints.Contains(checkpointID))
        {
            Debug.Log($"[SaveTrigger] Checkpoint '{checkpointID}' já foi utilizado no Slot {activeSlot}. Ignorando...");
            return;
        }

        // --- EXECUTA O SALVAMENTO ---
        Vector3 pos = transform.position;
        string currentScene = SceneManager.GetActiveScene().name;
        int levelNum = currentScene.EndsWith("2") ? 2 : 1;

        // 1. Grava no Slot 0 (Autosave temporário da sessão)
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

        // 2. Grava no Slot Ativo (caso exista um slot 1, 2 ou 3 selecionado)
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

            // Atualiza o ficheiro DAT (save_1.dat, save_2.dat, etc.)
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

        // 3. Marca este Checkpoint como UTILIZADO para não voltar a funcionar neste Slot
        usedCheckpoints += checkpointID + ",";
        PlayerPrefs.SetString($"Slot{activeSlot}_UsedCheckpoints", usedCheckpoints);

        PlayerPrefs.Save();
        Debug.Log($"[SaveTrigger] Checkpoint '{checkpointID}' consumido com SUCESSO no Slot {activeSlot}!");
    }
}