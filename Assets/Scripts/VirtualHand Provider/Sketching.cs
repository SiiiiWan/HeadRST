using UnityEngine;

public class Sketching : MonoBehaviour
{
    [Range(0f, 0.2f)] public float width = 0.02f;
    private LineRenderer _currentDrawing;
    private int _index;

    void Update()
    {
        if(Settings.GetInstance().VirtualHandProvider.IsGazeRedirecting() || PinchDetector.GetInstance().IsNoHandPinching)
        {
            _currentDrawing = null;
            return;
        }


        if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
        {
            CreateSketchLine();
        }

        if (_currentDrawing != null)
        {
            Draw();
        }

    }

    private void CreateSketchLine()
    {
        if (_currentDrawing == null)
        {
            Vector3 HandToPinchOffset = HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false);
            Vector3 currentPinchPosition = Settings.GetInstance().GetVirtualHandPose(isRightHand: true).position + HandToPinchOffset;
            _index = 0;
            _currentDrawing = new GameObject().AddComponent<LineRenderer>();
            _currentDrawing.startWidth = width;
            _currentDrawing.endWidth = width;
            _currentDrawing.material = new Material(Shader.Find("Sprites/Default"));
            _currentDrawing.positionCount = 1;
            _currentDrawing.SetPosition(0, currentPinchPosition);
            _currentDrawing.transform.parent = transform;

        }        
    }

    private void Draw()
    {
        Vector3 HandToPinchOffset = HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false);
        Vector3 currentPinchPosition = Settings.GetInstance().GetVirtualHandPose(isRightHand: true).position + HandToPinchOffset;

        Vector3 currentPos = _currentDrawing.GetPosition(_index);
        if (Vector3.Distance(currentPos, currentPinchPosition) > 0.01f)
        {
            _index++;
            _currentDrawing.positionCount = _index + 1;
            _currentDrawing.SetPosition(_index, currentPinchPosition);
        }

        
    }
}
