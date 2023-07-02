using UnityEngine;
using UnityEngine.AI;

public static class Utility
{
    public static Vector3 GetRandPointOnNavMesh(Vector3 center, float distance, int areaMask)
    {
        var randPos = Random.insideUnitSphere * distance + center;

        NavMeshHit hit;
        NavMesh.SamplePosition(randPos, out hit, distance, areaMask);

        return hit.position;
    }

    public static float GetRandNormalDistribution(float mean, float standard)
    {
        var x1 = Random.Range(0f, 1f);
        var x2 = Random.Range(0f, 1f);
        return mean + standard * (Mathf.Sqrt(-2.0f * Mathf.Log(x1)) * Mathf.Sin(2.0f * Mathf.PI * x2));
    }
}