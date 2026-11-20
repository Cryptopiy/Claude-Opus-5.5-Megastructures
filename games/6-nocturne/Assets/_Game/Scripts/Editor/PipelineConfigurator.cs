// PipelineConfigurator: URP asset, renderer, renderer features, default pipeline and the single quality level, imported from
// Thread and set to the Nocturne pipeline of D-008 (docs/research/R3_urp_rendergraph_instancing.md; IMPORT_PLAN 8.22):
//   PC_RPAsset:  HDR on, 64-bit colour buffer (R16G16B16A16), MSAA 4x, render scale 1, depth texture OFF and opaque texture OFF
//                (the rain reads the MSAA depth directly, D-005), ALL shadows off (14.2.3: no shadow maps; main-light and
//                additional-light shadows unsupported), main and additional lights per pixel (Forward+ cap 256 per camera),
//                SRP Batcher on, GPU Resident Drawer off, LOD cross-fade off.
//   PC_Renderer: Forward+, depth priming disabled, depth attachment Depth_32_Stencil_8 set explicitly (lesson 5; Thread left
//                DepthFormat.Default), NocRainFeature (material = Rain.mat, BeforeRenderingPostProcessing, D-005) and
//                NocGradeFeature (material = Grade.mat, AfterRenderingPostProcessing), SSAO removed, postProcessData = null (URP
//                post is never used: NocGrade is the one stack; the template's PostProcessData referenced the URP post shaders,
//                which the player build strips and logs as 16+ "stripped" lines: Phase 2 audit item 12); Look v2 (TEXTURE_PLAN
//                8.3 TP-0): NocDebugViewFeature (shader = Megastructure/NocDebugView) AFTER NocGradeFeature: the debug views of
//                TEXTURE_PLAN 7.1, no pass unless the player runs with -debug-view N; NocFacadePrepassFeature (shader =
//                Megastructure/NocFacadePrepass, TEXTURE_PLAN 2.12): the NocFacade depth prepass, switched by -facade-prepass.
//   GraphicsSettings.defaultRenderPipeline = PC_RPAsset; QualitySettings: one level "PC" using PC_RPAsset, vSync 1.
// Internal URP settings are written through SerializedObject with their m_ names. Idempotent: a second run changes nothing.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Megastructure.EditorTools
{
    public static class PipelineConfigurator
    {
        public const string GradeFeatureType = "Megastructure.NocGradeFeature";
        public const string RainFeatureType = "Megastructure.NocRainFeature";
        public const string DebugViewFeatureType = "Megastructure.NocDebugViewFeature";
        public const string DebugViewShader = "Megastructure/NocDebugView";
        public const string DebugViewFeatureShaderField = "shader";
        public const string FacadePrepassFeatureType = "Megastructure.NocFacadePrepassFeature";
        public const string FacadePrepassShader = "Megastructure/NocFacadePrepass";
        public const string GradeFeatureMaterialField = "material";
        public const string QualityLevelName = "PC";

        public static void Apply(Material grade, List<string> problems)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(NocPaths.RpAsset);
            if (asset == null) { NocLog.Problem(problems, "Pipeline: " + NocPaths.RpAsset + " not found"); return; }
            ConfigureAsset(asset);

            var rd = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(NocPaths.RendererAsset);
            if (rd == null) { NocLog.Problem(problems, "Pipeline: " + NocPaths.RendererAsset + " not found"); }
            else
            {
                ConfigureRenderer(rd);
                RemoveFeatures(rd, "ScreenSpaceAmbientOcclusion");
                var lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
                EnsureFeature(rd, RainFeatureType, lib != null ? lib.Rain : null, "Rain.mat", problems);
                EnsureFeature(rd, GradeFeatureType, grade, "Grade.mat", problems);
                // after the grade: its output pass shares AfterRenderingPostProcessing and must run after NocGrade
                EnsureFeature(rd, DebugViewFeatureType, DebugViewFeatureShaderField, MaterialFactory.FindShader(DebugViewShader), DebugViewShader, problems);
                // the NocFacade depth prepass (TEXTURE_PLAN 2.12; BeforeRenderingOpaques, so its place in the list does not matter)
                EnsureFeature(rd, FacadePrepassFeatureType, DebugViewFeatureShaderField, MaterialFactory.FindShader(FacadePrepassShader), FacadePrepassShader, problems);
                EnsureRendererIsUsed(asset, rd, problems);
            }

            GraphicsSettings.defaultRenderPipeline = asset;
            ConfigureQuality(asset, problems);
            AssetDatabase.SaveAssets();
            NocLog.Info($"Pipeline: {Describe(asset, rd)}");
        }

        public static string Describe(UniversalRenderPipelineAsset asset, UniversalRendererData rd)
        {
            return $"HDR={asset.supportsHDR} precision={asset.hdrColorBufferPrecision} MSAA={asset.msaaSampleCount} " +
                   $"depthTex={asset.supportsCameraDepthTexture} opaqueTex={asset.supportsCameraOpaqueTexture} " +
                   $"mainShadows={asset.supportsMainLightShadows} addShadows={asset.supportsAdditionalLightShadows} " +
                   $"addLights={asset.additionalLightsRenderingMode} GRD={asset.gpuResidentDrawerMode} SRPBatcher={asset.useSRPBatcher}" +
                   (rd != null ? $" mode={rd.renderingMode} priming={rd.depthPrimingMode} depth={rd.depthAttachmentFormat} postData={(rd.postProcessData != null ? rd.postProcessData.name : "null")} features={rd.rendererFeatures.Count}" : "");
        }

        static void ConfigureAsset(UniversalRenderPipelineAsset asset)
        {
            asset.supportsHDR = true;
            asset.hdrColorBufferPrecision = HDRColorBufferPrecision._64Bits;    // lesson 5, 14.12
            asset.msaaSampleCount = NocturneConstants.Msaa;
            asset.renderScale = 1f;
            asset.supportsCameraDepthTexture = false;                            // D-008
            asset.supportsCameraOpaqueTexture = false;                           // D-008
            asset.shadowDistance = 0f;                                           // 14.2.3: no shadow maps
            asset.useSRPBatcher = true;
            asset.gpuResidentDrawerMode = GPUResidentDrawerMode.Disabled;
            asset.useAdaptivePerformance = false;
            // The template points the asset at SampleSceneProfile; URP post is off on our camera, so the kept DefaultVolumeProfile is enough.
            asset.volumeProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(NocPaths.DefaultVolumeProfile);

            var so = new SerializedObject(asset);
            SetInt(so, "m_MainLightRenderingMode", (int)LightRenderingMode.PerPixel);
            SetBool(so, "m_MainLightShadowsSupported", false);
            SetInt(so, "m_AdditionalLightsRenderingMode", (int)LightRenderingMode.PerPixel);
            SetBool(so, "m_AdditionalLightShadowsSupported", false);
            SetBool(so, "m_AnyShadowsSupported", false);
            SetBool(so, "m_SoftShadowsSupported", false);
            SetBool(so, "m_RequireDepthTexture", false);
            SetBool(so, "m_RequireOpaqueTexture", false);
            SetBool(so, "m_EnableLODCrossFade", false);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        static void ConfigureRenderer(UniversalRendererData rd)
        {
            rd.renderingMode = RenderingMode.ForwardPlus;
            rd.depthPrimingMode = DepthPrimingMode.Disabled;
            rd.depthAttachmentFormat = DepthFormat.Depth_32_Stencil_8;   // D32_SFloat_S8_UInt, reversed-Z float (lesson 5)
            rd.depthTextureFormat = DepthFormat.Depth_32_Stencil_8;
            if (rd.postProcessData != null)
            {
                NocLog.Info("Pipeline: postProcessData " + rd.postProcessData.name + " -> null (NocGrade is the one post stack; audit item 12)");
                rd.postProcessData = null;
            }
            EditorUtility.SetDirty(rd);
        }

        static void EnsureRendererIsUsed(UniversalRenderPipelineAsset asset, UniversalRendererData rd, List<string> problems)
        {
            var so = new SerializedObject(asset);
            var list = so.FindProperty("m_RendererDataList");
            var def = so.FindProperty("m_DefaultRendererIndex");
            if (list == null || def == null) { NocLog.Problem(problems, "Pipeline: cannot read m_RendererDataList of the URP asset"); return; }
            int idx = -1;
            for (int i = 0; i < list.arraySize; i++) if (list.GetArrayElementAtIndex(i).objectReferenceValue == rd) idx = i;
            if (idx < 0)
            {
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = rd;
                idx = list.arraySize - 1;
            }
            def.intValue = idx;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Removes renderer features by class name (list and local-file-id map kept in sync).</summary>
        public static void RemoveFeatures(ScriptableRendererData rd, string typeName)
        {
            var so = new SerializedObject(rd);
            var list = so.FindProperty("m_RendererFeatures");
            var map = so.FindProperty("m_RendererFeatureMap");
            var doomed = new List<Object>();
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var obj = list.GetArrayElementAtIndex(i).objectReferenceValue;
                if (obj == null || obj.GetType().Name != typeName) continue;
                doomed.Add(obj);
                int before = list.arraySize;
                list.DeleteArrayElementAtIndex(i);
                if (list.arraySize == before) list.DeleteArrayElementAtIndex(i);
                if (map != null && i < map.arraySize) map.DeleteArrayElementAtIndex(i);
            }
            if (doomed.Count == 0) return;
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var o in doomed)
            {
                NocLog.Info($"Pipeline: removed renderer feature {o.name} ({typeName})");
                Object.DestroyImmediate(o, true);
            }
            rd.SetDirty();
            EditorUtility.SetDirty(rd);
            AssetDatabase.SaveAssetIfDirty(rd);
        }

        /// <summary>Adds (once) the renderer feature typeName to rd and sets its 'material' field (NocRainFeature, NocGradeFeature).</summary>
        static void EnsureFeature(UniversalRendererData rd, string typeName, Material grade, string matName, List<string> problems)
            => EnsureFeature(rd, typeName, GradeFeatureMaterialField, grade, matName, problems);

        /// <summary>Adds (once) the renderer feature typeName to rd (appended after the existing ones) and sets its object field
        /// fieldName (a material, or NocDebugViewFeature's shader).</summary>
        static void EnsureFeature(UniversalRendererData rd, string typeName, string fieldName, Object grade, string matName, List<string> problems)
        {
            var type = EditorUtil.FindType(typeName);
            if (type == null || !typeof(ScriptableRendererFeature).IsAssignableFrom(type))
            {
                NocLog.Problem(problems, $"Pipeline: renderer feature type {typeName} not found");
                return;
            }
            ScriptableRendererFeature feature = null;
            foreach (var f in rd.rendererFeatures) if (f != null && f.GetType() == type) { feature = f; break; }
            bool created = feature == null;
            if (created)
            {
                feature = (ScriptableRendererFeature)ScriptableObject.CreateInstance(type);
                feature.name = type.Name;
                AssetDatabase.AddObjectToAsset(feature, rd);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);
                var so = new SerializedObject(rd);
                var list = so.FindProperty("m_RendererFeatures");
                var map = so.FindProperty("m_RendererFeatureMap");
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = feature;
                if (map != null)
                {
                    map.arraySize++;
                    map.GetArrayElementAtIndex(map.arraySize - 1).longValue = localId;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (grade == null) NocLog.Problem(problems, $"Pipeline: {matName} missing, {type.Name}.material left unset");
            else if (!SetObjectField(feature, fieldName, grade))
                NocLog.Problem(problems, $"Pipeline: {typeName} has no serialized field '{fieldName}' of type {grade.GetType().Name}");
            feature.SetActive(true);
            EditorUtility.SetDirty(feature);
            rd.SetDirty();
            EditorUtility.SetDirty(rd);
            AssetDatabase.SaveAssetIfDirty(rd);
            NocLog.Info($"Pipeline: {(created ? "added" : "kept")} {type.Name} on {rd.name} (material {(grade != null ? grade.name : "none")})");
        }

        static bool SetObjectField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(fieldName);
            if (p != null && p.propertyType == SerializedPropertyType.ObjectReference)
            {
                p.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
                if (p.objectReferenceValue == value) return true;
            }
            var fi = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fi != null && value != null && fi.FieldType.IsAssignableFrom(value.GetType())) { fi.SetValue(target, value); return true; }
            return false;
        }

        /// <summary>One quality level "PC" using PC_RPAsset, vSync 1 (Part 8.1); every other level is removed.</summary>
        static void ConfigureQuality(UniversalRenderPipelineAsset asset, List<string> problems)
        {
            var qs = EditorUtil.LoadProjectSettingsObject("ProjectSettings/QualitySettings.asset");
            if (qs == null) { NocLog.Problem(problems, "Pipeline: cannot load ProjectSettings/QualitySettings.asset"); return; }
            var so = new SerializedObject(qs);
            var levels = so.FindProperty("m_QualitySettings");
            if (levels == null) { NocLog.Problem(problems, "Pipeline: m_QualitySettings not found"); return; }
            int keep = -1;
            for (int i = 0; i < levels.arraySize; i++)
            {
                var name = levels.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                if (name != null && name.stringValue == QualityLevelName) { keep = i; break; }
            }
            if (keep < 0) keep = 0;
            for (int i = levels.arraySize - 1; i >= 0; i--)
            {
                if (i == keep) continue;
                var name = levels.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                NocLog.Info("Pipeline: removing quality level " + (name != null ? name.stringValue : i.ToString()));
                levels.DeleteArrayElementAtIndex(i);
            }
            var level = levels.GetArrayElementAtIndex(0);
            var lname = level.FindPropertyRelative("name");
            if (lname != null) lname.stringValue = QualityLevelName;
            var rp = level.FindPropertyRelative("customRenderPipeline");
            if (rp != null) rp.objectReferenceValue = asset;
            var vs = level.FindPropertyRelative("vSyncCount");
            if (vs != null) vs.intValue = 1;
            var sh = level.FindPropertyRelative("shadows");
            if (sh != null) sh.intValue = 0;                               // ShadowQuality.Disable (14.2.3)
            var excl = level.FindPropertyRelative("excludedTargetPlatforms");
            if (excl != null && excl.isArray)
                for (int i = excl.arraySize - 1; i >= 0; i--)
                    if (excl.GetArrayElementAtIndex(i).stringValue == "Standalone") excl.DeleteArrayElementAtIndex(i);
            var cur = so.FindProperty("m_CurrentQuality");
            if (cur != null) cur.intValue = 0;
            var perPlatform = so.FindProperty("m_PerPlatformDefaultQuality");
            if (perPlatform != null) ZeroAllInts(perPlatform);
            so.ApplyModifiedPropertiesWithoutUndo();

            QualitySettings.SetQualityLevel(0, true);
            QualitySettings.vSyncCount = 1;
            if (QualitySettings.renderPipeline != asset) QualitySettings.renderPipeline = asset;
            NocLog.Info($"Pipeline: quality levels = [{string.Join(", ", QualitySettings.names)}], vSync {QualitySettings.vSyncCount}");
        }

        static void ZeroAllInts(SerializedProperty root)
        {
            var it = root.Copy();
            var end = root.GetEndProperty();
            bool enter = true;
            while (it.Next(enter) && !SerializedProperty.EqualContents(it, end))
            {
                enter = true;
                if (it.propertyType == SerializedPropertyType.Integer && it.name != "size") it.intValue = 0;
            }
        }

        static void SetBool(SerializedObject so, string name, bool v)
        {
            var p = so.FindProperty(name);
            if (p != null) p.boolValue = v; else NocLog.Warn("Pipeline: serialized field " + name + " not found");
        }

        static void SetInt(SerializedObject so, string name, int v)
        {
            var p = so.FindProperty(name);
            if (p == null) { NocLog.Warn("Pipeline: serialized field " + name + " not found"); return; }
            p.intValue = v;
        }

        /// <summary>Serialized bool of the URP asset (Validate).</summary>
        public static bool GetBool(Object o, string name, bool fallback)
        {
            var p = new SerializedObject(o).FindProperty(name);
            return p != null ? p.boolValue : fallback;
        }
    }
}
