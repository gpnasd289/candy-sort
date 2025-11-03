using Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<Tube> tubes = new List<Tube>();
    public GameObject candyPrefab;
    public GameObject tubePrefab;
    public GameObject candyLayerPrefab;

    private Tube selectedTube = null;
    private List<CandyLayer> movingLayers = null;
    private bool isAnimating = false;

    // Bezier curve data for movement
    private Vector3 bezierStart;
    private Vector3 bezierControl1;
    private Vector3 bezierControl2;
    private Vector3 bezierEnd;
    private bool hasBezierCurve = false;

    private InputController inputController;
    private LevelGenerator levelGenerator;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        inputController = GetComponent<InputController>();
        levelGenerator = GetComponent<LevelGenerator>();
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        AudioManager.Ins.PlayBgm(BGM_TYPE.HOME, 0f);
        UIManager.Instance.ShowPanel(UIManager.Instance.PanelHome);
        UIManager.Instance.PanelHome.StartLoading(true);
    }

    public void InitializeLevel()
    {
        UIManager.Instance.StartGame();
        levelGenerator.InitializeLevel();
    }

    public void OnTubeClicked(Tube tube)
    {
        if (isAnimating)
            return;

        if (selectedTube == null)
        {
            // First tube selection
            if (tube.CanMove() && tube.GetLayerCount() > 0)
            {
                selectedTube = tube;
                movingLayers = tube.GetMovableGroup();

                if (movingLayers.Count == 0)
                {
                    selectedTube = null;
                    return;
                }

                HighlightTube(tube, true);
            }
        }
        else
        {
            // Second tube selection
            if (tube == selectedTube)
            {
                // Cancel selection
                CancelMove();
            }
            else
            {
                // Try to move layers
                AttemptMove(selectedTube, tube);
            }
        }
    }

    private void AttemptMove(Tube fromTube, Tube toTube)
    {
        if (!IsValidMove(fromTube, toTube))
        {
            CancelMove();
            return;
        }

        ExecuteMove(fromTube, toTube);
    }

    private bool IsValidMove(Tube fromTube, Tube toTube)
    {
        // Check if from tube can move (top layer not mystery)
        if (!fromTube.CanMove())
            return false;

        // Check if there are layers to move
        if (movingLayers.Count == 0)
            return false;

        CandyLayer firstLayer = movingLayers[0];

        // Check if target tube can receive layers
        if (toTube.GetLayerCount() >= toTube.maxLayers)
            return false;

        // Check if tube is empty or has matching top color
        if (!toTube.IsEmpty())
        {
            CandyLayer topLayer = toTube.GetTopLayer();

            // Can't add to mystery layer
            if (topLayer.isMystery)
                return false;

            if (topLayer.layerColor != firstLayer.layerColor)
                return false;
        }

        return true;
    }

    private void ExecuteMove(Tube fromTube, Tube toTube)
    {
        isAnimating = true;

        // Calculate how many layers can actually move
        int spaceAvailable = toTube.maxLayers - toTube.GetLayerCount();
        int layersToMove = Mathf.Min(movingLayers.Count, spaceAvailable);

        // Take only the layers that can fit (from the top)
        List<CandyLayer> actualMovingLayers = new();
        for (int i = 0; i < layersToMove; i++)
        {
            actualMovingLayers.Add(movingLayers[i]);
        }
        movingLayers.Clear();
        movingLayers = actualMovingLayers;
        // Remove layers from source
        fromTube.RemoveLayers(movingLayers);

        // Pass bezier curve to animation if available
        if (hasBezierCurve)
        {
            StartCoroutine(AnimateLayersMovement(movingLayers, fromTube, toTube, true));
        }
        else
        {
            StartCoroutine(AnimateLayersMovement(movingLayers, fromTube, toTube, false));
        }

        HighlightTube(selectedTube, false);
        selectedTube = null;
    }

    public void SetMovementCurve(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end)
    {
        bezierStart = start;
        bezierControl1 = control1;
        bezierControl2 = control2;
        bezierEnd = end;
        hasBezierCurve = true;
    }

    private IEnumerator AnimateLayersMovement(List<CandyLayer> layers, Tube from, Tube to, bool useBezier)
    {
        int completedAnimations = 0;

        // Animate each layer with staggered timing
        for (int i = 0; i < layers.Count; i++)
        {
            CandyLayer layer = layers[i];

            if (useBezier && hasBezierCurve)
            {
                to.AddLayerAnimatedWithCurve(layer, bezierStart, bezierControl1, bezierControl2, bezierEnd, () =>
                {
                    completedAnimations++;
                });
            }

            // Delay between each layer starting to move
            yield return new WaitForSeconds(0.15f);
        }

        // Wait for all animations to complete
        while (completedAnimations < layers.Count)
        {
            yield return null;
        }

        // Small delay before rearranging
        yield return new WaitForSeconds(0.2f);

        // Rearrange source tube
        from.RearrangeLayers();

        movingLayers = null;
        isAnimating = false;
        hasBezierCurve = false;

        CheckWinCondition();
    }

    private void CancelMove()
    {
        if (selectedTube != null)
        {
            HighlightTube(selectedTube, false);
        }
        selectedTube = null;
        movingLayers = null;
    }

    private void HighlightTube(Tube tube, bool highlight)
    {
        // Add visual feedback for selected tube
        if (tube.TryGetComponent<Renderer>(out var renderer))
        {
            if (highlight)
            {
                renderer.material.color = Color.yellow;
            }
            else
            {
                renderer.material.color = Color.white;
            }
        }
    }

    private void CheckWinCondition()
    {
        // Check 1: No mystery layers remaining
        foreach (Tube tube in tubes)
        {
            if (tube.GetLayerCount() == 0)
                continue;

            foreach (CandyLayer layer in tube.GetAllLayers())
            {
                if (layer.isMystery)
                {
                    return; // Still have mystery layers, not complete
                }
            }
        }

        // Check 2: Group tubes by color and check completion rules
        Dictionary<CandyColor, List<Tube>> tubesByColor = new();

        foreach (Tube tube in tubes)
        {
            if (tube.GetLayerCount() == 0)
                continue;

            // Only count tubes with single color
            if (!tube.IsSingleColor())
            {
                return; // Mixed color tube exists, not complete
            }

            CandyColor tubeColor = tube.GetTopLayer().layerColor;

            if (!tubesByColor.ContainsKey(tubeColor))
            {
                tubesByColor[tubeColor] = new List<Tube>();
            }
            tubesByColor[tubeColor].Add(tube);
        }

        // Check 3: For each color, at most one tube can be incomplete (not full)
        foreach (var colorGroup in tubesByColor)
        {
            int incompleteTubes = 0;

            foreach (Tube tube in colorGroup.Value)
            {
                if (tube.GetLayerCount() < tube.maxLayers)
                {
                    incompleteTubes++;
                }
            }

            // If more than 1 incomplete tube of same color, not complete
            if (incompleteTubes > 1)
            {
                return;
            }
        }

        // All conditions met!
        OnLevelComplete();
    }

    private void OnLevelComplete()
    {
        Debug.Log("Level Complete!");

        if (levelGenerator.currentLevelID == 20) levelGenerator.currentLevelID = 1;
        else levelGenerator.currentLevelID++;
        PlayerPrefs.SetInt("CurrentLevelID", levelGenerator.currentLevelID);

        // Stop timer
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.StopTimer();
        }

        UIManager.Instance.ShowWinPanel();
        ClearDragState();
    }
    public void ClearDragState()
    {
        inputController.ClearDragLineAfterMove();
    }
    public Tube CreateTube(GameObject prefab, Vector3 position)
    {
        GameObject tubeObj = Instantiate(prefab, position, Quaternion.identity);
        Tube tube = tubeObj.GetComponent<Tube>();
        tube.candyLayerPrefab = candyLayerPrefab;
        tubes.Add(tube);
        return tube;
    }

    public Candy CreateCandy(CandyColor color, bool isMystery = false)
    {
        GameObject candyObj = Instantiate(candyPrefab);
        Candy candy = candyObj.GetComponent<Candy>();
        candy.SetColor(color);
        candy.SetMystery(isMystery);
        return candy;
    }
}