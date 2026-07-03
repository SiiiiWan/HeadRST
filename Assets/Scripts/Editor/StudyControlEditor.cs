using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StudyControl))]
public class StudyControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (GUILayout.Button("Skip Current Trial"))
        {
            StudyControl control = (StudyControl)target;
            if (Application.isPlaying)
            {
                control.SkipCurrentTrial();
            }
            else
            {
                Debug.LogWarning("Skip Current Trial can only be used in Play Mode.", control);
            }
        }
    }
}
