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

    public Linescript(float widthMultiplier,Transform parent)
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

    public bool IsVisible {get; set;}

    public void DrawRay(Ray ray)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if(IsVisible == false)
        {
            lineRenderer.SetPosition(0, Vector3.zero); 
            lineRenderer.SetPosition(1, Vector3.zero); 
            return;             
        }


        lineRenderer.SetPosition(0, ray.origin); 


        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            lineRenderer.SetPosition(1, ray.origin + (hit.point - ray.origin)/3*2);     
        }
        else
        {
            lineRenderer.SetPosition(1, ray.origin + ray.direction * 100);
        }           
    }


    public void SetPosition(Vector3 start, Vector3 end)
    {
        LineRenderer lineRenderer = LineGameObject.GetComponent<LineRenderer>();

        if(IsVisible == false)
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
}