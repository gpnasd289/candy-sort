using System.Collections.Generic;
using UnityEngine;

public class Tube : MonoBehaviour
{
    public int maxLayers = 7;
    public float layerSpacing = 0.3f;
    public Transform layerContainer;

    public GameObject candyLayerPrefab;

    private List<CandyLayer> layers = new List<CandyLayer>();

    void Start()
    {
        if (layerContainer == null)
        {
            GameObject containerObj = new GameObject("LayerContainer");
            containerObj.transform.SetParent(transform);
            containerObj.transform.localPosition = Vector3.zero;
            layerContainer = containerObj.transform;
        }
    }

    public bool CanReceiveLayer(CandyLayer layer)
    {
        // Tube must not be full
        if (layers.Count >= maxLayers)
            return false;

        // If empty, can receive any layer
        if (layers.Count == 0)
            return true;

        // Must match top layer color (if top layer is not mystery)
        CandyLayer topLayer = GetTopLayer();
        if (topLayer.isMystery)
            return false; // Can't add to mystery layer

        return topLayer.layerColor == layer.layerColor;
    }

    public void AddLayer(CandyLayer layer)
    {
        layers.Add(layer);
        layer.SetParentTube(this);
        layer.layerIndex = layers.Count - 1;
        layer.transform.SetParent(layerContainer);

        UpdateLayerPosition(layer, layers.Count - 1);
    }

    public void AddLayerAnimatedWithCurve(CandyLayer layer, Vector3 bezierStart, Vector3 bezierControl1, Vector3 bezierControl2, Vector3 bezierEnd, System.Action onComplete = null)
    {
        int targetIndex = layers.Count;
        layers.Add(layer);
        layer.SetParentTube(this);
        layer.layerIndex = targetIndex;
        layer.transform.SetParent(layerContainer);

        Vector3 targetWorldPos = GetLayerWorldPosition(targetIndex);

        // Animate along bezier curve to target
        layer.AnimateAlongBezierCurve(bezierStart, bezierControl1, bezierControl2, bezierEnd, targetWorldPos, () =>
        {
            // Check if top layer should be revealed
            CheckAndRevealTopLayer();

            onComplete?.Invoke();
        });
    }

    public CandyLayer GetTopLayer()
    {
        if (layers.Count == 0)
            return null;
        return layers[^1];
    }

    public List<CandyLayer> GetMovableGroup()
    {
        List<CandyLayer> group = new List<CandyLayer>();
        if (layers.Count == 0)
            return group;

        CandyLayer topLayer = GetTopLayer();

        // Top layer must not be mystery
        if (topLayer.isMystery)
            return group;

        CandyColor topColor = topLayer.layerColor;

        // Get all consecutive layers from top with same color
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            if (layers[i].layerColor == topColor && !layers[i].isMystery)
            {
                group.Add(layers[i]);
            }
            else
            {
                break;
            }
        }

        return group;
    }

    public void RemoveLayers(List<CandyLayer> layersToRemove)
    {
        foreach (CandyLayer layer in layersToRemove)
        {
            layers.Remove(layer);
        }

        // Update indices of remaining layers
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].layerIndex = i;
        }

        // After removing layers, check if new top layer should be revealed
        CheckAndRevealTopLayer();
    }

    public bool CanMove()
    {
        if (layers.Count == 0)
            return false;

        CandyLayer topLayer = GetTopLayer();
        // Can move if top layer is not mystery
        return !topLayer.isMystery;
    }

    public bool IsFull()
    {
        return layers.Count >= maxLayers;
    }

    public bool IsEmpty()
    {
        return layers.Count == 0;
    }

    private void CheckAndRevealTopLayer()
    {
        if (layers.Count == 0)
            return;

        CandyLayer topLayer = GetTopLayer();

        // If top layer is mystery, reveal it
        if (topLayer.isMystery)
        {
            topLayer.RevealColorGradually();
        }
    }

    private void UpdateLayerPosition(CandyLayer layer, int index)
    {
        Vector3 localPos = (index * layerSpacing * Vector3.up) + new Vector3(0f, layerSpacing, 0f);
        layer.SetLocalPositionImmediate(localPos);
    }

    private Vector3 GetLayerWorldPosition(int index)
    {
        Vector3 localPos = (index * layerSpacing * Vector3.up) + new Vector3(0f, layerSpacing, 0f);
        return layerContainer.TransformPoint(localPos);
    }

    public void RearrangeLayers()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            UpdateLayerPosition(layers[i], i);
        }
    }

    public int GetLayerCount()
    {
        return layers.Count;
    }

    public bool IsSingleColor()
    {
        if (layers.Count == 0)
            return false;

        CandyColor firstColor = layers[0].layerColor;
        foreach (CandyLayer layer in layers)
        {
            if (layer.layerColor != firstColor)
                return false;
        }
        return true;
    }
    public List<CandyLayer> GetAllLayers()
    {
        return new List<CandyLayer>(layers);
    }

    public CandyLayer CreateLayer(CandyColor color, bool isMystery)
    {
        GameObject layerObj;

        if (candyLayerPrefab != null)
        {
            layerObj = Instantiate(candyLayerPrefab);
        }
        else
        {
            layerObj = new GameObject("CandyLayer");
            layerObj.AddComponent<CandyLayer>();
        }

        CandyLayer layer = layerObj.GetComponent<CandyLayer>();
        if (layer == null)
        {
            layer = layerObj.AddComponent<CandyLayer>();
        }

        layer.Initialize(color, isMystery, layers.Count, this);

        return layer;
    }
}