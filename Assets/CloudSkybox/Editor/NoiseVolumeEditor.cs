using UnityEngine;
using UnityEditor;
using System.IO;

namespace CloudSkybox
{
    [CustomEditor(typeof(NoiseVolume))]
    public class NoiseVolumeEditor : Editor
    {
        SerializedProperty _noiseType;
        SerializedProperty _frequency;
        SerializedProperty _fractalLevel;
        SerializedProperty _seed;

        void OnEnable()
        {
            _noiseType = serializedObject.FindProperty("_noiseType");
            _frequency = serializedObject.FindProperty("_frequency");
            _fractalLevel = serializedObject.FindProperty("_fractalLevel");
            _seed = serializedObject.FindProperty("_seed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_noiseType);
            EditorGUILayout.PropertyField(_frequency);
            EditorGUILayout.PropertyField(_fractalLevel);
            EditorGUILayout.PropertyField(_seed);
            var shouldRebuild = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (shouldRebuild)
                foreach (var t in targets)
                    ((NoiseVolume)t).RebuildTexture();
        }

        static void CreateAsset(int resolution)
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
            asset.ChangeResolution(resolution);
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

        [MenuItem("Assets/Create/NoiseVolume/32")]
        public static void CreateNoiseVolume32()
        {
            CreateAsset(32);
        }

        [MenuItem("Assets/Create/NoiseVolume/64")]
        public static void CreateNoiseVolume64()
        {
            CreateAsset(64);
        }

        [MenuItem("Assets/Create/NoiseVolume/128")]
        public static void CreateNoiseVolume128()
        {
            CreateAsset(128);
        }
    }
}
