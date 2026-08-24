using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private void Update()
    {
        if (CoinManager.Instance == null)
            return;

        coinText.text = "Moedas: " + CoinManager.Instance.CurrentCoins;
    }
}