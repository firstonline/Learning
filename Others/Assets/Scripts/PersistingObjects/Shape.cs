using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
	[SerializeField] MeshRenderer[] m_meshRenderers;

	public int MShapeId
	{
		get { return m_shapeId; }
		set
		{
			if (m_shapeId == int.MinValue && value != int.MinValue)
			{
				m_shapeId = value;
			}
			else
			{
				Debug.LogError("Not allowed to change shape id");
			}
		}
	}

	public int ColorCount
	{
		get { return m_colors.Length; }
	}
	
	public Vector3 AngularVelocity { get; set; }
	public Vector3 Velocity { get; set; }

	public int MaterialId { get; private set; }

	private int m_shapeId = int.MinValue;
	private Color[] m_colors;
	
	static int ms_colorPropertyId = Shader.PropertyToID("_Color");
	static MaterialPropertyBlock ms_sharedPropertyBlock;

	private void Awake()
	{
		m_colors = new Color[m_meshRenderers.Length];
	}
	
	public void GameUpdate()
	{
		transform.Rotate(AngularVelocity * Time.deltaTime);
		transform.localPosition += Velocity * Time.deltaTime;
	}
	
	public void SetMaterial(Material material, int materialId)
	{
		for (int i = 0; i < m_meshRenderers.Length; i++)
		{
			m_meshRenderers[i].material = material;
		}
		MaterialId = materialId;
	}

	public void SetColour(Color color)
	{
		if (ms_sharedPropertyBlock == null)
		{
			ms_sharedPropertyBlock = new MaterialPropertyBlock();
		}
		ms_sharedPropertyBlock.SetColor(ms_colorPropertyId, color);
		for (int i = 0; i < m_meshRenderers.Length; i++)
		{
			m_colors[i] = color;
			m_meshRenderers[i].SetPropertyBlock(ms_sharedPropertyBlock);
		}
	}
	
	public void SetColour(Color color, int index)
	{
		if (ms_sharedPropertyBlock == null)
		{
			ms_sharedPropertyBlock = new MaterialPropertyBlock();
		}
		ms_sharedPropertyBlock.SetColor(ms_colorPropertyId, color);
		m_colors[index] = color;
		m_meshRenderers[index].SetPropertyBlock(ms_sharedPropertyBlock);
	}

	public override void Save(GameDataWriter writer)
	{
		base.Save(writer);
		writer.Write(m_colors.Length);

		for (int i = 0; i < m_colors.Length; i++)
		{
			writer.Write(m_colors[i]);
		}
		writer.Write(AngularVelocity);
		writer.Write(Velocity);
	}

	public override void Load(GameDataReader reader)
	{
		base.Load(reader);
		if (reader.Version >= 6)
		{
			LoadColours(reader);

		}
		else
		{
			SetColour(reader.Version > 0 ? reader.ReadColor() : Color.white);
		}
		AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
		Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
	}

	private void LoadColours(GameDataReader reader)
	{
		int count = reader.ReadInt();
		int max = count <= m_colors.Length ? count : m_colors.Length;
		int i = 0;
		for (; i < max; i++)
		{
			SetColour(reader.ReadColor(), i);
		}
	}
}
