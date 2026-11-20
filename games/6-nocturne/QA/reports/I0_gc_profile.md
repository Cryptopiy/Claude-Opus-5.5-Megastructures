# GC.Alloc in I0_gc_profile.raw

169 frames (0..168), 422 allocations, 11973 B (70.8 B/frame)

| bytes/frame | allocs/frame | thread > marker stack |
|---|---|---|
| 16.90 | 0.994 | Main Thread > Main Thread > PlayerLoop > PreUpdate.NewInputUpdate > UnityEngine.InputModule.dll!UnityEngineInternal.Input::NativeInputSystem.NotifyBeforeUpdate() [Invoke] |
| 16.90 | 0.994 | Main Thread > Main Thread > PlayerLoop > PostLateUpdate.PlayerSendFrameStarted > FrameEvents.NewInputBeforeRenderUpdate > UnityEngine.InputModule.dll!UnityEngineInternal.Input::NativeInputSystem.NotifyBeforeUpdate() [Invoke] |
| 12.00 | 0.006 | Main Thread > Main Thread > LogStringToConsole > UnityEngine.CoreModule.dll!UnityEngine::StackTraceUtility.ExtractStackTrace() [Invoke] |
| 5.95 | 0.012 | Main Thread > Main Thread > LogStringToConsole |
| 4.73 | 0.278 | Main Thread > Main Thread > PlayerLoop > FixedUpdate.NewInputFixedUpdate > UnityEngine.InputModule.dll!UnityEngineInternal.Input::NativeInputSystem.NotifyBeforeUpdate() [Invoke] |
| 4.54 | 0.030 | Main Thread > Main Thread |
| 3.96 | 0.065 | Main Thread > Main Thread > PlayerLoop > PostLateUpdate.FinishFrameRendering > UnityEngine.CoreModule.dll!UnityEngine.Rendering::RenderPipelineManager.DoRenderLoop_Internal() [Invoke] > Inl_UniversalRenderTotal > Inl_RenderCameraStack > Inl_ScriptableRenderContext.Submit > UniversalRenderPipeline.RenderSingleCameraInternal: CameraRig > ExecuteRenderGraph > UIEvents.CanvasManagerRenderOverlays > UI.RenderOverlays > UnityEngine.UIModule.dll!UnityEngine::Canvas.EndRenderExtraOverlays() [Invoke] > NocPanel.Render > UIR.DrawChain |
| 3.07 | 0.024 | Main Thread > Main Thread > PlayerLoop > PostLateUpdate.UIElementsRepaintPanels > UnityEngine.UIElementsModule.dll!UnityEngine.UIElements::UIElementsRuntimeUtilityNative.RepaintPanels() [Invoke] > NocPanel.PrepareRepaint > UIElements.UpdateRenderData > RenderTreeManager.Process > RenderTree.UpdateVisuals > UIR.ConvertEntriesToCommands |
| 1.70 | 0.071 | Main Thread > Main Thread > PlayerLoop > PostLateUpdate.UIElementsRepaintPanels > UnityEngine.UIElementsModule.dll!UnityEngine.UIElements::UIElementsRuntimeUtilityNative.RepaintPanels() [Invoke] |
| 1.09 | 0.024 | Main Thread > Main Thread > PlayerLoop > PostLateUpdate.ProfilerEndFrame > Profiler.FlushCounters > Profiler.CollectLegacyStats > Profiler.CollectUIToolkitStats |
