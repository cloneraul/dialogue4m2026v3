using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject slotsPanel;

    [Header("Botões do Menu Principal")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Botões dos Slots")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button backButton;

    private void Start()
    {
        ShowMainPanel();
        CheckAutoSaveSlot();
        SetupButtonListeners();
    }

    private void CheckAutoSaveSlot()
    {
        if (continueButton != null)
        {
            bool hasSaveFile = (SaveSystem.Instance != null) && SaveSystem.Instance.LoadDataInFile(0);
            bool hasPositionSaved = PlayerPrefs.GetInt("Slot0_HasCheckpoint", 0) == 1;

            bool canContinue = hasSaveFile && hasPositionSaved;
            continueButton.gameObject.SetActive(canContinue);
        }
    }

    private void SetupButtonListeners()
    {
        if (continueButton != null) continueButton.onClick.AddListener(OnClick_Continue);
        if (newGameButton != null) newGameButton.onClick.AddListener(OnClick_NewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(ShowSlotsPanel);
        if (quitButton != null) quitButton.onClick.AddListener(OnClick_Quit);

        if (slot1Button != null) slot1Button.onClick.AddListener(() => OnClick_SelectSlot(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => OnClick_SelectSlot(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => OnClick_SelectSlot(3));
        if (backButton != null) backButton.onClick.AddListener(ShowMainPanel);
    }

    private void OnClick_Continue()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.LoadDataInFile(0))
        {
            LoadSavedPhase(0);
        }
    }

    private void OnClick_NewGame()
    {
        ClearSlotPosition(0);
        ClearSlotPosition(1);

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SetPlayerLevel(1, 0);
            SaveSystem.Instance.SaveDataInFile(0);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene("Gameplay");
        }
    }

    private void OnClick_SelectSlot(int slotIndex)
    {
        // Confere se o Slot selecionado tem checkpoint registrado
        if (PlayerPrefs.GetInt($"Slot{slotIndex}_HasCheckpoint", 0) == 1)
        {
            // Copia a posição salva do Slot selecionado para o Slot 0
            CopyPositionFromSlotToSlot(slotIndex, 0);

            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SetPlayerLevel(1, 0);
                SaveSystem.Instance.SaveDataInFile(0);
            }

            LoadSavedPhase(0);
        }
        else
        {
            Debug.LogWarning($"O Slot {slotIndex} está vazio!");
        }
    }

    private void CopyPositionFromSlotToSlot(int fromSlot, int toSlot)
    {
        float x = PlayerPrefs.GetFloat($"Slot{fromSlot}_PosX");
        float y = PlayerPrefs.GetFloat($"Slot{fromSlot}_PosY");
        float z = PlayerPrefs.GetFloat($"Slot{fromSlot}_PosZ");

        PlayerPrefs.SetFloat($"Slot{toSlot}_PosX", x);
        PlayerPrefs.SetFloat($"Slot{toSlot}_PosY", y);
        PlayerPrefs.SetFloat($"Slot{toSlot}_PosZ", z);
        PlayerPrefs.SetInt($"Slot{toSlot}_HasCheckpoint", 1);
        PlayerPrefs.Save();
    }

    private void ClearSlotPosition(int slotIndex)
    {
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_HasCheckpoint");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosX");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosY");
        PlayerPrefs.DeleteKey($"Slot{slotIndex}_PosZ");
        PlayerPrefs.Save();
    }

    private void LoadSavedPhase(int slotIndex)
    {
        int phaseLevel = (SaveSystem.Instance != null) ? SaveSystem.Instance.GetPlayerLevel(0) : 1;
        string targetScene = (phaseLevel == 2) ? "Gameplay 2" : "Gameplay";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene(targetScene);
        }
    }

    private void OnClick_Quit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (slotsPanel != null) slotsPanel.SetActive(false);
    }

    public void ShowSlotsPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (slotsPanel != null) slotsPanel.SetActive(true);
    }
}