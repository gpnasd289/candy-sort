using System.Collections.Generic;
using UnityEngine;

public class PathValidator : MonoBehaviour
{
    public static PathValidator Instance { get; private set; }

    public LayerMask blockLayer;
    public LayerMask tubeLayer;
    public float pathCheckRadius = 0.5f;
    public int pathCheckSegments = 10;

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
    }

    public bool IsPathClear(Tube fromTube, Tube toTube)
    {
        Vector3 start = fromTube.transform.position;
        Vector3 end = toTube.transform.position;

        // Check for blocking objects along the path
        return !IsPathBlocked(start, end, fromTube, toTube);
    }

    private bool IsStraightPath(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;

        // Check if path is horizontal or vertical (with some tolerance)
        float horizontalDot = Mathf.Abs(Vector3.Dot(direction, Vector3.right));
        float verticalDot = Mathf.Abs(Vector3.Dot(direction, Vector3.up));
        float forwardDot = Mathf.Abs(Vector3.Dot(direction, Vector3.forward));

        // Allow paths that are primarily along one axis
        float tolerance = 0.9f;
        return horizontalDot > tolerance || verticalDot > tolerance || forwardDot > tolerance;
    }

    private bool IsPathBlocked(Vector3 start, Vector3 end, Tube fromTube, Tube toTube)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        direction.Normalize();

        // Check multiple points along the path
        for (int i = 1; i < pathCheckSegments; i++)
        {
            float t = i / (float)pathCheckSegments;
            Vector3 checkPoint = start + direction * (distance * t);

            // Check for blocks at this point
            Collider[] hits = Physics.OverlapSphere(checkPoint, pathCheckRadius, blockLayer);
            if (hits.Length > 0)
            {
                return true; // Path is blocked
            }

            // Check for other tubes at this point (excluding source and target)
            Collider[] tubeHits = Physics.OverlapSphere(checkPoint, pathCheckRadius, tubeLayer);
            foreach (Collider hit in tubeHits)
            {
                Tube tube = hit.GetComponent<Tube>();
                if (tube != null && tube != fromTube && tube != toTube)
                {
                    return true; // Path blocked by another tube
                }
            }
        }

        return false; // Path is clear
    }

    public List<Tube> GetValidTargets(Tube sourceTube, List<Tube> allTubes)
    {
        List<Tube> validTargets = new List<Tube>();

        if (sourceTube == null || !sourceTube.CanMove() || sourceTube.GetLayerCount() == 0)
        {
            return validTargets;
        }

        List<CandyLayer> movingGroup = sourceTube.GetMovableGroup();
        if (movingGroup.Count == 0)
        {
            return validTargets;
        }

        foreach (Tube targetTube in allTubes)
        {
            if (targetTube == sourceTube)
                continue;

            // Check if target can receive candies
            if (!CanReceiveCandies(targetTube, movingGroup))
                continue;

            // Check if path is clear
            if (!IsPathClear(sourceTube, targetTube))
                continue;

            validTargets.Add(targetTube);
        }

        return validTargets;
    }

    private bool CanReceiveCandies(Tube tube, List<CandyLayer> layers)
    {
        // Check if tube has space
        if (tube.GetLayerCount() + layers.Count > tube.maxLayers)
            return false;

        // Check if tube is empty or has matching top color
        if (tube.IsEmpty())
            return true;

        CandyLayer topColor = tube.GetTopLayer();
        return topColor.layerColor == layers[0].layerColor;
    }

    public void VisualizePath(Vector3 start, Vector3 end, bool isValid)
    {
        Color lineColor = isValid ? Color.green : Color.red;
        Debug.DrawLine(start, end, lineColor, 1f);
    }

    // Draw gizmos for debugging
    void OnDrawGizmos()
    {
        if (Application.isPlaying && GameManager.Instance != null)
        {
            // Draw all possible paths
            List<Tube> tubes = GameManager.Instance.tubes;

            Gizmos.color = Color.yellow;
            foreach (Tube tube in tubes)
            {
                Gizmos.DrawWireSphere(tube.transform.position, 0.3f);
            }
        }
    }
}