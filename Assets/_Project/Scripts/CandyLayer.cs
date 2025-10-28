using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyLayer : MonoBehaviour
{
    public CandyColor layerColor;
    public bool isMystery = false;
    public int layerIndex;
    public Tube parentTube;

    [Header("Layer Layout")]
    public int candiesPerLayer = 7;
    public float layerRadius = 0.4f;
    public float candyScale = 0.1f;

    private List<Candy> candies = new List<Candy>();

    public void Initialize(CandyColor color, bool mystery, int index, Tube tube)
    {
        layerColor = color;
        isMystery = mystery;
        layerIndex = index;
        parentTube = tube;

        CreateCandies();
    }

    private void CreateCandies()
    {
        for (int i = 0; i < candiesPerLayer; i++)
        {
            Candy candy = GameManager.Instance.CreateCandy(layerColor, isMystery);
            candy.layerIndex = layerIndex;
            candy.positionInLayer = i;
            candy.SetLayer(this);
            candy.transform.SetParent(transform);

            // Position candy in circular layout
            Vector3 localPos = GetCandyLocalPosition(i);
            candy.transform.localPosition = localPos;
            candy.transform.localScale = Vector3.one * candyScale;

            candies.Add(candy);
        }
    }

    private Vector3 GetCandyLocalPosition(int index)
    {
        // Arrange candies in hexagon: 1 center + 6 around
        if (index == 0)
        {
            // Center candy
            return Vector3.zero;
        }
        else
        {
            // 6 candies around in hexagon pattern
            int hexIndex = index - 1; // 0-5 for the outer candies
            float angle = (360f / 6f) * hexIndex;
            float radian = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radian) * layerRadius;
            float z = Mathf.Sin(radian) * layerRadius;

            return new Vector3(x, 0, z);
        }
    }

    public void RevealColor()
    {
        isMystery = false;
        foreach (Candy candy in candies)
        {
            candy.SetMystery(false);
        }
    }

    public void RevealColorGradually()
    {
        StartCoroutine(RevealColorGraduallyCoroutine());
    }

    private IEnumerator RevealColorGraduallyCoroutine()
    {
        if (!isMystery)
            yield break;

        float revealDuration = 1f;
        float elapsed = 0f;

        // Lerp materials gradually
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / revealDuration;

            // Apply material lerp to each candy
            foreach (Candy candy in candies)
            {
                candy.LerpMaterialToNormal(t);
            }

            yield return null;
        }

        // Ensure final state - set all candies to normal material with correct color
        isMystery = false;
        foreach (Candy candy in candies)
        {
            candy.SetMystery(false);
            candy.SetColor(layerColor); // This will apply the normal material with color
        }
    }

    public List<Candy> GetCandies()
    {
        return new List<Candy>(candies);
    }

    public void SetParentTube(Tube tube)
    {
        parentTube = tube;
    }

    public void AnimateAlongBezierCurve(Vector3 bezierStart, Vector3 bezierControl1, Vector3 bezierControl2, Vector3 bezierEnd, Vector3 targetWorldPosition, System.Action onComplete = null)
    {
        StartCoroutine(AnimateAlongBezierCurveCoroutine(bezierStart, bezierControl1, bezierControl2, bezierEnd, targetWorldPosition, onComplete));
    }

    private IEnumerator AnimateAlongBezierCurveCoroutine(Vector3 bezierStart, Vector3 bezierControl1, Vector3 bezierControl2, Vector3 bezierEnd, Vector3 targetWorldPosition, System.Action onComplete)
    {
        int completedCandies = 0;

        // Detach all candies from layer BEFORE moving layer
        foreach (Candy candy in candies)
        {
            candy.transform.SetParent(null);
        }

        // NOW move layer transform to target position (candies are already detached, so they won't teleport)
        transform.position = targetWorldPosition;

        // Animate each candy individually with delay and offset
        for (int i = 0; i < candies.Count; i++)
        {
            Candy candy = candies[i];

            // Calculate exact target position for this candy in world space
            Vector3 exactTargetPos = transform.TransformPoint(GetCandyLocalPosition(i));

            // Stagger the start time for each candy
            float delay = i * 0.05f; // 50ms delay between each candy

            // Random offset radius for path variation
            float offsetRadius = Random.Range(0.1f, 0.2f);

            // Start candy animation
            candy.AnimateAlongBezierCurve(bezierStart, bezierControl1, bezierControl2, bezierEnd,
                                         exactTargetPos, delay, offsetRadius, () =>
                                         {
                                             completedCandies++;

                                             // Reattach to layer parent
                                             candy.transform.SetParent(transform);
                                             candy.transform.localPosition = GetCandyLocalPosition(candy.positionInLayer);
                                             candy.transform.localRotation = Quaternion.identity;
                                         });
        }

        // Wait for all candies to complete
        while (completedCandies < candies.Count)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void SetLocalPositionImmediate(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }
}