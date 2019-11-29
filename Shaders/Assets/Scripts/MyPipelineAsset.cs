using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipline")]
public class MyPipelineAsset : RenderPipelineAsset
{
   protected override RenderPipeline CreatePipeline()
   {
      return new MyPipeline();
   }
}
