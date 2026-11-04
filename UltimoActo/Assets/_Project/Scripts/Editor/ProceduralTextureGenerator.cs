#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UltimoActo.Editor
{
    /// <summary>
    /// Generador de texturas procedurales PBR (Albedo + Normal Maps) para la Academia de Ballet.
    /// Dota a los escenarios de adoquines húmedos, mármol agrietado con vetas,
    /// parqué de espiga, alfombra de damasco vino y molduras de pared.
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        private const string TexturesDir = "Assets/_Project/Textures/PBR/";

        [MenuItem("Tools/Último Acto/Generar Texturas PBR (Adoquines, Mármol, Madera, Paredes)")]
        public static void GenerateAllTextures()
        {
            if (!Directory.Exists(TexturesDir))
            {
                Directory.CreateDirectory(TexturesDir);
            }

            GenerateCobblestoneTextures();
            GenerateMarbleTextures();
            GenerateParquetTextures();
            GenerateVelvetCarpetTextures();
            GenerateWallIvoryTextures();
            GenerateFacadeTextures();
            GenerateMahoganyTextures();
            GenerateGreenVelvetTextures();
            GenerateLeatherTextures();

            AssetDatabase.Refresh();
            Debug.Log("<color=green>[Último Acto]</color> ¡Todas las texturas PBR y Normal Maps generadas con éxito!");
        }

        private static void GenerateCobblestoneTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Patrón de adoquines redondeados usando combinación de senos y ruido
                    float u = (float)x / size * 8f;
                    float v = (float)y / size * 8f;

                    // Desplazamiento por filas tipo traba de adoquines
                    if (Mathf.FloorToInt(v) % 2 == 1) u += 0.5f;

                    float fracX = Mathf.Abs((u % 1f) - 0.5f) * 2f;
                    float fracY = Mathf.Abs((v % 1f) - 0.5f) * 2f;
                    float stoneDist = Mathf.Sqrt(fracX * fracX + fracY * fracY);

                    float mortar = Mathf.SmoothStep(0.65f, 0.85f, stoneDist);
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.15f;

                    float h = (1f - mortar) + noise;
                    heightMap[x, y] = h;

                    // Color: piedra húmeda oscura en el adoquín, mortero oscuro con humedad
                    Color stoneColor = Color.Lerp(new Color(0.25f, 0.26f, 0.28f), new Color(0.18f, 0.19f, 0.20f), noise * 3f);
                    Color mortarColor = new Color(0.09f, 0.09f, 0.10f);
                    Color pixelColor = Color.Lerp(stoneColor, mortarColor, mortar);

                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 2.5f);
            SaveTexture(albedo, "Tex_Cobble_Albedo.png", false);
            SaveTexture(normal, "Tex_Cobble_Normal.png", true);
        }

        private static void GenerateMarbleTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Mármol negro veneciano con vetas claras y doradas (turbulencia perlin)
                    float nx = (float)x / size;
                    float ny = (float)y / size;

                    float turb = Mathf.Sin((nx + ny) * 12f + Mathf.PerlinNoise(nx * 6f, ny * 6f) * 5f);
                    turb = Mathf.Abs(turb);
                    float vein = Mathf.Pow(1f - turb, 8f);

                    float fineVein = Mathf.Pow(1f - Mathf.Abs(Mathf.Sin((nx - ny) * 16f + Mathf.PerlinNoise(nx * 10f, ny * 10f) * 4f)), 14f);

                    heightMap[x, y] = (vein * 0.7f) + (fineVein * 0.3f);

                    Color baseMarble = new Color(0.12f, 0.12f, 0.14f);
                    Color veinColor = new Color(0.85f, 0.82f, 0.75f);
                    Color goldVein = new Color(0.75f, 0.60f, 0.35f);

                    Color pixelColor = Color.Lerp(baseMarble, veinColor, vein);
                    pixelColor = Color.Lerp(pixelColor, goldVein, fineVein * 0.8f);

                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 1.2f);
            SaveTexture(albedo, "Tex_Marble_Albedo.png", false);
            SaveTexture(normal, "Tex_Marble_Normal.png", true);
        }

        private static void GenerateParquetTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Parqué en espiga o tablas de madera noble
                    float plankWidth = 32f;
                    float plankLength = 128f;

                    int plankRow = Mathf.FloorToInt(y / plankWidth);
                    float xOffset = (plankRow % 2) * (plankLength * 0.5f);
                    int plankCol = Mathf.FloorToInt((x + xOffset) / plankLength);

                    float uInPlank = ((x + xOffset) % plankLength) / plankLength;
                    float vInPlank = (y % plankWidth) / plankWidth;

                    // Bordes de unión entre tablas
                    float edgeX = Mathf.SmoothStep(0f, 0.08f, uInPlank) * Mathf.SmoothStep(0f, 0.08f, 1f - uInPlank);
                    float edgeY = Mathf.SmoothStep(0f, 0.12f, vInPlank) * Mathf.SmoothStep(0f, 0.12f, 1f - vInPlank);
                    float edge = edgeX * edgeY;

                    // Veta de la madera longitudinal
                    float grain = Mathf.PerlinNoise((x + xOffset) * 0.02f, y * 0.15f) * 0.25f;

                    // Tonalidad diferente por cada tabla
                    float plankTint = (Mathf.Sin(plankRow * 37f + plankCol * 17f) * 0.1f);

                    heightMap[x, y] = (edge * 0.8f) + (grain * 0.2f);

                    Color baseWood = new Color(0.28f + plankTint, 0.17f + (plankTint * 0.6f), 0.11f + (plankTint * 0.4f));
                    Color darkGroove = new Color(0.08f, 0.05f, 0.03f);

                    Color pixelColor = Color.Lerp(darkGroove, baseWood * (1f - grain), edge);
                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 2.0f);
            SaveTexture(albedo, "Tex_Parquet_Albedo.png", false);
            SaveTexture(normal, "Tex_Parquet_Normal.png", true);
        }

        private static void GenerateVelvetCarpetTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // Ribete dorado en los bordes laterales (u < 0.12 o u > 0.88)
                    bool isBorder = (u < 0.12f || u > 0.88f);
                    bool isFringe = (u < 0.03f || u > 0.97f);

                    // Patrón de damasco floral en el centro
                    float damask = Mathf.Sin(u * 20f) * Mathf.Cos(v * 20f);
                    damask += Mathf.PerlinNoise(u * 15f, v * 15f) * 0.3f;
                    damask = Mathf.Abs(damask);

                    Color wineColor = Color.Lerp(new Color(0.32f, 0.06f, 0.11f), new Color(0.24f, 0.04f, 0.08f), damask * 0.4f);
                    Color goldBorder = new Color(0.78f, 0.62f, 0.32f);
                    Color darkStitch = new Color(0.15f, 0.03f, 0.05f);

                    Color pixelColor = wineColor;
                    if (isBorder)
                    {
                        pixelColor = isFringe ? darkStitch : goldBorder;
                        heightMap[x, y] = 0.8f;
                    }
                    else
                    {
                        heightMap[x, y] = damask * 0.4f;
                    }

                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 1.8f);
            SaveTexture(albedo, "Tex_VelvetCarpet_Albedo.png", false);
            SaveTexture(normal, "Tex_VelvetCarpet_Normal.png", true);
        }

        private static void GenerateWallIvoryTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // Moldura clásica (boiserie) enmarcada
                    float borderDist = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                    float frameBevel = Mathf.Sin(Mathf.Clamp01(borderDist * 16f) * Mathf.PI);

                    // Manchas de humedad y deterioro de décadas
                    float stainNoise = Mathf.PerlinNoise(u * 4f, v * 4f);
                    float plasterNoise = Mathf.PerlinNoise(u * 25f, v * 25f) * 0.08f;

                    heightMap[x, y] = frameBevel * 0.6f + plasterNoise;

                    Color ivoryColor = new Color(0.86f, 0.83f, 0.77f);
                    Color stainColor = new Color(0.68f, 0.64f, 0.55f);
                    Color mouldColor = new Color(0.92f, 0.89f, 0.82f);

                    Color pixelColor = Color.Lerp(ivoryColor, stainColor, stainNoise * 0.4f);
                    pixelColor = Color.Lerp(pixelColor, mouldColor, frameBevel * 0.3f);

                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 2.2f);
            SaveTexture(albedo, "Tex_WallIvory_Albedo.png", false);
            SaveTexture(normal, "Tex_WallIvory_Normal.png", true);
        }

        private static void GenerateFacadeTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Bloques de sillería de piedra (Ashlar)
                    float blockH = 64f;
                    float blockW = 128f;

                    int row = Mathf.FloorToInt(y / blockH);
                    float xShift = (row % 2) * (blockW * 0.5f);

                    float uBlock = ((x + xShift) % blockW) / blockW;
                    float vBlock = (y % blockH) / blockH;

                    float jointX = Mathf.SmoothStep(0f, 0.06f, uBlock) * Mathf.SmoothStep(0f, 0.06f, 1f - uBlock);
                    float jointY = Mathf.SmoothStep(0f, 0.08f, vBlock) * Mathf.SmoothStep(0f, 0.08f, 1f - vBlock);
                    float joint = jointX * jointY;

                    float stoneGrain = Mathf.PerlinNoise(x * 0.04f, y * 0.04f) * 0.2f;
                    heightMap[x, y] = joint * 0.8f + stoneGrain;

                    Color stoneLight = new Color(0.52f, 0.49f, 0.45f);
                    Color jointDark = new Color(0.20f, 0.18f, 0.16f);

                    Color pixelColor = Color.Lerp(jointDark, stoneLight * (1f - stoneGrain), joint);
                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 2.5f);
            SaveTexture(albedo, "Tex_Facade_Albedo.png", false);
            SaveTexture(normal, "Tex_Facade_Normal.png", true);
        }

        private static void GenerateMahoganyTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float grain = Mathf.PerlinNoise(u * 1.5f, v * 18f);
                    grain += Mathf.PerlinNoise(u * 6f, v * 35f) * 0.3f;

                    heightMap[x, y] = grain * 0.5f;

                    Color darkMahogany = new Color(0.20f, 0.10f, 0.07f);
                    Color warmMahogany = new Color(0.32f, 0.15f, 0.09f);

                    Color pixelColor = Color.Lerp(darkMahogany, warmMahogany, grain * 0.5f);
                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 1.5f);
            SaveTexture(albedo, "Tex_Mahogany_Albedo.png", false);
            SaveTexture(normal, "Tex_Mahogany_Normal.png", true);
        }

        private static void GenerateGreenVelvetTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    float weave = Mathf.Sin(u * 80f) * Mathf.Cos(v * 80f) * 0.15f;
                    float pile = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.25f;

                    heightMap[x, y] = weave + pile;

                    Color baseGreen = new Color(0.06f, 0.18f, 0.10f);
                    Color highlightGreen = new Color(0.12f, 0.28f, 0.16f);

                    Color pixelColor = Color.Lerp(baseGreen, highlightGreen, pile + weave);
                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 1.8f);
            SaveTexture(albedo, "Tex_GreenVelvet_Albedo.png", false);
            SaveTexture(normal, "Tex_GreenVelvet_Normal.png", true);
        }

        private static void GenerateLeatherTextures()
        {
            int size = 512;
            Texture2D albedo = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Texture2D normal = new Texture2D(size, size, TextureFormat.RGBA32, true);
            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size;
                    float v = (float)y / size;

                    // Textura de cuero gastado con microgrietas
                    float grain = Mathf.PerlinNoise(x * 0.25f, y * 0.25f) * 0.4f;
                    float wrinkle = Mathf.PerlinNoise(u * 8f, v * 8f) * 0.3f;

                    heightMap[x, y] = grain + wrinkle;

                    Color darkLeather = new Color(0.14f, 0.08f, 0.06f);
                    Color wornLeather = new Color(0.24f, 0.14f, 0.09f);

                    Color pixelColor = Color.Lerp(darkLeather, wornLeather, grain * 0.7f);
                    albedo.SetPixel(x, y, pixelColor);
                }
            }

            GenerateNormalMapFromHeights(heightMap, normal, 2.4f);
            SaveTexture(albedo, "Tex_Leather_Albedo.png", false);
            SaveTexture(normal, "Tex_Leather_Normal.png", true);
        }

        private static void GenerateNormalMapFromHeights(float[,] heights, Texture2D normalTex, float strength)
        {
            int width = normalTex.width;
            int height = normalTex.height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xLeft = (x - 1 + width) % width;
                    int xRight = (x + 1) % width;
                    int yDown = (y - 1 + height) % height;
                    int yUp = (y + 1) % height;

                    float dX = (heights[xRight, y] - heights[xLeft, y]) * strength;
                    float dY = (heights[x, yUp] - heights[x, yDown]) * strength;

                    Vector3 n = new Vector3(-dX, -dY, 1f).normalized;
                    Color nc = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                    normalTex.SetPixel(x, y, nc);
                }
            }
        }

        private static void SaveTexture(Texture2D tex, string fileName, bool isNormalMap)
        {
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = $"{TexturesDir}{fileName}";
            File.WriteAllBytes(fullPath, bytes);

            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                if (isNormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                }
                else
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.sRGBTexture = true;
                }
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
