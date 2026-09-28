using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int value = 1;

    [Header("Save Settings")]
    [Tooltip("Se deixado em branco, o script usará automaticamente o nome do objeto na Hierarchy")]
    [SerializeField] private string coinID;

    private bool collected;

    public int Value => value;
    public string CoinID => coinID;

    private void Awake()
    {
        // Se a variável 'coinID' estiver em branco no Inspector,
        // preenche automaticamente com o nome exato do GameObject na Hierarchy!
        if (string.IsNullOrEmpty(coinID))
        {
            coinID = gameObject.name;
        }
    }

    private IEnumerator Start()
    {
        // Aguarda 2 frames para garantir que o CoinManager já carregou os dados do slot
        yield return null;
        yield return new WaitForEndOfFrame();

        CheckIfAlreadyCollected();
    }

    private void CheckIfAlreadyCollected()
    {
        if (CoinManager.Instance != null && CoinManager.Instance.IsCoinCollected(coinID))
        {
            collected = true;
            gameObject.SetActive(false); // Esconde a moeda pois já foi pega
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