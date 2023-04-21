using UnityEngine;
using UnityEditor;

namespace VoidTerrainGenerator {
    
    [CustomEditor(typeof(Noise))]
    public class NoiseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Noise script = (Noise)target;

            if (DrawDefaultInspector())
            {
                if (script.autoUpdate)
                {
                    script.GenerateMap();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                script.GenerateMap();
            }
        }
    }
}