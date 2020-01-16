using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
	public Material errorMaterial;

	private bool m_dynamicBatching;
	private bool m_instancing;	// GPU instancing (CPU tells the GPU to draw a specific mesh-material combination more than once via a single draw call)
	private const int m_maxVisibleLights = 4;

	private static int m_visibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
	private static int m_visibleLightDirectionsOrPositionsId =
		Shader.PropertyToID("_VisibleLightDirectionsOrPositions");
	private static int m_visibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
	private static int m_visibleLightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
	
	
	private Vector4[] m_visibleLightColors = new Vector4[m_maxVisibleLights];
	private Vector4[] m_visibleLightDirectionsOrPositions = new Vector4[m_maxVisibleLights];
	private Vector4[] m_visibleLightAttenuations = new Vector4[m_maxVisibleLights];
	private Vector4[] m_visibleLightSpotDirections = new Vector4[m_maxVisibleLights];
	
	public MyPipeline(bool dynamicBatching, bool instancing)
	{
		GraphicsSettings.lightsUseLinearIntensity = true;
		m_dynamicBatching = dynamicBatching;
		m_instancing = instancing;
	}
	
	private CommandBuffer m_commandBuffer = new CommandBuffer
	{
		name = "Render Camera"
	};
	
	protected override void Render(ScriptableRenderContext context, Camera[] cameras)
	{
		for (int i = 0; i < cameras.Length; i++)
		{
			Render(context, cameras[i]);
		}
	}

	private void Render(ScriptableRenderContext context, Camera camera)
	{

		ScriptableCullingParameters cullingParameters;
		if (!camera.TryGetCullingParameters(out cullingParameters))
		{
			return;
		}
		
#if UNITY_EDITOR // condition needed since EmitWorldGeometryForSceneView doesn't exist in builds
		
		// only add to scene view since in game view it is already added by default
		if (camera.cameraType == CameraType.SceneView)
		{
			// Add ui to the scene view
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif		
		// get culling from the camera
		var cullResults = context.Cull(ref cullingParameters);
		
		// clear previous draw stuff
		m_commandBuffer.ClearRenderTarget(true, false, Color.clear);

		ConfigureLights(cullResults);
		
		// make "Render Camera" appear in the profiler to easier CPU usage tracking
		m_commandBuffer.BeginSample("Render Camera");
		m_commandBuffer.SetGlobalVectorArray(m_visibleLightColorsId, m_visibleLightColors);
		m_commandBuffer.SetGlobalVectorArray(m_visibleLightDirectionsOrPositionsId, m_visibleLightDirectionsOrPositions);
		m_commandBuffer.SetGlobalVectorArray(m_visibleLightAttenuationsId, m_visibleLightAttenuations);
		m_commandBuffer.SetGlobalVectorArray(m_visibleLightSpotDirectionsId, m_visibleLightSpotDirections);

		// execute stored commands
		context.ExecuteCommandBuffer(m_commandBuffer);
		
		// clear all stored commands
		m_commandBuffer.Clear();
		
		
		// make properties rely on camera
		context.SetupCameraProperties(camera);

		// settings up drawing settings for objects with opaque Unlit shaders
		SortingSettings sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
		var drawSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
		drawSettings.enableDynamicBatching = m_dynamicBatching;
		
		// settings up filters for objects with opaque shaders 
		var filterSettings = FilteringSettings.defaultValue;
		filterSettings.renderQueueRange = RenderQueueRange.opaque;

		// draw the objects
		context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);
		
		
		context.DrawSkybox(camera);
		
		// change settings to draw transparent objects
		sortingSettings.criteria = SortingCriteria.CommonTransparent;
		drawSettings.sortingSettings = sortingSettings;
		
//		We draw the opaque renderers before the skybox to prevent overdraw. As the shapes will always be in front of 
//		the skybox, we avoid work by drawing them first. That's because the opaque shader pass writes to the depth 
//		buffer, which is used to skip anything that's drawn later that ends up further away
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(
			cullResults, ref drawSettings, ref filterSettings
		);

		// draw objects that dont have unusable unity shaders (used just to display broken objects added to the scene
		DrawDefaultPipeline(context, camera, cullResults);
		
		// end Render Camera area in profiler
		m_commandBuffer.EndSample("Render Camera");
		context.ExecuteCommandBuffer(m_commandBuffer);
		m_commandBuffer.Clear();
		
		// submit context to render
		context.Submit();
	}

	
	// conditional functions are only executed when conditions are met
	[Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
	private void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera, CullingResults cull)
	{
		if (errorMaterial == null)
		{
			Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
			errorMaterial = new Material(errorShader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		SortingSettings sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
		var drawSettings = new DrawingSettings(new ShaderTagId("ForwardBase"), sortingSettings);
		
		drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
		drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
		drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
		drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
		drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));
		drawSettings.overrideMaterial = errorMaterial;
		
		var filterSettings = FilteringSettings.defaultValue;
		
		context.DrawRenderers(cull, ref drawSettings, ref filterSettings);
	}

	private void ConfigureLights(CullingResults cull)
	{
		int i = 0;
		for (; i < cull.visibleLights.Length && i < m_maxVisibleLights; i++)
		{
			VisibleLight light = cull.visibleLights[i];
			m_visibleLightColors[i] = light.finalColor;

			Vector4 attenuation = Vector4.zero;
			if (light.lightType == LightType.Directional)
			{
				// index 0 - x vector
				// index 1 - y vector
				// index 2 - z vector
				// index 3 - position
				Vector4 v = light.localToWorldMatrix.GetColumn(2);
				v.x = -v.x;
				v.y = -v.y;
				v.z = v.z;
				m_visibleLightDirectionsOrPositions[i] = v;
			}
			else
			{
				m_visibleLightDirectionsOrPositions[i] = light.localToWorldMatrix.GetColumn(3);
				attenuation.x = 1f / Mathf.Max(light.range * light.range, 0.00001f);

				if (light.lightType == LightType.Spot)
				{
					Vector4 v = light.localToWorldMatrix.GetColumn(2);
					
				}
			}

			m_visibleLightAttenuations[i] = attenuation;
		}

		for (; i < m_maxVisibleLights; i++)
		{
			m_visibleLightColors[i] = Color.clear;
		}
	}
}
