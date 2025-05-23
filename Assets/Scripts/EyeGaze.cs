using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EyeGaze : Singleton<EyeGaze>
{
    public OVREyeGaze LeftEye, RightEye;
    
    public bool ShowGazeCursor;
    public Transform GazeCursor;

    private Vector3 _combinedGazeOrigin, _combinedGazeDir;
    // private Vector3 _gazeOrigin, _gazeDir;



    [Header("One Euro Filter")]

    public bool FilteringGaze = true;


    public float FilterFrequency = 90f;
    public float FilterMinCutOff = 0.05f;
    public float FilterBeta = 10f;
    public float FitlerDcutoff = 1f;

    private OneEuroFilter<Vector3> _gazeDirFilter;
    private OneEuroFilter<Vector3> _gazePosFilter;


    protected override void Awake()
    {
        base.Awake();

        _gazeDirFilter = new OneEuroFilter<Vector3>(FilterFrequency);
        _gazePosFilter = new OneEuroFilter<Vector3>(FilterFrequency);
    }

    void Update()
    {
        _combinedGazeOrigin = Vector3.Lerp(LeftEye.transform.position, RightEye.transform.position, 0.5f);
        _combinedGazeDir = Vector3.Lerp(LeftEye.transform.forward, RightEye.transform.forward, 0.5f).normalized;

        if(FilteringGaze)
        {
            _gazeDirFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);
            _gazePosFilter.UpdateParams(FilterFrequency, FilterMinCutOff, FilterBeta, FitlerDcutoff);

            _combinedGazeDir = _gazeDirFilter.Filter(_combinedGazeDir);
            _combinedGazeOrigin = _gazePosFilter.Filter(_combinedGazeOrigin);   
        }

        GazeCursor.transform.position = GetGazeRay().origin + GetGazeRay().direction * 2f;
        GazeCursor.gameObject.SetActive(ShowGazeCursor);
    }

    public Ray GetGazeRay()
    {
        return new Ray(_combinedGazeOrigin, _combinedGazeDir);
    }

    public Transform GetGazeHitTrans()
    {
        RaycastHit hit;
        if (Physics.Raycast(GetGazeRay(), out hit, 100f))
        {
            return hit.transform;
        }
        return null;
    }
}
