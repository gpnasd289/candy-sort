using UnityEngine;

public class InputController : MonoBehaviour
{
    public Camera mainCamera;
    private Vector3 dragStartPos;
    private Vector3 dragCurrentPos;
    private bool isDragging = false;
    private Tube dragStartTube = null;
    private Tube currentHoverTube = null;

    [Header("Drag Line Settings")]
    public Material dragLineMaterial;
    public float lineWidth = 0.1f;
    public Color validDragColor = Color.green;
    public Color invalidDragColor = Color.red;
    public Color neutralDragColor = Color.yellow;
    public int lineSegments = 20;

    private LineRenderer dragLineRendererStraight;
    private GameObject dragLineObject;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        CreateStraightDragLine();
    }

    void Update()
    {
        HandleInput();
    }

    private void CreateStraightDragLine()
    {
        dragLineObject = new GameObject("DragLine");
        dragLineObject.transform.SetParent(transform);

        dragLineRendererStraight = dragLineObject.AddComponent<LineRenderer>();

        if (dragLineMaterial != null)
        {
            dragLineRendererStraight.material = dragLineMaterial;
        }
        else
        {
            // Create default material
            dragLineRendererStraight.material = new Material(Shader.Find("Sprites/Default"));
        }

        dragLineRendererStraight.startWidth = lineWidth;
        dragLineRendererStraight.endWidth = lineWidth;
        dragLineRendererStraight.positionCount = 2;
        dragLineRendererStraight.useWorldSpace = true;
        dragLineRendererStraight.enabled = false;

        // Set default color
        dragLineRendererStraight.startColor = neutralDragColor;
        dragLineRendererStraight.endColor = neutralDragColor;
    }

    private void HandleInput()
    {
        // Handle both mouse and touch input
        if (Input.GetMouseButtonDown(0))
        {
            OnDragStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnDragging(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnDragEnd(Input.mousePosition);
        }

        // Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                OnDragStart(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                OnDragging(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended && isDragging)
            {
                OnDragEnd(touch.position);
            }
        }
    }

    private void OnDragStart(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tube tube = hit.collider.GetComponent<Tube>();
            if (tube != null)
            {
                dragStartPos = screenPosition;
                dragStartTube = tube;
                isDragging = true;

                dragLineRendererStraight.enabled = true;
            }
        }
    }

    private void OnDragging(Vector3 screenPosition)
    {
        dragCurrentPos = screenPosition;

        // Check what tube we're hovering over
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            currentHoverTube = hit.collider.GetComponent<Tube>();
        }
        else
        {
            currentHoverTube = null;
        }

        // Draw and update drag line
        UpdateDragLine();
    }

    private void OnDragEnd(Vector3 screenPosition)
    {
        if (dragStartTube == null)
        {
            ClearDragLine();
            isDragging = false;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tube targetTube = hit.collider.GetComponent<Tube>();

            if (targetTube != null && targetTube != dragStartTube)
            {
                // Store the bezier curve for candy animation
                if (IsValidConnection(dragStartTube, targetTube))
                {
                    StoreBezierCurveForMove(dragStartTube, targetTube);
                }
                else
                {
                    ClearDragLine();
                }

                // First click on source tube
                GameManager.Instance.OnTubeClicked(dragStartTube);

                // Second click on target tube
                GameManager.Instance.OnTubeClicked(targetTube);

                // Don't clear the line yet - GameManager will do it after animation
            }
            else
            {
                ClearDragLine();
            }
        }
        else
        {
            ClearDragLine();
        }

        isDragging = false;
        dragStartTube = null;
        currentHoverTube = null;
    }

    private void StoreBezierCurveForMove(Tube from, Tube to)
    {
        Vector3 start = from.transform.position;
        Vector3 end = to.transform.position;

        Vector3 control1, control2;
        CalculateBezierControlPoints(start, end, out control1, out control2);

        // Store curve in GameManager for use during animation
        GameManager.Instance.SetMovementCurve(start, control1, control2, end);
    }

    public void ClearDragLineAfterMove()
    {
        ClearDragLine();
    }

    private void UpdateDragLine()
    {
        if (dragStartTube == null || dragLineRendererStraight == null)
            return;

        Vector3 startWorldPos = dragStartTube.transform.position;

        // Get end position from hover tube or screen position
        Vector3 endWorldPos;
        if (currentHoverTube != null && currentHoverTube != dragStartTube)
        {
            endWorldPos = currentHoverTube.transform.position;
        }
        else
        {
            // Convert screen position to world position
            Ray ray = mainCamera.ScreenPointToRay(dragCurrentPos);
            Plane plane = new Plane(Vector3.up, startWorldPos);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                endWorldPos = ray.GetPoint(distance);
            }
            else
            {
                endWorldPos = startWorldPos + Vector3.forward * 2f;
            }
        }

        // Draw curved line (arc)
        //DrawArcLine(startWorldPos, endWorldPos);

        // Draw straight line
        DrawStraightLine(startWorldPos, endWorldPos);

        // Update line color based on validity
        UpdateLineColor();
    }
    private void DrawStraightLine(Vector3 start, Vector3 end)
    {
        dragLineRendererStraight.SetPosition(0, start);
        dragLineRendererStraight.SetPosition(1, end);
    }
    private void DrawArcLine(Vector3 start, Vector3 end)
    {
        Vector3 control1, control2;
        CalculateBezierControlPoints(start, end, out control1, out control2);

        for (int i = 0; i < lineSegments; i++)
        {
            float t = i / (float)(lineSegments - 1);
            Vector3 point = CalculateCubicBezierPoint(t, start, control1, control2, end);
            //dragLineRenderer.SetPosition(i, point);
        }
    }

    private void CalculateBezierControlPoints(Vector3 start, Vector3 end, out Vector3 control1, out Vector3 control2)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        Vector3 midPoint = (start + end) / 2f;

        // Height of the curve
        float curveHeight = Mathf.Min(distance * 2f, 2f);

        // Control points positioned above the start and end
        control1 = start + Vector3.up * 3 + direction * 0.001f;
        control2 = end + Vector3.up * 3 - direction * 0.001f;
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

    private void UpdateLineColor()
    {
        Color lineColor = neutralDragColor;

        // Check if we have a valid target tube
        if (currentHoverTube != null && currentHoverTube != dragStartTube)
        {
            // Check if the move would be valid
            if (IsValidConnection(dragStartTube, currentHoverTube))
            {
                lineColor = validDragColor;
            }
            else
            {
                lineColor = invalidDragColor;
            }
        }

        dragLineRendererStraight.startColor = lineColor;
        dragLineRendererStraight.endColor = lineColor;
    }

    private bool IsValidConnection(Tube fromTube, Tube toTube)
    {
        if (fromTube == null || toTube == null)
            return false;

        // Check if source tube has movable layers
        if (!fromTube.CanMove() || fromTube.GetLayerCount() == 0)
            return false;

        var movableLayers = fromTube.GetMovableGroup();
        if (movableLayers.Count == 0)
            return false;

        // Check if target can receive
        if (toTube.GetLayerCount() + movableLayers.Count > toTube.maxLayers)
            return false;

        // Check color matching
        if (!toTube.IsEmpty())
        {
            var topLayer = toTube.GetTopLayer();
            if (topLayer.layerColor != movableLayers[0].layerColor)
                return false;
        }

        // Check path clearance
        if (PathValidator.Instance != null)
        {
            return PathValidator.Instance.IsPathClear(fromTube, toTube);
        }

        return true;
    }

    private void ClearDragLine()
    {
        if (dragLineRendererStraight != null)
        {
            dragLineRendererStraight.enabled = false;
        }
    }
}