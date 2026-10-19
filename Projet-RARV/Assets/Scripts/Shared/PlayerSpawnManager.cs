using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public Transform GetSpawnPoint(int index)
    {
        if (index < 0 || index >= spawnPoints.Length)
        {
            Debug.LogError("Invalid spawn point index!");
            return null;
        }

        return spawnPoints[index];
    }

    public int SpawnCount => spawnPoints.Length;
}