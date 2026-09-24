using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int value = 1;

    [Header("Save Settings")]
    [Tooltip("Dê um ID único para cada moeda do mapa (ex: Coin_01, Coin_02)")]
    [SerializeField] private string coinID;

    private bool collected;

    public int Value => value;
    public string CoinID => coinID;

    private IEnumerator Start()
    {
        // Aguarda a sincronização dos Singletons no carregamento da cena
        yield return null;

        if (CoinManager.Instance != null && CoinManager.Instance.IsCoinCollected(coinID))
        {
            // Se a moeda já foi salva como coletada no Slot carregado, oculta da fase
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