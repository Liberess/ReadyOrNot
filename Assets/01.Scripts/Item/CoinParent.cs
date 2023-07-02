using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinParent : MonoBehaviour
{
    [SerializeField] private Coin[] coins = new Coin[5];

    private void Start()
    {
        foreach (var coin in coins)
            coin.Burst();
    }
}