// SceneBuilder: brings Assets/_Game/Scenes/Main.unity to the scene contract on every RebuildAll (MASTER_PROMPT 7.1, 7.7).
// Imported from Thread. Stable rebuild: the existing scene is opened and fixed IN PLACE (objects and components found by name
// and type are reused, unexpected ones removed, every value set again), so the file IDs and the YAML do not churn between
// RebuildAll runs; only a missing scene starts empty. Every component of the contract is added BY FULL TYPE NAME through
// reflection, so the editor assembly compiles while the runtime systems are still being written; a missing type is logged
// as a problem.
//   Game        GameBootstrap (materials, palette, worldRoot, sun, mainCamera), ProgressDirector, AtmosphereDriver,
//               BeatDirector, Player.QualityBootstrap, UI.UiDirector + UIDocument (PanelSettings asset), QA.CaptureHarness,
//               QA.AutoTestRunner, QA.PerfMonitor, QA.WallBot, Audio.AudioDirector, Rendering.InstancedRegistryDriver
//   Player      CharacterController (14.4) + Player.PlayerController, Player.AutoWalker
//     CameraRig Player.CameraRig + Camera (MainCamera, near 0.05, far = the beat-1 plane 150 m, HDR, solid black) +
//               UniversalAdditionalCameraData (URP post OFF: BwGrade does bloom + grade + dither; AA None; dithering off)
//               + AudioListener (the audio generator lives on Game/AudioRig, never on the camera)
//   Sun         directional light, realtime, soft shadows (AtmosphereDriver drives it)
//   World       the root of every beat builder's content
//   PathGuide   Player.PathGuide (corridor colliders on layers 30 / 31 at runtime)
// RenderSettings: no skybox (the sky is BwFarField geometry, drawn by its owner), flat black ambient (our shaders read the
// _BwAmb* trilight), Unity fog off, no reflections, no baked or realtime GI.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Megastructure.EditorTools
{
    public static class SceneBuilder
    {
        public static readonly string[] GameComponents =
        {
            typeof(GameBootstrap).FullName, typeof(ProgressDirector).FullName, typeof(AtmosphereDriver).FullName, typeof(BeatDirector).FullName,
            "Megastructure.Player.QualityBootstrap", "UnityEngine.UIElements.UIDocument", "Megastructure.UI.UiDirector",
            "Megastructure.QA.CaptureHarness", "Megastructure.QA.AutoTestRunner", "Megastructure.QA.PerfMonitor", "Megastructure.QA.WallBot",
            "Megastructure.Audio.AudioDirector", "Megastructure.Rendering.InstancedRegistryDriver",
        };
        public static readonly string[] PlayerComponents = { "UnityEngine.CharacterController", "Megastructure.Player.PlayerController", "Megastructure.Player.AutoWalker" };
        public static readonly string[] CameraRigComponents = { "Megastructure.Player.CameraRig" };
        public static readonly string[] PathGuideComponents = { "Megastructure.Player.PathGuide" };
        public static readonly string[] RootNames = { "Game", "Player", "Sun", "World", "PathGuide" };

        public static void Build(MaterialLibrary lib, BulwarkPalette palette, List<string> problems)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                BwLog.Problem(problems, "SceneBuilder: cancelled by the user (unsaved scene)");
                return;
            }
            EditorUtil.EnsureFolder(BwPaths.ScenesDir);
            // Open the existing scene and fix it in place (stable file IDs); a missing scene starts empty.
            bool existed = System.IO.File.Exists(BwPaths.Abs(BwPaths.MainScene));
            var scene = existed ? EditorSceneManager.OpenScene(BwPaths.MainScene, OpenSceneMode.Single)
                                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Opening a scene (Single) unloads unused assets: re-acquire the library and palette from disk.
            if (lib == null) lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(BwPaths.LibraryAsset);
            if (palette == null) palette = AssetDatabase.LoadAssetAtPath<BulwarkPalette>(BwPaths.PaletteAsset);
            RemoveUnexpectedRoots(scene);

            // ---- Sun (directional light; AtmosphereDriver rotates it every frame from BulwarkSchedule)
            var sunGo = Root(scene, "Sun");
            KeepChildren(sunGo.transform);
            Vector3 s0 = BulwarkSchedule.Evaluate(0).SunDir;
            sunGo.transform.localPosition = Vector3.zero;
            sunGo.transform.rotation = Quaternion.LookRotation(-s0, Vector3.up);
            sunGo.transform.localScale = Vector3.one;
            var sun = GetOrAdd<Light>(sunGo);
            sun.type = LightType.Directional;
            sun.lightmapBakeType = LightmapBakeType.Realtime;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 1f;
            sun.intensity = 1f;
            sun.color = Color.white;          // AtmosphereDriver sets colour and intensity from the schedule
            sun.useColorTemperature = false;
            sun.cullingMask = ~0;
            var sunData = sun.GetUniversalAdditionalLightData();
            sunData.usePipelineSettings = true;   // AtmosphereDriver switches to per-light biases at runtime
            sunData.softShadowQuality = SoftShadowQuality.UsePipelineSettings;
            KeepOnly(sunGo, typeof(Light), typeof(UniversalAdditionalLightData));

            // ---- World root (beat builders parent their content here; empty in the scene)
            var world = Root(scene, "World");
            KeepChildren(world.transform);
            ResetTransform(world.transform);
            KeepOnly(world);

            // ---- Player and camera rig
            var player = Root(scene, "Player");
            ResetTransform(player.transform);
            KeepChildren(player.transform, "CameraRig");
            var rig = Child(player.transform, "CameraRig");
            KeepChildren(rig.transform);
            ResetTransform(rig.transform);
            rig.transform.localPosition = new Vector3(0f, (float)FrameworkConst.EyeHeight, 0f);
            rig.tag = "MainCamera";
            var cam = GetOrAdd<Camera>(rig);
            cam.nearClipPlane = (float)FrameworkConst.CameraNear;
            cam.farClipPlane = (float)BulwarkSchedule.Far(0);
            cam.fieldOfView = (float)BulwarkSchedule.Vfov(0);
            cam.allowHDR = true;
            cam.allowMSAA = true;                  // sample count comes from the URP asset (4x)
            cam.allowDynamicResolution = false;
            cam.clearFlags = CameraClearFlags.SolidColor;   // the sky is geometry (BwFarField); never a Unity skybox
            cam.backgroundColor = Color.black;
            cam.useOcclusionCulling = false;       // no baked occlusion data in a procedural world
            cam.depth = 0f;
            var acd = cam.GetUniversalAdditionalCameraData();
            acd.renderType = CameraRenderType.Base;
            acd.renderPostProcessing = false;      // BwGrade is the one post stack (3.1 rule 6)
            acd.antialiasing = AntialiasingMode.None;
            acd.dithering = false;                 // exactly one dither: BwGrade's (8.2, 14.7 H5)
            acd.renderShadows = true;
            acd.stopNaN = false;
            acd.requiresDepthOption = CameraOverrideOption.UsePipelineSettings;
            acd.requiresColorOption = CameraOverrideOption.UsePipelineSettings;
            acd.allowXRRendering = false;
            acd.allowHDROutput = false;
            GetOrAdd<AudioListener>(rig);
            foreach (var n in CameraRigComponents) EditorUtil.AddComponentByName(rig, n, problems);
            KeepOnly(rig, Types(CameraRigComponents, typeof(Camera), typeof(UniversalAdditionalCameraData), typeof(AudioListener)));
            foreach (var n in PlayerComponents) EditorUtil.AddComponentByName(player, n, problems);
            KeepOnly(player, Types(PlayerComponents));

            // ---- Path guide
            var pathGuide = Root(scene, "PathGuide");
            KeepChildren(pathGuide.transform);
            ResetTransform(pathGuide.transform);
            foreach (var n in PathGuideComponents) EditorUtil.AddComponentByName(pathGuide, n, problems);
            KeepOnly(pathGuide, Types(PathGuideComponents));

            // ---- Game systems (last, so serialized references can point at everything above)
            var game = Root(scene, "Game");
            KeepChildren(game.transform);
            ResetTransform(game.transform);
            Component bootstrap = null;
            foreach (var n in GameComponents)
            {
                var c = EditorUtil.AddComponentByName(game, n, problems);
                if (n == typeof(GameBootstrap).FullName) bootstrap = c;
            }
            KeepOnly(game, Types(GameComponents));
            if (bootstrap != null)
            {
                var so = new SerializedObject(bootstrap);
                SetRef(so, "materials", lib, problems);
                SetRef(so, "palette", palette, problems);
                SetRef(so, "worldRoot", world.transform, problems);
                SetRef(so, "sun", sun, problems);
                SetRef(so, "mainCamera", cam, problems);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            var doc = game.GetComponent<UnityEngine.UIElements.UIDocument>();
            var panel = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>(BwPaths.PanelSettingsAsset);
            if (doc != null)
            {
                if (panel == null) BwLog.Problem(problems, "SceneBuilder: PanelSettings asset missing (" + BwPaths.PanelSettingsAsset + ")");
                doc.panelSettings = panel;
                var ui = game.GetComponent(EditorUtil.FindType("Megastructure.UI.UiDirector"));
                if (ui != null) { var uso = new SerializedObject(ui); SetRef(uso, "document", doc, problems); uso.ApplyModifiedPropertiesWithoutUndo(); }
            }
            var atmo = game.GetComponent<AtmosphereDriver>();
            if (atmo != null) { var aso = new SerializedObject(atmo); SetRef(aso, "key", sun, problems); SetRef(aso, "cam", cam, problems); aso.ApplyModifiedPropertiesWithoutUndo(); }
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) Megastructure.Player.PlayerController.ConfigureController(cc);

            // Root order in the hierarchy: Game, Player, Sun, World, PathGuide.
            game.transform.SetSiblingIndex(0);
            player.transform.SetSiblingIndex(1);
            sunGo.transform.SetSiblingIndex(2);
            world.transform.SetSiblingIndex(3);
            pathGuide.transform.SetSiblingIndex(4);

            ConfigureRenderSettings(lib, sun, problems);
            RemoveUnexpectedRoots(scene);

            if (!EditorSceneManager.SaveScene(scene, BwPaths.MainScene, false))
                BwLog.Problem(problems, "SceneBuilder: SaveScene failed for " + BwPaths.MainScene);
            else
                BwLog.Info($"SceneBuilder: wrote {BwPaths.MainScene} ({scene.rootCount} roots, {(existed ? "updated in place" : "new scene")})");
        }

        // ------------------------------------------------------------------ in-place helpers (stable file IDs)

        /// <summary>The first root object with this name (the contract's), or a new one in the open scene.</summary>
        static GameObject Root(Scene scene, string name)
        {
            foreach (var r in scene.GetRootGameObjects()) if (r.name == name) return r;
            var go = new GameObject(name);
            if (go.scene != scene) SceneManager.MoveGameObjectToScene(go, scene);
            return go;
        }

        /// <summary>The first child with this name, or a new one.</summary>
        static GameObject Child(Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; i++) if (parent.GetChild(i).name == name) return parent.GetChild(i).gameObject;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        /// <summary>Unity overloads == for destroyed / missing components, so no ?? here.</summary>
        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static void ResetTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        /// <summary>Removes every child except the first one of each allowed name (the contract has no other children).</summary>
        static void KeepChildren(Transform t, params string[] allowed)
        {
            var seen = new HashSet<string>();
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (System.Array.IndexOf(allowed, c.name) >= 0 && seen.Add(c.name)) continue;
                BwLog.Warn($"SceneBuilder: removing unexpected child '{c.name}' of '{t.name}'");
                Object.DestroyImmediate(c.gameObject);
                i--;
            }
        }

        static System.Type[] Types(string[] names, params System.Type[] extra)
        {
            var list = new List<System.Type>(extra);
            foreach (var n in names) { var t = EditorUtil.FindType(n); if (t != null) list.Add(t); }
            return list.ToArray();
        }

        /// <summary>The contract is exact: missing scripts and components of no allowed type are removed (Transform always stays).</summary>
        static void KeepOnly(GameObject go, params System.Type[] allowed)
        {
            int missing = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (missing > 0) BwLog.Warn($"SceneBuilder: removed {missing} missing script(s) from '{go.name}'");
            var comps = go.GetComponents<Component>();
            for (int i = comps.Length - 1; i >= 0; i--)   // last first: a component goes before the ones it requires
            {
                var c = comps[i];
                if (c == null || c is Transform) continue;
                bool ok = false;
                foreach (var t in allowed) if (t != null && t.IsInstanceOfType(c)) { ok = true; break; }
                if (ok) continue;
                BwLog.Warn($"SceneBuilder: removing unexpected component {c.GetType().Name} from '{go.name}'");
                Object.DestroyImmediate(c);
            }
        }

        static void SetRef(SerializedObject so, string field, Object value, List<string> problems)
        {
            var p = so.FindProperty(field);
            if (p == null || p.propertyType != SerializedPropertyType.ObjectReference)
            {
                BwLog.Problem(problems, $"SceneBuilder: {so.targetObject.GetType().Name}.{field} is not a serialized object field");
                return;
            }
            if (value == null) BwLog.Problem(problems, $"SceneBuilder: {so.targetObject.GetType().Name}.{field} has no value to assign");
            p.objectReferenceValue = value;
        }

        static void ConfigureRenderSettings(MaterialLibrary lib, Light sun, List<string> problems)
        {
            RenderSettings.skybox = null;                                 // the sky is BwFarField geometry
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.fog = false;                                   // 8.4: all fog comes from BwFogApply
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;                // no sky cubemap capture (shaders ignore it)
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.reflectionBounces = 1;

            // No lightmaps, no realtime GI (8.3): a LightingSettings asset shared by the scene.
            EditorUtil.EnsureFolder(BwPaths.SettingsDir);
            var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(BwPaths.LightingAsset);
            if (ls == null)
            {
                ls = new LightingSettings { name = "BulwarkLighting" };
                AssetDatabase.CreateAsset(ls, BwPaths.LightingAsset);
            }
            ls.bakedGI = false;
            ls.realtimeGI = false;   // (LightingSettings.autoGenerate is gone in Unity 6: lighting is never auto-generated)
            EditorUtility.SetDirty(ls);
            Lightmapping.lightingSettings = ls;
        }

        /// <summary>The scene contract is exact: objects created by edit-mode callbacks of components and duplicates of a contract
        /// root (only the first of each name is kept, the one Root() reuses) are removed.</summary>
        static void RemoveUnexpectedRoots(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var seen = new HashSet<string>();
            foreach (var r in roots)
            {
                if ((r.hideFlags & HideFlags.DontSave) != 0) continue;
                if (System.Array.IndexOf(RootNames, r.name) >= 0 && seen.Add(r.name)) continue;
                BwLog.Warn($"SceneBuilder: removing unexpected root object '{r.name}' (created in edit mode by a component, or a duplicate?)");
                Object.DestroyImmediate(r);
            }
        }
    }
}
