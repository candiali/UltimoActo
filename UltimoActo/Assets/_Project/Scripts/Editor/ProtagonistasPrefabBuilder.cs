#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Construye los prefabs PF_Protagonista_A y PF_Protagonista_B en Assets/_Project/Prefabs/Personajes/
    /// a partir de las mallas 3D (.obj) generadas en Assets/_Project/Models/Personajes/ y les asigna
    /// los materiales URP Dark Academia correspondientes.
    /// </summary>
    public static class ProtagonistasPrefabBuilder
    {
        private const string ModelsFolder = "Assets/_Project/Models/Personajes";
        private const string PrefabsFolder = "Assets/_Project/Prefabs/Personajes";
        private const string MaterialsFolder = "Assets/_Project/Materials/Personajes";

        [MenuItem("Tools/Último Acto/Personajes/Generar Prefabs Protagonistas A y B")]
        public static void BuildProtagonistasPrefabs()
        {
            EnsureFolders();
            AssetDatabase.Refresh();

            var palette = new Dictionary<string, Material>
            {
                { "Mat_Char_Skin", GetOrCreateMaterial("Mat_Char_Skin", new Color(0.86f, 0.77f, 0.72f), 0f, 0.35f) },
                { "Mat_Char_HairDark", GetOrCreateMaterial("Mat_Char_HairDark", new Color(0.09f, 0.08f, 0.09f), 0f, 0.25f) },
                { "Mat_Char_HairChestnut", GetOrCreateMaterial("Mat_Char_HairChestnut", new Color(0.24f, 0.14f, 0.10f), 0f, 0.30f) },
                { "Mat_Char_CharcoalCoat", GetOrCreateMaterial("Mat_Char_CharcoalCoat", new Color(0.12f, 0.13f, 0.15f), 0f, 0.15f) },
                { "Mat_Char_WineJacket", GetOrCreateMaterial("Mat_Char_WineJacket", new Color(0.35f, 0.09f, 0.14f), 0f, 0.18f) },
                { "Mat_Char_IvoryCloth", GetOrCreateMaterial("Mat_Char_IvoryCloth", new Color(0.90f, 0.87f, 0.78f), 0f, 0.15f) },
                { "Mat_Char_LeatherDark", GetOrCreateMaterial("Mat_Char_LeatherDark", new Color(0.16f, 0.10f, 0.08f), 0.05f, 0.45f) },
                { "Mat_Char_AgedGold", GetOrCreateMaterial("Mat_Char_AgedGold", new Color(0.72f, 0.58f, 0.29f), 0.85f, 0.55f) },
                { "Mat_Char_Black", GetOrCreateMaterial("Mat_Char_Black", new Color(0.06f, 0.06f, 0.07f), 0f, 0.20f) }
            };

            BuildFromModel("CH_Protagonista_A.obj", "PF_Protagonista_A", palette);
            BuildFromModel("CH_Protagonista_B.obj", "PF_Protagonista_B", palette);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Último Acto] Prefabs PF_Protagonista_A y PF_Protagonista_B actualizados con mallas 3D en " + PrefabsFolder);
        }

        private static void BuildFromModel(string objFileName, string prefabName, Dictionary<string, Material> palette)
        {
            string modelPath = $"{ModelsFolder}/{objFileName}";
            ConfigureModelImporter(modelPath);

            GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (modelAsset == null)
            {
                Debug.LogError($"[Último Acto] No se encontró el modelo 3D en {modelPath}");
                return;
            }

            GameObject root = new GameObject(prefabName);
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            visual.name = "VisualRoot";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            // Desempaquetar instancia anidada para que el prefab quede limpio y editable
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // Asignar materiales URP Dark Academia a cada slot del .obj
            foreach (MeshRenderer renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material[] current = renderer.sharedMaterials;
                for (int i = 0; i < current.Length; i++)
                {
                    if (current[i] == null) continue;
                    foreach (var kvp in palette)
                    {
                        if (current[i].name.Contains(kvp.Key))
                        {
                            current[i] = kvp.Value;
                            break;
                        }
                    }
                }
                renderer.sharedMaterials = current;
            }

            string prefabPath = $"{PrefabsFolder}/{prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void ConfigureModelImporter(string modelPath)
        {
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.importNormals != ModelImporterNormals.Calculate)
            {
                importer.importNormals = ModelImporterNormals.Calculate;
                importer.normalSmoothingAngle = 60f;
                changed = true;
            }
            if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportViaMaterialDescription)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Personajes");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Materials"))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Materials", "Personajes");
        }

        private static Material GetOrCreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string fullPath = $"{MaterialsFolder}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, fullPath);
            }
            return mat;
        }
    }
}
#endif
