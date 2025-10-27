using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_", menuName = "Candy Sort/Level Config", order = 1)]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string levelName = "Level 1";
    [TextArea(2, 4)]
    public string levelDescription = "";

    [Header("Level Settings")]
    public int targetMoves = 0; // 0 = unlimited
    public float timeLimit = 0f; // 0 = unlimited

    [Header("Tube Configuration")]
    public List<TubeData> tubes = new List<TubeData>();

    [Header("Obstacles")]
    public List<BlockData> blocks = new List<BlockData>();

    [System.Serializable]
    public class TubeData
    {
        public Vector3 position = Vector3.zero;
        public List<LayerData> layers = new List<LayerData>();
    }

    [System.Serializable]
    public class LayerData
    {
        public CandyColor color = CandyColor.Red;
        public bool isMystery = false;
    }

    [System.Serializable]
    public class BlockData
    {
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public BlockType blockType = BlockType.Standard;
    }

    public enum BlockType
    {
        Standard,
        Large,
        Small,
        Wall
    }

    // Validation
    private void OnValidate()
    {
        // Ensure level number is positive
        if (levelNumber < 1)
            levelNumber = 1;

        // Update level name to match number
        if (string.IsNullOrEmpty(levelName) || levelName == "Level 1")
        {
            levelName = "Level " + levelNumber;
        }
    }

    // Helper methods
    public int GetTubeCount()
    {
        return tubes.Count;
    }

    public int GetTotalLayerCount()
    {
        int total = 0;
        foreach (var tube in tubes)
        {
            total += tube.layers.Count;
        }
        return total;
    }

    public int GetMysteryLayerCount()
    {
        int count = 0;
        foreach (var tube in tubes)
        {
            foreach (var layer in tube.layers)
            {
                if (layer.isMystery)
                    count++;
            }
        }
        return count;
    }

    public Dictionary<CandyColor, int> GetColorDistribution()
    {
        Dictionary<CandyColor, int> distribution = new Dictionary<CandyColor, int>();

        foreach (var tube in tubes)
        {
            foreach (var layer in tube.layers)
            {
                if (!distribution.ContainsKey(layer.color))
                {
                    distribution[layer.color] = 0;
                }
                distribution[layer.color]++;
            }
        }

        return distribution;
    }

    public bool ValidateLevel()
    {
        // Check if level has tubes
        if (tubes.Count == 0)
        {
            Debug.LogWarning($"Level {levelNumber}: No tubes configured!");
            return false;
        }

        // Check color distribution (each color should have enough layers to fill a tube)
        var colorDist = GetColorDistribution();
        foreach (var kvp in colorDist)
        {
            if (kvp.Value < 3)
            {
                Debug.LogWarning($"Level {levelNumber}: Color {kvp.Key} has only {kvp.Value} layers (too few to be solvable)");
            }
        }

        return true;
    }
}