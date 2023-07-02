using UnityEngine;

public class Rotator : MonoBehaviour
{
    public bool xRot;
    public bool yRot;
    public bool zRot;

    public float rotationSpeed = 60;

    private float xRotSpeed = 0f;
    private float yRotSpeed = 0f;
    private float zRotSpeed = 0f;

    private void Start()
    {
        if (xRot)
            xRotSpeed = rotationSpeed;
        else
            xRotSpeed = 0f;

        if (yRot)
            yRotSpeed = rotationSpeed;
        else
            yRotSpeed = 0f;

        if (zRot)
            zRotSpeed = rotationSpeed;
        else
            zRotSpeed = 0f;
    }

    private void Update()
    {
        transform.Rotate(xRotSpeed * Time.deltaTime,
            yRotSpeed * Time.deltaTime, zRotSpeed * Time.deltaTime);
    }
}