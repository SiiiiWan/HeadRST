using UnityEngine;

public class DepthControlIndicator : MonoBehaviour
{
    public GameObject Square;
    public GameObject Triangle;

    private Vector3 _squareLocalPosition_start;
    private Vector3 _triangleLocalPosition_start;

    void Start()
    {
        _squareLocalPosition_start = Square.transform.localPosition;
        _triangleLocalPosition_start = Triangle.transform.localPosition;
    }

    public void ResetTrianglePosition()
    {
        Triangle.transform.localPosition = _triangleLocalPosition_start;
    }

    public void SetTrianglePosition_world(Vector3 pos)
    {
        Triangle.transform.position = pos;
    }

    public void SetSquarePosition_world(Vector3 pos)
    {
        Square.transform.position = pos;
    }

    public void SetSquareVisible(bool isVisible)
    {
        Square.SetActive(isVisible);
    }

    public void SetTriangleVisible(bool isVisible)
    {
        Triangle.SetActive(isVisible);
    }

    public void SetSquareColor(Material color)
    {
        var renderer = Square.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = color;
        }
    }

    public void SetTriangleColor(Material color)
    {
        var renderer = Triangle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = color;
        }
    }

    public void SetColor_All(Material color)
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = color;
        }
    }
}