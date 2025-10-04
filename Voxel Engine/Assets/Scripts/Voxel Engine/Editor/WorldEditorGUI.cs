using UnityEditor;
using UnityEngine;

namespace Voxel_Engine.Editor
{
    [CustomEditor(typeof(World))]
    public class WorldEditorGUI : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Save Folder"))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + Application.persistentDataPath.Replace("/","\\"));
            }
        }
    }
}