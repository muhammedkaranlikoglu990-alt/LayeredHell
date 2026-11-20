#if UNITY_EDITOR
// Creates and configures GPU-instanced M_<ATLAS>.mat materials for all 9 texture atlases.
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class MaterialBuilder
    {
        public struct AtlasSpec
        {
            public string atlasName;
            public bool tileMode;
            public float smoothness;

            public AtlasSpec(string atlasName, bool tileMode, float smoothness)
            {
                this.atlasName = atlasName;
                this.tileMode = tileMode;
                this.smoothness = smoothness;
            }
        }

        public static readonly AtlasSpec[] Atlases =
        {
            new AtlasSpec("ENV_STRUCT_2K",    true,  0.18f),
            new AtlasSpec("ENV_MAT_2K",       true,  0.28f),
            new AtlasSpec("ENV_CITY_2K",      true,  0.22f),
            new AtlasSpec("HERO_WEAPONS_1K",  false, 0.45f),
            new AtlasSpec("HERO_ZOMBIE_1K",   false, 0.20f),
            new AtlasSpec("HERO_SURVIVOR_1K", false, 0.22f),
            new AtlasSpec("HERO_ITEMS_1K",    false, 0.32f),
            new AtlasSpec("PROP_A_512",       false, 0.25f),
            new AtlasSpec("PROP_B_512",       false, 0.25f),
        };

        [MenuItem("Layered Hell/2. Build Atlas Materials")]
        public static void BuildAllMaterials()
        {
            const string materialsDir = "Assets/Art/Materials";
            if (!Directory.Exists(materialsDir))
                Directory.CreateDirectory(materialsDir);

            Shader shader = Shader.Find("LayeredHell/AtlasTiling");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null)
                shader = Shader.Find("Standard");

            int built = 0;
            for (int i = 0; i < Atlases.Length; i++)
            {
                var spec = Atlases[i];
                string matPath = materialsDir + "/M_" + spec.atlasName + ".mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                else
                {
                    mat.shader = shader;
                }

                mat.enableInstancing = true;
                mat.SetFloat("_CellSize", 0.25f);
                mat.SetFloat("_TileMode", spec.tileMode ? 1.0f : 0.0f);
                mat.SetFloat("_Smoothness", spec.smoothness);
                mat.SetColor("_Tint", Color.white);
                mat.SetColor("_Emission", Color.black);

                string texPath = "Assets/Art/Textures/" + spec.atlasName + ".png";
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.mainTexture = tex;
                }

                EditorUtility.SetDirty(mat);
                built++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[LayeredHell] Built " + built + " GPU-instanced atlas materials in " + materialsDir);
        }
    }
}
#endif
