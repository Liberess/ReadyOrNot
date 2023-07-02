using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacterRotator : MonoBehaviour
{
    [SerializeField, Range(0.01f, 0.1f)] private float rotateSpeed = 0.05f;

    private void Update()
    {
        transform.Rotate(new Vector3(0f, -rotateSpeed, 0f));
    }
}