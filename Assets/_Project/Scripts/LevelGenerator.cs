using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    [Header("Level Configuration")]
    public bool levelDebug = false;
    public LevelConfig currentLevel;
    public int currentLevelID = 1;

    [Header("Prefab References")]
    public GameObject blockPrefabStandard;
    public GameObject blockPrefabLarge;
    public GameObject blockPrefabSmall;
    public GameObject blockPrefabWall;


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

    public void InitializeLevel()
    {
        currentLevelID = PlayerPrefs.GetInt("CurrentLevelID", 1);
        if (currentLevel != null && levelDebug)
        {
            LoadLevel(currentLevel);
        }
        else
        {
            Addressables.LoadAssetAsync<LevelConfig>($"Assets/_Project/SO/Level_{currentLevelID}.asset").Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    currentLevel = handle.Result;
                    LoadLevel(currentLevel);
                }
            };
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

    private IEnumerator LoadLevelCoroutine(LevelConfig config)
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

        // Initialize game state
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ResetMoveCount();
            UIManager.Instance.SetLevelName(config.levelName);
        }

        // Start timer if level has time limit
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.StartTimer(config.timeLimit);
        }

        Debug.Log($"Level {config.levelNumber} loaded: {config.tubes.Count} tubes, {config.GetTotalLayerCount()} layers");
    }

    private void CreateTubeFromData(LevelConfig.TubeData tubeData)
    {
        Tube tube = GameManager.Instance.CreateTube(tubeData.tubePrefab, tubeData.position);

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

    public void ReloadCurrentLevel()
    {
        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
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
}