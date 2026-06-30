using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TechniqueControl))]
public class TechniqueControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TechniqueControl control = (TechniqueControl)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Switch Technique", EditorStyles.boldLabel);

        if (GUILayout.Button("GAZE+PINCH")) Switch(control, MagicPitchTechnique.GazePinch);
        if (GUILayout.Button("MAGIC")) Switch(control, MagicPitchTechnique.MAGIC);
        if (GUILayout.Button("MAGICPITCH")) Switch(control, MagicPitchTechnique.MAGICPITCH);
        if (GUILayout.Button("MAGMODPITCH")) Switch(control, MagicPitchTechnique.MAGMODPITCH);
    }

    private static void Switch(TechniqueControl control, MagicPitchTechnique technique)
    {
        Undo.RecordObject(control, "Switch MagicPitch Technique");
        foreach (var manipulationTechnique in control.GetComponents<ManipulationTechnique>())
        {
            Undo.RecordObject(manipulationTechnique, "Switch MagicPitch Technique");
        }

        control.SetTechnique(technique);
        EditorUtility.SetDirty(control);
        foreach (var manipulationTechnique in control.GetComponents<ManipulationTechnique>())
        {
            EditorUtility.SetDirty(manipulationTechnique);
        }
    }
}
