using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MultiSelectionCubeVolume : MonoBehaviour
{
    public GameObject SelectionCubePrefab;

    private GameObject _selectionCubeInstance;
    private BoxCollider _selectionCubeCollider;
    private Vector3 _pinchStartPosition;

    public List<Transform> SelectedTransforms { get; private set; } = new List<Transform>();

    void Update()
    {
        Vector3 HandToPinchOffset = HandData.GetInstance().GetHandPosition(usePinchTip: true) - HandData.GetInstance().GetHandPosition(usePinchTip: false);

        if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame && ObjectManager.GetInstance().PickedUpObject == null)
        {
            if (SelectionCubePrefab != null)
            {
                // Store the starting position of the pinch
                _pinchStartPosition = Settings.GetInstance().GetVirtualHandPosition(isRightHand: true) + HandToPinchOffset;

                // Instantiate the prefab
                _selectionCubeInstance = Instantiate(SelectionCubePrefab);
                _selectionCubeInstance.name = "SelectionCubeInstance";
                _selectionCubeInstance.transform.parent = transform;
                _selectionCubeInstance.tag = "MultiselectVolume";
                _selectionCubeCollider = _selectionCubeInstance.GetComponent<BoxCollider>();
            }
            else
            {
                Debug.LogWarning("SelectionCubePrefab is not assigned in the VirtualHandInteraction component.");
            }
        }

        // While pinching
        if (PinchDetector.GetInstance().IsOneHandPinching)
        {
            if (_selectionCubeInstance != null)
            {
                // Get the current position of the pinch
                Vector3 currentPinchPosition = Settings.GetInstance().GetVirtualHandPosition(isRightHand: true) + HandToPinchOffset;

                // The center of the cube is the midpoint between the start and current positions
                Vector3 center = (_pinchStartPosition + currentPinchPosition) / 2f;

                // The size of the cube is the absolute difference between the positions
                Vector3 size = new Vector3(
                    Mathf.Abs(_pinchStartPosition.x - currentPinchPosition.x),
                    Mathf.Abs(_pinchStartPosition.y - currentPinchPosition.y),
                    Mathf.Abs(_pinchStartPosition.z - currentPinchPosition.z)
                );

                // Update the cube's position and scale
                _selectionCubeInstance.transform.position = center;
                _selectionCubeInstance.transform.localScale = size;


                // Detect and add objects within the resized cube
                UpdateSelectedObjects();
            }
        }

        // pinch released
        if (PinchDetector.GetInstance().IsNoHandPinching && _selectionCubeInstance != null)
        {
            Destroy(_selectionCubeInstance);
            _selectionCubeInstance = null;
            _selectionCubeCollider = null;
        }
    }

    private void UpdateSelectedObjects()
    {
        if (_selectionCubeInstance == null) return;

        var previouslySelected = new List<Transform>(SelectedTransforms);

        SelectedTransforms.Clear();

        // Use OverlapBox to find all colliders intersecting with the selection cube
        Collider[] hitColliders = Physics.OverlapBox(
            _selectionCubeInstance.transform.position,
            _selectionCubeInstance.transform.localScale / 2f,
            _selectionCubeInstance.transform.rotation
        );

        // Add the transform of each hit object to the list
        foreach (var hitCollider in hitColliders)
        {
            // Ensure we don't add the selection cube itself
            if (hitCollider.gameObject != _selectionCubeInstance)
            {
                SelectedTransforms.Add(hitCollider.transform);
            }
        }

        var newlySelected = SelectedTransforms.Except(previouslySelected);
        foreach (Transform trans in newlySelected)
        {
            if (trans.TryGetComponent<Outline>(out var outline))
            {
                outline.enabled = true;
            }
        }

        // Find deselected items (in previous but not in current) and disable their outline
        var deselected = previouslySelected.Except(SelectedTransforms);
        foreach (Transform trans in deselected)
        {
            if (trans.TryGetComponent<Outline>(out var outline))
            {
                outline.enabled = false;
            }
        }
    }
}
