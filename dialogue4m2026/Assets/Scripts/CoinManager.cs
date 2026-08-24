using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private int currentCoins = 0;

    public int CurrentCoins => currentCoins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CollectCoin(Coin coin)
    {
        if (coin == null)
            return;

        currentCoins += coin.Value;

        Debug.Log("Moeda coletada! Total: " + currentCoins);
    }

    public void ResetCoins()
    {
        currentCoins = 0;

        Debug.Log("Moedas resetadas.");
    }
}