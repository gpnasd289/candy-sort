using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    [Header("Level Configuration")]
    public LevelConfig currentLevel;
    public List<LevelConfig> allLevels = new List<LevelConfig>();

    [Header("Prefab References")]
    public GameObject blockPrefabStandard;
    public GameObject blockPrefabLarge;
    public GameObject blockPrefabSmall;
    public GameObject blockPrefabWall;

    private int currentLevelIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
        else if (allLevels.Count > 0)
        {
            LoadLevel(allLevels[0]);
        }
        else
        {
            Debug.LogError("No levels configured in LevelManager!");
        }
    }

    public void LoadLevel(LevelConfig config)
    {
        if (config == null)
        {
            Debug.LogError("Cannot load null level config!");
            return;
        }

        currentLevel = config;

        // Validate level
        if (!config.ValidateLevel())
        {
            Debug.LogWarning($"Level {config.levelNumber} has validation warnings!");
        }

        // Clear existing level
        ClearCurrentLevel();

        // Load new level
        StartCoroutine(LoadLevelCoroutine(config));
    }

    private System.Collections.IEnumerator LoadLevelCoroutine(LevelConfig config)
    {
        yield return new WaitForSeconds(0.5f);

        // Create tubes
        foreach (var tubeData in config.tubes)
        {
            CreateTubeFromData(tubeData);
        }

        // Create blocks
        foreach (var blockData in config.blocks)
        {
            CreateBlockFromData(blockData);
        }

        Debug.Log($"Level {config.levelNumber} loaded: {config.tubes.Count} tubes, {config.GetTotalLayerCount()} layers");
    }

    private void CreateTubeFromData(LevelConfig.TubeData tubeData)
    {
        Tube tube = GameManager.Instance.CreateTube(tubeData.position);

        // Create layers
        foreach (var layerData in tubeData.layers)
        {
            CandyLayer layer = tube.CreateLayer(layerData.color, layerData.isMystery);
            tube.AddLayer(layer);
        }
    }

    private void CreateBlockFromData(LevelConfig.BlockData blockData)
    {
        GameObject blockPrefab = GetBlockPrefab(blockData.blockType);

        if (blockPrefab == null)
        {
            Debug.LogWarning($"No prefab found for block type: {blockData.blockType}");
            return;
        }

        GameObject block = Instantiate(blockPrefab, blockData.position, Quaternion.identity);
        block.transform.localScale = blockData.scale;
        block.tag = "Block";
    }

    private GameObject GetBlockPrefab(LevelConfig.BlockType blockType)
    {
        switch (blockType)
        {
            case LevelConfig.BlockType.Standard:
                return blockPrefabStandard;
            case LevelConfig.BlockType.Large:
                return blockPrefabLarge;
            case LevelConfig.BlockType.Small:
                return blockPrefabSmall;
            case LevelConfig.BlockType.Wall:
                return blockPrefabWall;
            default:
                return blockPrefabStandard;
        }
    }

    private void ClearCurrentLevel()
    {
        // Clear all tubes
        if (GameManager.Instance != null)
        {
            foreach (Tube tube in GameManager.Instance.tubes)
            {
                if (tube != null)
                {
                    Destroy(tube.gameObject);
                }
            }
            GameManager.Instance.tubes.Clear();
        }

        // Clear all blocks
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }
    }

    public void LoadNextLevel()
    {
        if (allLevels.Count == 0)
        {
            Debug.LogWarning("No levels in allLevels list!");
            return;
        }

        currentLevelIndex++;

        if (currentLevelIndex >= allLevels.Count)
        {
            Debug.Log("All levels completed!");
            currentLevelIndex = 0; // Loop back to first level
        }

        LoadLevel(allLevels[currentLevelIndex]);
    }

    public void LoadPreviousLevel()
    {
        if (allLevels.Count == 0)
            return;

        currentLevelIndex--;

        if (currentLevelIndex < 0)
        {
            currentLevelIndex = allLevels.Count - 1;
        }

        LoadLevel(allLevels[currentLevelIndex]);
    }

    public void ReloadCurrentLevel()
    {
        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
    }

    public void LoadLevelByNumber(int levelNumber)
    {
        LevelConfig level = allLevels.Find(l => l.levelNumber == levelNumber);

        if (level != null)
        {
            currentLevelIndex = allLevels.IndexOf(level);
            LoadLevel(level);
        }
        else
        {
            Debug.LogWarning($"Level {levelNumber} not found!");
        }
    }

    public LevelConfig GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetCurrentLevelNumber()
    {
        return currentLevel != null ? currentLevel.levelNumber : 0;
    }

    public int GetTotalLevelCount()
    {
        return allLevels.Count;
    }
}