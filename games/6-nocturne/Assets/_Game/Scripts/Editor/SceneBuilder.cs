// SceneBuilder: builds Assets/_Game/Scenes/Main.unity from scratch on every RebuildAll (MASTER_PROMPT 7.1, 7.7), imported from
// Thread. Every component of the scene contract is added BY FULL TYPE NAME through reflection (IMPORT_PLAN 4.9: the strings are
// the flat Megastructure namespace; a missing type is logged as a problem, so RebuildAll fails loudly).
//   Game        GameBootstrap (materials, cityRoot, backdropRoot, mainCamera), ProgressDirector (skyFill), BeatDirector,
//               QualityBootstrap, InstancedRegistryDriver, CaptureHarness, AutoTestRunner, PerfMonitor, WallBot, SoakRunner,
//               UIDocument (NocPanel) + UiDirector (panelSettings)
//   Player      CharacterController + PlayerController + AutoWalker
//     CameraRig Camera (MainCamera, near 0.05, far 12 000, HDR, clear SolidColor = Floor, no skybox, D-008) +
//               UniversalAdditionalCameraData (URP post OFF, AA none, dithering OFF, no shadows) + CameraRig + AudioListener
//   Audio       AudioSource (no clip) + NocSynth + AudioDirector (the generator lives on an AudioSource, never on the listener)
//   SkyFill     directional light, the deck colour normalised, no shadows (LOOK_V2 6); the AtmosphereDriver sets it per beat
//   World       City, Backdrop (BackdropRig on the sky dome object NocSky; the shader positions the dome at the camera)
//   PathGuide   PathGuide (corridor colliders on layer 30 at runtime)
// RenderSettings: no skybox (D-008: URP's skybox pass would paint over the dome), flat black ambient (our shaders use the
// trilight globals), fog off (8.4), no reflections, no baked or realtime GI.
// Idempotent on disk: a fresh scene gets new random local file ids, so when the new file is structurally identical to the
// previous one (SceneFileCompare: every object, every property and every internal reference equal, ids ignored) the previous
// bytes are kept, and RebuildAll never leaves a Main.unity diff without a reason (Phase 3 integration).
using System;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Megastructure.EditorTools
{
    public static class SceneBuilder
    {
        public static readonly string[] GameComponents =
        {
            "Megastructure.GameBootstrap", "Megastructure.ProgressDirector", "Megastructure.BeatDirector",
            "Megastructure.QualityBootstrap", "Megastructure.InstancedRegistryDriver", "Megastructure.CaptureHarness",
            "Megastructure.AutoTestRunner", "Megastructure.PerfMonitor", "Megastructure.WallBot", "Megastructure.SoakRunner",
            "UnityEngine.UIElements.UIDocument", "Megastructure.UiDirector",
        };
        public static readonly string[] PlayerComponents = { "UnityEngine.CharacterController", "Megastructure.PlayerController", "Megastructure.AutoWalker" };
        public static readonly string[] CameraRigComponents = { "Megastructure.CameraRig" };
        public static readonly string[] AudioComponents = { "UnityEngine.AudioSource", "Megastructure.NocSynth", "Megastructure.AudioDirector" };
        public static readonly string[] PathGuideComponents = { "Megastructure.PathGuide" };
        public static readonly string[] RootNames = { "Game", "Player", "Audio", "SkyFill", "World", "PathGuide" };

        /// <summary>Every type name the scene contract resolves by reflection (ProjectTests checks they all exist).</summary>
        public static IEnumerable<string> AllContractTypes()
        {
            foreach (var a in new[] { GameComponents, PlayerComponents, CameraRigComponents, AudioComponents, PathGuideComponents })
                foreach (var n in a) yield return n;
        }

        public static void Build(MaterialLibrary lib, PanelSettings panel, List<string> problems)
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                NocLog.Problem(problems, "SceneBuilder: cancelled by the user (unsaved scene)");
                return;
            }
            EditorUtil.EnsureFolder(NocPaths.ScenesDir);
            string sceneAbs = NocPaths.Abs(NocPaths.MainScene);
            byte[] before = System.IO.File.Exists(sceneAbs) ? System.IO.File.ReadAllBytes(sceneAbs) : null;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (lib == null) lib = AssetDatabase.LoadAssetAtPath<MaterialLibrary>(NocPaths.LibraryAsset);
            if (panel == null) panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(NocPaths.PanelAsset);

            // ---- Sky fill (directional, no shadows)
            var fillGo = new GameObject("SkyFill");
            fillGo.transform.rotation = Quaternion.Euler(AtmosphereDriver.FillPitch, AtmosphereDriver.FillYaw, 0f);   // LOOK_V2 6: pitch -60, yaw 35
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.lightmapBakeType = LightmapBakeType.Realtime;
            fill.shadows = LightShadows.None;
            fill.intensity = (float)AtmosphereDriver.SkyFill[0];
            fill.color = AtmosphereDriver.SkyFillColourAt(0).gamma;   // the deck above as a soft key (CloudHot normalised in beat 1)
            fill.useColorTemperature = false;
            fill.cullingMask = ~0;

            // ---- World roots and the sky dome
            var world = new GameObject("World");
            var city = new GameObject("City");
            city.transform.SetParent(world.transform, false);
            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(world.transform, false);
            var dome = new GameObject("NocSky");
            dome.transform.SetParent(backdrop.transform, false);
            dome.AddComponent<MeshFilter>().sharedMesh = SkyDomeMesh(problems);
            var domeR = dome.AddComponent<MeshRenderer>();
            domeR.sharedMaterial = lib != null ? lib.Sky : null;
            if (domeR.sharedMaterial == null) NocLog.Problem(problems, "SceneBuilder: Sky material missing");
            domeR.shadowCastingMode = ShadowCastingMode.Off;
            domeR.receiveShadows = false;
            domeR.lightProbeUsage = LightProbeUsage.Off;
            domeR.reflectionProbeUsage = ReflectionProbeUsage.Off;
            var rig = dome.AddComponent<BackdropRig>();
            rig.gridStep = 0f; rig.followX = true; rig.followY = true; rig.followZ = true;   // exact follow (14.2.5 NocSky)

            // ---- Player and camera rig
            var player = new GameObject("Player");
            var camGo = new GameObject("CameraRig");
            camGo.transform.SetParent(player.transform, false);
            camGo.transform.localPosition = new Vector3(0f, (float)NocturneConstants.EyeHeight, 0f);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = NocturneConstants.CameraNear;
            cam.farClipPlane = NocturneConstants.CameraFar;
            cam.fieldOfView = (float)NocSchedule.Fov(0);
            cam.allowHDR = true;
            cam.allowMSAA = true;
            cam.allowDynamicResolution = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = FloorColour();
            cam.useOcclusionCulling = false;
            cam.depth = 0f;
            var acd = cam.GetUniversalAdditionalCameraData();
            acd.renderType = CameraRenderType.Base;
            acd.renderPostProcessing = false;      // NocGrade is the one post stack
            acd.antialiasing = AntialiasingMode.None;
            acd.dithering = false;                 // exactly one dither: NocGrade's (8.2, 14.7 H1)
            acd.renderShadows = false;             // 14.2.3
            acd.stopNaN = false;
            acd.requiresDepthOption = CameraOverrideOption.Off;
            acd.requiresColorOption = CameraOverrideOption.Off;
            acd.allowXRRendering = false;
            acd.allowHDROutput = false;
            camGo.AddComponent<AudioListener>();
            foreach (var n in CameraRigComponents) EditorUtil.AddComponentByName(camGo, n, problems);
            foreach (var n in PlayerComponents) EditorUtil.AddComponentByName(player, n, problems);

            // ---- Audio generator (AudioSource without clip; lesson 14)
            var audio = new GameObject("Audio");
            foreach (var n in AudioComponents) EditorUtil.AddComponentByName(audio, n, problems);
            var src = audio.GetComponent<AudioSource>();
            if (src != null) { src.clip = null; src.playOnAwake = false; src.loop = false; src.spatialBlend = 0f; src.volume = 1f; src.bypassReverbZones = true; }

            // ---- Path guide
            var pathGuide = new GameObject("PathGuide");
            foreach (var n in PathGuideComponents) EditorUtil.AddComponentByName(pathGuide, n, problems);

            // ---- Game systems (last, so serialized references can point at everything above)
            var game = new GameObject("Game");
            var added = new Dictionary<string, Component>();
            foreach (var n in GameComponents) added[n] = EditorUtil.AddComponentByName(game, n, problems);
            if (added.TryGetValue("Megastructure.GameBootstrap", out var bootstrap) && bootstrap != null)
            {
                var so = new SerializedObject(bootstrap);
                SetRef(so, "materials", lib, problems);
                SetRef(so, "cityRoot", city.transform, problems);
                SetRef(so, "backdropRoot", backdrop.transform, problems);
                SetRef(so, "mainCamera", cam, problems);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (added.TryGetValue("Megastructure.ProgressDirector", out var pd) && pd != null)
            {
                var so = new SerializedObject(pd);
                SetRef(so, "skyFill", fill, problems);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (added.TryGetValue("UnityEngine.UIElements.UIDocument", out var docC) && docC is UIDocument doc)
            {
                doc.panelSettings = panel;
                if (panel == null) NocLog.Problem(problems, "SceneBuilder: NocPanel.asset missing (UI cannot render)");
            }
            if (added.TryGetValue("Megastructure.UiDirector", out var ui) && ui != null)
            {
                var so = new SerializedObject(ui);
                SetRef(so, "document", docC, problems);
                SetRef(so, "panelSettings", panel, problems);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            game.transform.SetSiblingIndex(0);
            player.transform.SetSiblingIndex(1);
            audio.transform.SetSiblingIndex(2);
            fillGo.transform.SetSiblingIndex(3);
            world.transform.SetSiblingIndex(4);
            pathGuide.transform.SetSiblingIndex(5);

            ConfigureRenderSettings(fill, problems);
            RemoveUnexpectedRoots(scene);

            if (!EditorSceneManager.SaveScene(scene, NocPaths.MainScene, false))
            {
                NocLog.Problem(problems, "SceneBuilder: SaveScene failed for " + NocPaths.MainScene);
                return;
            }
            int roots = scene.rootCount;
            if (before != null && SceneFileCompare.Equivalent(System.Text.Encoding.UTF8.GetString(before), System.IO.File.ReadAllText(sceneAbs)))
            {
                // only Unity's random local ids differ: keep the previous bytes (no diff) and reload them
                System.IO.File.WriteAllBytes(sceneAbs, before);
                AssetDatabase.ImportAsset(NocPaths.MainScene, ImportAssetOptions.ForceSynchronousImport);
                EditorSceneManager.OpenScene(NocPaths.MainScene, OpenSceneMode.Single);
                NocLog.Info(FormattableString.Invariant($"SceneBuilder: {NocPaths.MainScene} unchanged ({roots} roots; structurally identical, previous file kept)"));
            }
            else
                NocLog.Info(FormattableString.Invariant($"SceneBuilder: wrote {NocPaths.MainScene} ({roots} roots{(before != null ? ", content changed" : ", new")})"));
        }

        /// <summary>The camera clear colour: the Floor token (the darkest allowed colour, 14.2.1), as stored sRGB.</summary>
        public static Color FloorColour()
        {
            var f = new Color(NocturneConstants.FloorLinR, NocturneConstants.FloorLinG, NocturneConstants.FloorLinB, 1f);
            return f.gamma;
        }

        /// <summary>A unit icosphere (subdivided) saved as an asset: the NocSky dome mesh (radius applied in the shader).</summary>
        static Mesh SkyDomeMesh(List<string> problems)
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(NocPaths.SkyMeshAsset);
            if (mesh != null) return mesh;
            var mb = new MeshBuilder();
            MeshKit.Sphere(mb, Vector3.zero, 1f, 48, 24);
            mesh = mb.ToMesh("NocSkyDome");
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2f * (float)NocturneConstants.SkyDomeRadius);
            EditorUtil.EnsureFolder(NocPaths.SettingsDir);
            AssetDatabase.CreateAsset(mesh, NocPaths.SkyMeshAsset);
            NocLog.Info(FormattableString.Invariant($"SceneBuilder: sky dome mesh {mesh.vertexCount} vertices saved to {NocPaths.SkyMeshAsset}"));
            return mesh;
        }

        static void SetRef(SerializedObject so, string field, Object value, List<string> problems)
        {
            var p = so.FindProperty(field);
            if (p == null || p.propertyType != SerializedPropertyType.ObjectReference)
            {
                NocLog.Problem(problems, FormattableString.Invariant($"SceneBuilder: {so.targetObject.GetType().Name}.{field} is not a serialized object field"));
                return;
            }
            if (value == null) NocLog.Problem(problems, FormattableString.Invariant($"SceneBuilder: {so.targetObject.GetType().Name}.{field} has no value to assign"));
            p.objectReferenceValue = value;
        }

        static void ConfigureRenderSettings(Light fill, List<string> problems)
        {
            RenderSettings.skybox = null;                                  // D-008
            RenderSettings.sun = fill;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.fog = false;                                   // 8.4: all fog comes from NocFog
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.reflectionBounces = 1;

            EditorUtil.EnsureFolder(NocPaths.SettingsDir);
            var ls = AssetDatabase.LoadAssetAtPath<LightingSettings>(NocPaths.LightingAsset);
            if (ls == null)
            {
                ls = new LightingSettings { name = "NocLighting" };
                AssetDatabase.CreateAsset(ls, NocPaths.LightingAsset);
            }
            ls.bakedGI = false;
            ls.realtimeGI = false;
            EditorUtility.SetDirty(ls);
            Lightmapping.lightingSettings = ls;
        }

        static void RemoveUnexpectedRoots(Scene scene)
        {
            foreach (var r in scene.GetRootGameObjects())
            {
                if (System.Array.IndexOf(RootNames, r.name) >= 0) continue;
                if ((r.hideFlags & HideFlags.DontSave) != 0) continue;
                NocLog.Warn(FormattableString.Invariant($"SceneBuilder: removing unexpected root object '{r.name}'"));
                Object.DestroyImmediate(r);
            }
        }
    }
    /// <summary>
    /// Structural comparison of two Unity YAML scene files that ignores the random local file ids: every document ("--- !u!C
    /// &amp;ID") is labelled by its text with the local references ("fileID: ID" of a document of the same file) replaced by the
    /// referenced document's label of the previous round (4 rounds of this refinement make a label depend on the whole
    /// neighbourhood: owner, components, parent, children); the files are equivalent when the multisets of labels are equal.
    /// External references (fileID + guid) are kept verbatim.
    /// </summary>
    public static class SceneFileCompare
    {
        static readonly System.Text.RegularExpressions.Regex Header =
            new System.Text.RegularExpressions.Regex(@"^--- !u!(\d+) &(-?\d+)(.*)$", System.Text.RegularExpressions.RegexOptions.Multiline);
        static readonly System.Text.RegularExpressions.Regex Ref = new System.Text.RegularExpressions.Regex(@"fileID: (-?\d+)");
        public const int Rounds = 4;

        public static bool Equivalent(string a, string b)
        {
            var la = Labels(a);
            var lb = Labels(b);
            if (la == null || lb == null || la.Count != lb.Count) return false;
            la.Sort(StringComparer.Ordinal);
            lb.Sort(StringComparer.Ordinal);
            for (int i = 0; i < la.Count; i++) if (la[i] != lb[i]) return false;
            return true;
        }

        /// <summary>Final labels of every document (null when the text has no documents).</summary>
        public static List<string> Labels(string text)
        {
            var ms = Header.Matches(text);
            if (ms.Count == 0) return null;
            var ids = new List<string>(ms.Count);
            var bodies = new List<string>(ms.Count);
            for (int i = 0; i < ms.Count; i++)
            {
                int start = ms[i].Index + ms[i].Length;
                int end = i + 1 < ms.Count ? ms[i + 1].Index : text.Length;
                ids.Add(ms[i].Groups[2].Value);
                bodies.Add("!u!" + ms[i].Groups[1].Value + ms[i].Groups[3].Value + text.Substring(start, end - start).Replace("\r\n", "\n"));
            }
            var index = new Dictionary<string, int>(ids.Count);
            for (int i = 0; i < ids.Count; i++) index[ids[i]] = i;
            var label = new string[ids.Count];
            for (int i = 0; i < label.Length; i++) label[i] = "";
            for (int round = 0; round <= Rounds; round++)
            {
                var next = new string[label.Length];
                for (int i = 0; i < bodies.Count; i++)
                {
                    string body = Ref.Replace(bodies[i], m => index.TryGetValue(m.Groups[1].Value, out int j)
                        ? "fileID: <" + (m.Groups[1].Value == ids[i] ? "self" : label[j]) + ">" : m.Value);
                    next[i] = Hash(body);
                }
                label = next;
            }
            return new List<string>(label);
        }

        /// <summary>64-bit FNV-1a of the UTF-16 code units, as hex.</summary>
        static string Hash(string s)
        {
            ulong h = 14695981039346656037UL;
            for (int i = 0; i < s.Length; i++) { h ^= s[i]; h *= 1099511628211UL; }
            return h.ToString("x16", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
