using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Identificador do Checkpoint")]
    [SerializeField] private string checkpointID;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se quem passou pelo trigger foi o Jogador
        if (other.CompareTag("Player") && !activated)
        {
            activated = true;

            // Salva a posição exata do jogador e grava no Slot 0 (Autosave)
            SaveData data = new SaveData();
            data.SetPlayerPosition(other.transform.position);
            data.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // Salva o JSON no Slot 0
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("SaveSlot_0", json);
            PlayerPrefs.Save();

            Debug.Log($"Checkpoint [Slot 0] salvo na posição: {other.transform.position}");
        }
    }
}