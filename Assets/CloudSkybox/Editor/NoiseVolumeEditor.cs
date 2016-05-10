using UnityEngine;
using UnityEditor;
using System.IO;

namespace CloudSkybox
{
    [CustomEditor(typeof(NoiseVolume))]
    public class NoiseVolumeEditor : Editor
    {
        SerializedProperty _frequency;
        SerializedProperty _seed;

        void OnEnable()
        {
            _frequency = serializedObject.FindProperty("_frequency");
            _seed = serializedObject.FindProperty("_seed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_frequency);
            EditorGUILayout.PropertyField(_seed);
            var shouldRebuild = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (shouldRebuild)
                foreach (var t in targets)
                    ((NoiseVolume)t).RebuildTexture();
        }

        [MenuItem("Assets/Create/NoiseVolume")]
        public static void CreateNoiseVolume()
        {
            // Make a proper path from the current selection.
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(path))
                path = "Assets";
            else if (Path.GetExtension(path) != "")
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewNoiseVolume.asset");

            // Create an asset.
            var asset = ScriptableObject.CreateInstance<NoiseVolume>();
            AssetDatabase.CreateAsset(asset, assetPathName);
            AssetDatabase.AddObjectToAsset(asset.texture, asset);

            // Build an initial volume for the asset.
            asset.RebuildTexture();

            // Save the generated mesh asset.
            AssetDatabase.SaveAssets();

            // Tweak the selection.
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
