using UnityEngine;
using System.Collections.Generic;

public class CubeGenerator : MonoBehaviour
{

    private static readonly List<Vector3> _placedCubePositions = new List<Vector3>();
    public PositionRotationProvider PositionRotationProvider {get; set; }
    private Vector3 _absPosition;

    void Start()
    {
        _absPosition = transform.position;
        ClearPlacedCubePositions();
    }

    void Update()
    {
        PositionRotationProvider = ObjectManager.GetInstance().PositionRotationProvider_Global;

        Vector3 potentialPosition = PositionRotationProvider.GetPositionOutput(_absPosition);

        if (float.IsNaN(potentialPosition.x) || float.IsNaN(potentialPosition.y) || float.IsNaN(potentialPosition.z))
        {
            _absPosition = transform.position;
        }
        else
        {
            _absPosition = potentialPosition;
        }

        Vector3 updatedPosition = new Vector3(Mathf.Round(_absPosition.x * 2f) * 0.5f,
                                              Mathf.Round(_absPosition.y * 2f) * 0.5f,
                                              Mathf.Round(_absPosition.z * 2f) * 0.5f);

        if (updatedPosition.y < 0.25f)
        {
            updatedPosition.y = 0.25f;
        }

        transform.position = updatedPosition;

            if (PinchDetector.GetInstance().IsOneHandPinching)
            {
                PlaceCube();
            }
    }

    void PlaceCube()
    {
        if (!_placedCubePositions.Contains(transform.position))
        {
            GameObject newCube = Instantiate(gameObject, transform.position, transform.rotation);
            newCube.GetComponent<Outline>().enabled = false;
            Destroy(newCube.GetComponent<CubeGenerator>());

            // Add the new cube's position to the list to prevent duplicates.
            _placedCubePositions.Add(transform.position);
            AudioPlay.PlayClickSound();
        }
    }

    public static void ClearPlacedCubePositions()
    {
        _placedCubePositions.Clear();
    }


}