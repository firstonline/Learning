using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
	
    public Mesh[] m_meshes;
    public Material m_material;
	public int m_maxDepth;
	public float m_childScale;
	public float m_spawnProbability;
	public float m_maxRotationSpeed;
	public float m_maxTwist;
	
	
	private float m_rotationSpeed;
	private int m_depth;

	
	private static Vector3[] m_childDirections = 
	{
		Vector3.up,
		Vector3.right,
		Vector3.left,
		Vector3.forward,
		Vector3.back,
		Vector3.down, 
	};
	private static Quaternion[] m_childOrientations = 
	{
		Quaternion.identity,
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.Euler(0f, 0f, 90f),
		Quaternion.Euler(90f, 0f, 0f),
		Quaternion.Euler(-90f, 0f, 0f),
		Quaternion.Euler(180f, 0f, 0f),
		
	};
	
	private Material[,] m_materials;
	
    private void Start()
	{
		if (m_materials == null) 
		{
			InitializeMaterials();
		}
		gameObject.AddComponent<MeshFilter>().mesh = m_meshes[Random.Range(0, m_meshes.Length)];
		gameObject.AddComponent<MeshRenderer>().material = m_materials[m_depth, Random.Range(0, 2)];
		if (m_depth < m_maxDepth)
		{
			StartCoroutine(CreateChildren());
		}

		m_rotationSpeed = Random.Range(-m_maxRotationSpeed, m_maxRotationSpeed);
		transform.Rotate(Random.Range(-m_maxTwist, m_maxTwist), 0f, 0f);
	}

	private void Update()
	{
		transform.Rotate(0f, m_rotationSpeed * Time.deltaTime, 0f);
		if (m_depth == 0)
		{
			transform.Rotate(m_rotationSpeed * Time.deltaTime, 0f, 0f);
		}
	}

	public void Initialize(Fractal parent, int childIndex)
	{
		m_spawnProbability = parent.m_spawnProbability;
		m_meshes = parent.m_meshes;
		m_materials = parent.m_materials;
		m_maxDepth = parent.m_maxDepth;
		m_depth = parent.m_depth + 1;
		m_childScale = parent.m_childScale;
		transform.parent = parent.transform;
		transform.localScale = Vector3.one * m_childScale;
		transform.localPosition = m_childDirections[childIndex] * (0.5f + 0.5f * m_childScale);
		transform.localRotation = m_childOrientations[childIndex];
		m_maxRotationSpeed = parent.m_maxRotationSpeed;
	}

	private void InitializeMaterials () 
	{
		m_materials = new Material[m_maxDepth + 1, 2];
		for (int i = 0; i <= m_maxDepth; i++) 
		{
			float t = i / (m_maxDepth - 1f);
			t *= t;
			m_materials[i, 0] = new Material(m_material);
			m_materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);
			m_materials[i, 1] = new Material(m_material);
			m_materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
		}
		m_materials[m_maxDepth, 0].color = Color.magenta;
		m_materials[m_maxDepth, 1].color = Color.red;
	}
	
	private IEnumerator CreateChildren()
	{
		int maxDirections = m_depth == 0 ? m_childDirections.Length : m_childDirections.Length - 1;
		for (int i = 0; i < maxDirections; i++) 
		{
			if (m_spawnProbability > Random.value)
			{
				yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
				new GameObject("Fractal Child").AddComponent<Fractal>().
					Initialize(this, i);
			}
		}
	}
}
