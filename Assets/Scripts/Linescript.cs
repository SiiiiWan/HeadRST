using UnityEngine;

public class Linescript
{
    public GameObject LineGameObject = new GameObject("Line");

    public Linescript()
    {
        LineRenderer lineRenderer = LineGameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lineRenderer.material.renderQueue = 3150;
        lineRenderer.material.color = Color.yellow;
        lineRenderer.widthMultiplier = 0.005f;
        lineRenderer.positionCount = 2;
        IsVisible = true;
    }

    public Linescript(int sampleNumberForCircle, Color color)
    {
        LineRenderer lineRenderer = LineGameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lineRenderer.material.renderQueue = 3150;
        lineRenderer.material.color = color;
        lineRenderer.widthMultiplier = 0.005f;
        lineRenderer.positionCount = sampleNumberForCircle;
        IsVisible = true;
    }

    public bool IsVisible { get; set; }

    public void SetPosition(Vector3 start, Vector3 end)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if (IsVisible == false)
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            return;
        }

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void DrawRing(Vector3 center, float radius)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if (!IsVisible)
        {
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector3.zero);
            }
            return;
        }

        Vector3 toCameraDirection = (Camera.main.transform.position - center).normalized;
        Quaternion faceCamera = Quaternion.LookRotation(toCameraDirection);

        int segments = lineRenderer.positionCount;
        float angleStep = 360f / (segments - 1);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 localPoint = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Vector3 worldPoint = center + faceCamera * localPoint;

            lineRenderer.SetPosition(i, worldPoint);
        }
    }

    public void SetWidth(float width)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = width;
    }
}
