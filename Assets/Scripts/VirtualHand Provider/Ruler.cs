using UnityEngine;

public class Ruler : MonoBehaviour
{
    public Transform rulerStart, rulerEnd;
    private LineRenderer _rulerLine;
    private TextMesh _distanceText;

    void Update()
    {
        if(_rulerLine == null)
        {
            _rulerLine = new GameObject().AddComponent<LineRenderer>();
            _rulerLine.startWidth = 0.005f;
            _rulerLine.endWidth = 0.005f;
            _rulerLine.material = new Material(Shader.Find("Sprites/Default"));
            _rulerLine.positionCount = 2;
            _rulerLine.transform.parent = transform;
        }

        if (_distanceText == null)
        {
            GameObject textObject = new GameObject("DistanceText");
            textObject.transform.parent = transform;
            _distanceText = textObject.AddComponent<TextMesh>();
            _distanceText.fontSize = 50;
            _distanceText.anchor = TextAnchor.MiddleCenter;
            _distanceText.alignment = TextAlignment.Center;
            _distanceText.characterSize = 0.01f;
        }

        _rulerLine.SetPosition(0, rulerStart.position);
        _rulerLine.SetPosition(1, rulerEnd.position);

        float distance = Vector3.Distance(rulerStart.position, rulerEnd.position);
        Vector3 midPoint = (rulerStart.position + rulerEnd.position) / 2;

        _distanceText.transform.position = midPoint;
        _distanceText.text = $"{distance:F2}m"; // Format to 2 decimal places

        // Orient the text towards the camera
        if (Camera.main != null)
        {
            _distanceText.transform.LookAt(Camera.main.transform);
            _distanceText.transform.Rotate(0, 180, 0);
        }
    }
}
