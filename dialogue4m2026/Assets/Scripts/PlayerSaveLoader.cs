using UnityEngine;

public class PlayerSaveLoader : MonoBehaviour
{
    private void Start()
    {
        // Verifica se existe algum save no Slot 0 (Autosave)
        string json = PlayerPrefs.GetString("SaveSlot_0", "");

        if (!string.IsNullOrEmpty(json))
        {
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Desativa o CharacterController/Rigidbody temporariamente para mover sem conflito de física
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Move o jogador para a posição gravada no Checkpoint
            transform.position = data.GetPlayerPosition();

            if (cc != null) cc.enabled = true;

            Debug.Log($"Jogador posicionado no Checkpoint: {transform.position}");
        }
    }
}