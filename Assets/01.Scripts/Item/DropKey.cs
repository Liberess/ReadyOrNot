using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropKey : MonoBehaviour
{
    [SerializeField] private GameObject key;

    private void Start()
    {
        if(key == null)
            key = Resources.Load("KeyItem") as GameObject;
    }

    public void CreateKey()
    {
        var pos = new Vector3(transform.position.x, 1.2f, transform.position.z);
        Instantiate(key, pos, Quaternion.identity).GetComponent<Key>();
    }
}