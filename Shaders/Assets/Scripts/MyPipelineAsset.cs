using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/My Pipline")]
public class MyPipelineAsset : RenderPipelineAsset
{

   [SerializeField] private bool m_dynamicBatching;
   [SerializeField] private bool m_instancing;
   
   protected override RenderPipeline CreatePipeline()
   {
      return new MyPipeline(m_dynamicBatching, m_instancing);
   }
}
