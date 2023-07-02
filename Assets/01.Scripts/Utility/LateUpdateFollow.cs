using UnityEngine;

public class LateUpdateFollow : MonoBehaviour
{
    [SerializeField]
    private Transform targetToFollow;

    private void LateUpdate()
    {
        transform.position = targetToFollow.position;  
        transform.rotation = targetToFollow.rotation;  
    }
}