using UnityEngine;

public class CoinDropper : MonoBehaviour
{
    [SerializeField] private Coin coinPrefab;

    [Header("Amount (random between min and max, inclusive)")]
    [SerializeField] private int minCoins = 1;
    [SerializeField] private int maxCoins = 3;

    [Header("Pop")]
    [SerializeField] private float popHorizontal = 2.5f;
    [SerializeField] private float popUpMin = 4f;
    [SerializeField] private float popUpMax = 7f;

    private void OnValidate()
    {
        minCoins = Mathf.Max(0, minCoins);
        maxCoins = Mathf.Max(minCoins, maxCoins);
    }

    public void Drop()
    {
        int amount = Random.Range(minCoins, maxCoins + 1); // int max is exclusive

        for (int i = 0; i < amount; i++)
        {
            Coin coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            coin.Launch(new Vector3(
                Random.Range(-popHorizontal, popHorizontal),
                Random.Range(popUpMin, popUpMax),
                0f));
        }
    }
}