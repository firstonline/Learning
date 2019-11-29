using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
	private Mesh m_deformingMesh;
	public float springForce = 20f;
	public float damping = 5f;

	private Vector3[] m_originalVertices, m_displacedVertices;
	private Vector3[] m_vertexVelocities;
	private float m_uniformScale;
	
	private void Start()
	{
		m_deformingMesh = GetComponent<MeshFilter>().mesh;
		m_originalVertices = m_deformingMesh.vertices;
		m_displacedVertices = new Vector3[m_originalVertices.Length];
		for (int i = 0; i < m_displacedVertices.Length; i++)
		{
			m_displacedVertices[i] = m_originalVertices[i];
		}
		
		m_vertexVelocities = new Vector3[m_originalVertices.Length];
	}

	private void Update()
	{
		m_uniformScale = transform.localScale.x;
		for (int i = 0; i < m_displacedVertices.Length; i++)
		{
			UpdateVertexes(i);
		}

		m_deformingMesh.vertices = m_displacedVertices;
		m_deformingMesh.RecalculateNormals();
	}

	public void AddDeformingForce(Vector3 point, float force)
	{
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < m_displacedVertices.Length; i++)
		{
			AddForceToVertex(i, point, force);
		}
	}

	private void AddForceToVertex(int i, Vector3 point, float force)
	{
		Vector3 pointToVertex = m_displacedVertices[i] - point;
		pointToVertex *= m_uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		m_vertexVelocities[i] += pointToVertex.normalized * velocity;
	}

	private void UpdateVertexes(int i)
	{
		Vector3 velocity = m_vertexVelocities[i];
		Vector3 displacement = m_displacedVertices[i] - m_originalVertices[i];
		displacement *= m_uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		m_vertexVelocities[i] = velocity;
		m_displacedVertices[i] += velocity * (Time.deltaTime / m_uniformScale);
	}
}
