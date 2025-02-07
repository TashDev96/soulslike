using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class Test : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
	    private Material _blitMaterial;

	    public void Setup(Material blitMaterial)
	    {
		    _blitMaterial = blitMaterial;
		    requiresIntermediateTexture = true;
	    } // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {


	        var resourceData = frameData.Get<UniversalResourceData>();

	        if(resourceData.isActiveTargetBackBuffer)
	        {
		        Debug.LogWarning("Back buffer is active");
		        return;
	        }

	        var source = resourceData.activeColorTexture;
	        var destinationDesc = renderGraph.GetTextureDesc(source);
	        destinationDesc.name = "Test";
	        destinationDesc.clearBuffer = false;

	        var destination = renderGraph.CreateTexture(destinationDesc);
	        
	        RenderGraphUtils.BlitMaterialParameters param = new(source, destination, _blitMaterial,0);
			renderGraph.AddBlitPass(param, "Test Pass");
	        
	        resourceData.cameraColor = destination;
        }

        
    }

    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    public Material material;
    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = renderPassEvent;
        
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
	    m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
        
    }
}
