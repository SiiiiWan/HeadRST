using UnityEngine;
using System.Collections;
using System;

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

    public Linescript(float widthMultiplier)
    {
        LineRenderer lineRenderer = LineGameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lineRenderer.material.renderQueue = 3150;
        lineRenderer.material.color = Color.yellow;
        lineRenderer.widthMultiplier = widthMultiplier;
        lineRenderer.positionCount = 2;
        IsVisible = true;
    }

    public Linescript(float widthMultiplier, Transform parent)
    {
        LineRenderer lineRenderer = LineGameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("UI/Unlit/Transparent"));
        lineRenderer.material.renderQueue = 3150;
        lineRenderer.material.color = Color.yellow;
        lineRenderer.widthMultiplier = widthMultiplier;
        lineRenderer.positionCount = 2;
        IsVisible = true;
        LineGameObject.transform.parent = parent;
    }

    public bool IsVisible { get; set; }

    public void DrawRay(Ray ray)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if (IsVisible == false)
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
            return;
        }


        lineRenderer.SetPosition(0, ray.origin);


        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            lineRenderer.SetPosition(1, ray.origin + (hit.point - ray.origin) / 3 * 2);
        }
        else
        {
            lineRenderer.SetPosition(1, ray.origin + ray.direction * 100);
        }
    }


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

    public void ResetPosition()
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.zero);
    }

    public void DrawRing(Vector3 center, float radius)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if (!IsVisible)
        {
            // Hide the ring by setting all positions to zero
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Vector3.zero);
            }
            return;
        }

        // Calculate direction from center to camera
        Vector3 toCameraDirection = (Camera.main.transform.position - center).normalized;

        // Create a rotation that faces the camera
        Quaternion faceCamera = Quaternion.LookRotation(toCameraDirection);

        // Generate ring points
        int segments = lineRenderer.positionCount;
        float angleStep = 360f / (segments - 1); // -1 to close the ring

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            // Create point on XY plane (Z = 0)
            Vector3 localPoint = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            // Rotate the point to face the camera and translate to center
            Vector3 worldPoint = center + faceCamera * localPoint;

            lineRenderer.SetPosition(i, worldPoint);
        }
    }

    public void SetColor(Color color)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();
        lineRenderer.material.color = color;
    }

    public void SetWidth(float width)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = width;
    }
}