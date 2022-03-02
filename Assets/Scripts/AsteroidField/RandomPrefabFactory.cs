using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class RandomPrefabFactory : ScriptableObject
{
    [SerializeField] private RandomPrefabEntry[] weightedPrefabs;
    [SerializeField][HideInInspector] private int totalWeight;

    public GameObject GetRandomPrefab()
    {
        var number = Random.Range(0, totalWeight);
        foreach (var weightedPrefab in weightedPrefabs)
        {
            if (number < weightedPrefab.weight)
            {
                return weightedPrefab.prefab;
            }

            number -= weightedPrefab.weight;
        }

        return null;
    }
    
    private void OnValidate()
    {
        totalWeight = weightedPrefabs.Sum(e => e.weight);
    }
}

[System.Serializable]
public struct RandomPrefabEntry
{
    public GameObject prefab;
    [Range(1, 100)]
    public int weight;
}