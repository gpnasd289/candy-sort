using UnityEngine;
using System.Collections;
using Audio;
using System;

public class Candy : MonoBehaviour
{
    public CandyColor candyColor;
    public bool isMystery = false;
    public int layerIndex;
    public int positionInLayer; // 0-6 position within the layer

    [Header("Materials")]
    public Material normalMaterial;
    public Material mysteryMaterial;

    public GameObject visual;
    private Renderer candyRenderer;
    private CandyLayer currentLayer;
    private bool isMoving = false;

    void Awake()
    {
        candyRenderer = visual.GetComponent<Renderer>();
    }

    public void SetColor(CandyColor color)
    {
        candyColor = color;
        if(!isMystery) UpdateVisual();
    }

    public void SetMystery(bool mystery)
    {
        isMystery = mystery;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (candyRenderer == null)
            return;

        if (isMystery)
        {
            if (mysteryMaterial != null)
            {
                candyRenderer.material = mysteryMaterial;
            }
        }
        else
        {
            if (normalMaterial != null)
            {
                candyRenderer.material = normalMaterial;
            }
            candyRenderer.material.color = GetColorFromEnum(candyColor);
        }
    }

    public void LerpMaterialToNormal(float t)
    {
        if (candyRenderer == null)
            return;

        if (mysteryMaterial != null && normalMaterial != null)
        {
            // Lerp between mystery and normal material
            candyRenderer.material.Lerp(mysteryMaterial, normalMaterial, t);

            // Also lerp the color
            Color targetColor = GetColorFromEnum(candyColor);
            candyRenderer.material.color = Color.Lerp(mysteryMaterial.color, targetColor, t);
        }
        else
        {
            // Fallback to color lerp
            Color mysteryColor = Color.gray;
            Color targetColor = GetColorFromEnum(candyColor);
            candyRenderer.material.color = Color.Lerp(mysteryColor, targetColor, t);
        }
    }

    public void SetLayer(CandyLayer layer)
    {
        currentLayer = layer;
    }

    public CandyLayer GetLayer()
    {
        return currentLayer;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    // Animate candy along bezier curve with offset
    public void AnimateAlongBezierCurve(Vector3 bezierStart, Vector3 bezierControl1, Vector3 bezierControl2, Vector3 bezierEnd,
                                        Vector3 exactTargetPosition, float delay, float offsetRadius, Action onComplete = null)
    {
        StartCoroutine(AnimateBezierCoroutine(bezierStart, bezierControl1, bezierControl2, bezierEnd, exactTargetPosition, delay, offsetRadius, onComplete));
    }

    private IEnumerator AnimateBezierCoroutine(Vector3 bezierStart, Vector3 bezierControl1, Vector3 bezierControl2, Vector3 bezierEnd,
                                                Vector3 exactTargetPosition, float delay, float offsetRadius, Action onComplete)
    {
        // Wait for delay
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        isMoving = true;

        Vector3 startPosition = transform.position;
        float duration = 0.6f;
        float elapsed = 0f;

        // Detach from parent to move freely
        Transform originalParent = transform.parent;
        transform.SetParent(null);

        // Calculate perpendicular offset direction for this candy
        Vector3 pathDirection = (bezierEnd - bezierStart).normalized;
        Vector3 offsetDirection = Vector3.Cross(pathDirection, Vector3.up).normalized;
        float offsetAngle = UnityEngine.Random.Range(0f, 360f);
        Vector3 randomOffset = Quaternion.Euler(0, offsetAngle, 0) * (offsetDirection * offsetRadius);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease in-out
            float smoothT = t * t * (3f - 2f * t);

            // Calculate position along bezier curve
            Vector3 bezierPoint = CalculateCubicBezierPoint(smoothT, bezierStart, bezierControl1, bezierControl2, bezierEnd);

            // Add offset that decreases as candy approaches target
            float offsetFactor = 1f - smoothT;
            Vector3 currentOffset = randomOffset * offsetFactor;

            // In the last 20% of movement, smoothly transition to exact target position
            if (t > 0.8f)
            {
                float transitionT = (t - 0.8f) / 0.2f;
                transform.position = Vector3.Lerp(bezierPoint + currentOffset, exactTargetPosition, transitionT);
            }
            else
            {
                transform.position = bezierPoint + currentOffset;
            }

            // Add rotation during flight
            transform.Rotate(Vector3.up, Time.deltaTime * 360f);

            yield return null;
        }

        // Ensure exact final position and rotation
        transform.position = exactTargetPosition;
        transform.rotation = Quaternion.identity;

        isMoving = false;

        AudioManager.Ins.PlaySfx(SFX_TYPE.CANDY_MOVED);
        onComplete?.Invoke();
    }

    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    private Color GetColorFromEnum(CandyColor color)
    {
        Color newColor = new();
        switch (color)
        {
            case CandyColor.Red:
            {
                if (ColorUtility.TryParseHtmlString("#ff0000", out newColor))
                    return newColor;
                else return Color.red;
            }
            case CandyColor.Blue: 
                {
                    if (ColorUtility.TryParseHtmlString("#0000ff", out newColor))
                        return newColor;
                    else return Color.blue;
                } 
            case CandyColor.Green:
                {
                    if (ColorUtility.TryParseHtmlString("#00ff00", out newColor))
                        return newColor;
                    else return Color.green;
                }
            case CandyColor.Yellow:
                {
                    if (ColorUtility.TryParseHtmlString("#ffe800", out newColor))
                        return newColor;
                    else return Color.yellow;
                }
            case CandyColor.Purple:
                {
                    if (ColorUtility.TryParseHtmlString("#a32ee9", out newColor))
                        return newColor;
                    else return new Color(0.5f, 0f, 0.5f);
                }
            case CandyColor.Orange:
                {
                    if (ColorUtility.TryParseHtmlString("#FF8024", out newColor))
                        return newColor;
                    else return new Color(1f, 0.5f, 0f);
                }
                
            case CandyColor.Pink:
                {
                    if (ColorUtility.TryParseHtmlString("#f11ff8", out newColor))
                        return newColor;
                    else return new Color(1f, 0.75f, 0.8f);
                }
            default: return Color.white;
        }
    }
}

public enum CandyColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink
}