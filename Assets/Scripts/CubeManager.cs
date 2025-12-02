using System.Collections.Generic;
using UnityEngine;

public class CubeManager : Singleton<CubeManager>
{
    public Material CubeSolidMaterial;
    public Material CubeTransparentMaterial;
    public Material CubeHoverMaterial;

    public CubeStackingCursor CubeStackingCursor;

    public List<ManipulatableCube> CurrentFocusedCubes = new List<ManipulatableCube>();
    public ManipulatableCube ClosestFocusedCube;

    void Update()
    {
        ClosestFocusedCube = GetClosestFocusedCube();

    }

    public void RegisterFocusedCube(ManipulatableCube cube)
    {
        if (!CurrentFocusedCubes.Contains(cube))
        {
            CurrentFocusedCubes.Add(cube);
        }
    }

    public void UnregisterFocusedCube(ManipulatableCube cube)
    {
        if (CurrentFocusedCubes.Contains(cube))
        {
            CurrentFocusedCubes.Remove(cube);
        }
    }

    public ManipulatableCube GetClosestFocusedCube()
    {
        if (CurrentFocusedCubes == null || CurrentFocusedCubes.Count == 0)
        {
            return null;
        }

        ManipulatableCube closestCube = null;
        float minDistance = float.MaxValue;
        Vector3 cursorPosition = CubeStackingCursor.transform.position;

        foreach (var cube in CurrentFocusedCubes)
        {
            if (cube == null) continue;

            float distance = Vector3.Distance(cube.transform.position, cursorPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCube = cube;
            }
        }

        return closestCube;
    }

    public Vector3 GetCenterOfFocusedCubes()
    {
        if (CurrentFocusedCubes == null || CurrentFocusedCubes.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 center = Vector3.zero;
        int validCubeCount = 0;

        foreach (var cube in CurrentFocusedCubes)
        {
            if (cube == null) continue;

            center += cube.transform.position;
            validCubeCount++;
        }

        if (validCubeCount > 0)
        {
            center /= validCubeCount;
        }

        return center;
    }

}
