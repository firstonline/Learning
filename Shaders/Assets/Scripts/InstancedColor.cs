using UnityEngine;


// this class is to handle colors on the material. this 
// will enable one material to be able to have multiple colors
// in the game with the need to create new material. this will also
// make GPU instancing or batching work better
public class InstancedColor : MonoBehaviour
{
    [SerializeField] private Color m_color = Color.white;
	private static MaterialPropertyBlock m_propertyBlock;
	private static int m_colorID = Shader.PropertyToID("_Color");

    private void Awake()
    {
       OnValidate();
    }

    private void OnValidate()
    {
		if (m_propertyBlock == null)
		{
			m_propertyBlock = new MaterialPropertyBlock();
		}
		m_propertyBlock.SetColor(m_colorID, m_color);
        GetComponent<MeshRenderer>().SetPropertyBlock(m_propertyBlock);
    }
}
