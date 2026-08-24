using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int value = 1;

    private bool collected = false;

    public int Value => value;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        collected = true;

        CoinManager.Instance.CollectCoin(this);

        gameObject.SetActive(false);
    }
}