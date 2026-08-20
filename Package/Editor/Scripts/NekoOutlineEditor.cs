#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using NekoOutlines.Runtime;

namespace NekoOutlines.Editor
{
    [CustomEditor(typeof(NekoOutline))]
    public class NekoOutlineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NekoOutline outlineScript = (NekoOutline)target;

            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Refresh Outline", GUILayout.Height(30f)))
            {
                if (outlineScript != null)
                {
                    outlineScript.Refresh();
                    EditorUtility.SetDirty(outlineScript);
                }
            }
        }
    }
}
#endif
