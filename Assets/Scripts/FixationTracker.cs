using System.Collections.Generic;
using UnityEngine;

public class FixationTracker
{
    public float FixationDuration { get; private set; }
    public float FixationAngle { get; private set; }
    public int FixationWindowSize { get; private set; }
    public Vector3 FixationCentroid { get; private set; }
    public bool IsFixating {get; private set;}
    private Queue<Vector3> _fixationDirBuffer = new Queue<Vector3>();

    public FixationTracker(float duration, float dispersion)
    {
        FixationDuration = duration;
        FixationAngle = dispersion;

        FixationWindowSize = (int)(FixationDuration / Time.deltaTime);
    }

    public bool GetIsFixating(Vector3 gazeDir)
    {
        if (_fixationDirBuffer.Count < FixationWindowSize)
        {
            _fixationDirBuffer.Enqueue(gazeDir);
            return false;
        }

        if (_fixationDirBuffer.Count == FixationWindowSize)
        {
            _fixationDirBuffer.Enqueue(gazeDir);
        }

        while (_fixationDirBuffer.Count > FixationWindowSize)
        {
            _fixationDirBuffer.Dequeue();
        }

        Vector3 tmpCentroid = GetFixationDirCentroid();
        float dispersion = 0f;
        foreach (Vector3 dir in _fixationDirBuffer)
        {
            float angle = Vector3.Angle(dir, tmpCentroid);
            if (angle > dispersion) dispersion = angle;
        }

        if (dispersion > FixationAngle)
        {
            _fixationDirBuffer.Clear();
            return false;
        }

        FixationCentroid = tmpCentroid;

        return true;
    }

    public Vector3 GetFixationDirCentroid()
    {
        if (_fixationDirBuffer == null || _fixationDirBuffer.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var dir in _fixationDirBuffer)
        {
            sum += dir.normalized;
        }
        Vector3 centroid = sum / _fixationDirBuffer.Count;
        return centroid.normalized;
    }
}
