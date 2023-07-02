using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ItemSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject[] items;

    private float lastSpawnTime = 0f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float destroyTime = 5f;

    private float timeBetSpawn;

    [SerializeField] private float timeBetSpawnMin = 7f;
    [SerializeField] private float timeBetSpawnMax = 2f;

    private void Start()
    {
        timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        lastSpawnTime = 0f;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if(Time.time >= lastSpawnTime + timeBetSpawn)
        {
            Spawn();
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
        }
    }

    private void Spawn()
    {
        var spawnPosition = Utility.GetRandPointOnNavMesh(Vector3.zero, maxDistance, NavMesh.AllAreas);

        spawnPosition += Vector3.up * 0.7f;

        GameObject tempItem = items[Random.Range(0, items.Length)];

        var item = PhotonNetwork.Instantiate(tempItem.name, spawnPosition, Quaternion.identity);
        if (tempItem.name == "AmmoPack")
            item.gameObject.transform.rotation = Quaternion.Euler(12.113f, 91f, 90f);
        StartCoroutine(DestroyItem(item, destroyTime));
    }

    private IEnumerator DestroyItem(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null)
            PhotonNetwork.Destroy(target);
    }
}