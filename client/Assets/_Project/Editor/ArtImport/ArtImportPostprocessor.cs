using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PlowParty.Editor.ArtImport
{
    public sealed class ArtImportPostprocessor : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/_Project/Art/";
        private const string UiRoot = "Assets/_Project/Art/UI/";
        private const string PalettePath = "Assets/_Project/Art/Shared/T_Palette.png";
        private const string PaletteMaterialPath = "Assets/_Project/Art/Shared/M_Palette.mat";
        private const string PaletteMaterialName = "Palette";
        private const string SkinnedModelPrefix = "SK_";
        private const string AndroidPlatform = "Android";
        private const int WorldTextureMaxSize = 1024;
        private const int UiTextureMaxSize = 2048;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot, StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            if (assetPath == PalettePath)
            {
                ApplyPaletteSettings(importer);
                return;
            }

            var isUi = assetPath.StartsWith(UiRoot, StringComparison.Ordinal);
            var maxSize = isUi ? UiTextureMaxSize : WorldTextureMaxSize;
            importer.textureType = isUi ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.mipmapEnabled = !isUi;
            importer.maxTextureSize = maxSize;
            var android = importer.GetPlatformTextureSettings(AndroidPlatform);
            android.overridden = true;
            android.maxTextureSize = maxSize;
            android.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(android);
        }

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ArtRoot, StringComparison.Ordinal))
            {
                return;
            }

            var importer = (ModelImporter)assetImporter;
            ApplyGeometrySettings(importer);
            ApplyAnimationSettings(importer, Path.GetFileName(assetPath).StartsWith(SkinnedModelPrefix, StringComparison.Ordinal));
            RemapPaletteMaterial(importer);
        }

        private static void ApplyPaletteSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }

        private static void ApplyGeometrySettings(ModelImporter importer)
        {
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.isReadable = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        }

        private static void ApplyAnimationSettings(ModelImporter importer, bool isSkinned)
        {
            importer.animationType = isSkinned ? ModelImporterAnimationType.Generic : ModelImporterAnimationType.None;
            importer.importAnimation = isSkinned;
        }

        private void RemapPaletteMaterial(ModelImporter importer)
        {
            context.DependsOnSourceAsset(PaletteMaterialPath);
            var palette = AssetDatabase.LoadAssetAtPath<Material>(PaletteMaterialPath);
            if (palette == null)
            {
                return;
            }

            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), PaletteMaterialName), palette);
        }
    }
}
