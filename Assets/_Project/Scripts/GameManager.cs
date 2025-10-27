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
    }

    void Start()
    {
        InitializeLevel();
    }

    void InitializeLevel()
    {
        // Example level setup - customize as needed
        // This would typically be loaded from a level configuration
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

        if (!IsPathClear(fromTube, toTube))
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
        if (toTube.GetLayerCount() + movingLayers.Count > toTube.maxLayers)
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

    private bool IsPathClear(Tube fromTube, Tube toTube)
    {
        Vector3 start = fromTube.transform.position;
        Vector3 end = toTube.transform.position;

        // Check straight line path
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit[] hits = Physics.RaycastAll(start, direction, distance);

        foreach (RaycastHit hit in hits)
        {
            // Check if hit a blocking object
            if (hit.collider.CompareTag("Block"))
                return false;

            // Check if hit another tube (not the target)
            Tube hitTube = hit.collider.GetComponent<Tube>();
            if (hitTube != null && hitTube != toTube && hitTube != fromTube)
                return false;
        }

        return true;
    }

    private void ExecuteMove(Tube fromTube, Tube toTube)
    {
        isAnimating = true;

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

    private System.Collections.IEnumerator AnimateLayersMovement(List<CandyLayer> layers, Tube from, Tube to, bool useBezier)
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

        // Clear the drag line after all candies finish moving
        if (inputController != null)
        {
            inputController.ClearDragLineAfterMove();
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
    }

    public Tube CreateTube(Vector3 position)
    {
        GameObject tubeObj = Instantiate(tubePrefab, position, Quaternion.identity);
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