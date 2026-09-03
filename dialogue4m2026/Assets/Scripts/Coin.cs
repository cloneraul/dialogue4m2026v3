using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int value = 1;

    [Header("Save Settings")]
    [Tooltip("Dê um ID único para cada moeda do mapa (ex: Coin_01, Coin_02)")]
    [SerializeField] private string coinID;

    private bool collected = false;

    public int Value => value;
    public string CoinID => coinID;

    private void Start()
    {
        // Se a moeda já tiver sido salva como coletada no Checkpoint, desativa ela do cenário
        if (CoinManager.Instance != null && CoinManager.Instance.IsCoinCollected(coinID))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.CollectCoin(this);
        }

        gameObject.SetActive(false);
    }
}