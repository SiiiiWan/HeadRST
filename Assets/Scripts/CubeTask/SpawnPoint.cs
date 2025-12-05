using UnityEngine;

public class SpawnPoint : MonoBehaviour, IInGazeConeHandler
{
    public GameObject SpawnObjectPrefab;
    public Transform SpawnObjectParent;

    public bool IsInGazeCone { get; private set; }

    void Update()
    {
        RefreshInGazeConeState();

        if (IsInGazeCone)
        {
            if (PinchDetector.GetInstance().IsOneHandPinching && PinchDetector.GetInstance().IsNoHandPinching_LastFrame)
            {
                GameObject spawnObject = Instantiate(SpawnObjectPrefab, transform.position, Quaternion.identity);
                spawnObject.transform.SetParent(SpawnObjectParent);

                ManipulatableCube manipulatableCube = spawnObject.GetComponent<ManipulatableCube>();
                ObjectManager.GetInstance().TaskCursor.SetCursorPositionTo(transform.position);
                manipulatableCube.OnPickup();
            }
        }
    }

    public void RefreshInGazeConeState()
    {
        float angleToGaze = Vector3.Angle(EyeGaze.GetInstance().GetGazeRay().direction, transform.position - EyeGaze.GetInstance().GetGazeRay().origin);
        if (angleToGaze <= 10f || EyeGaze.GetInstance().GetGazeHitTrans() == transform)
        {
            OnGazeConeEnter();
        }
        else
        {
            OnGazeConeExit();
        }
    }

    public void OnGazeConeEnter()
    {
        IsInGazeCone = true;
        GetComponent<Outline>().enabled = true;
    }

    public void OnGazeConeExit()
    {
        IsInGazeCone = false;
        GetComponent<Outline>().enabled = false;
    }
}
