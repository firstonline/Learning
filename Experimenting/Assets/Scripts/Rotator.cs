using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Rotator : MonoBehaviour
{
	public enum Rotation
	{
		Zero = 0,
		Quarter = 90,
		Half = 180,
		QuarterToFull = 270,
	}

	public static bool Rotating { get; set; }// TODO: make this more smart
	
	[System.Serializable]
	public class ConnectionOnRotation
	{
		public Rotation Rotation;
		public Walkable Walkable1;
		public Walkable Walkable2;
	}

	[SerializeField] private GameObject m_objectToRotate;
	[SerializeField] private List<Walkable> m_walkablesToDisableOnRotate;
	[SerializeField] private List<ConnectionOnRotation> m_connectionOnRotations;
	[SerializeField] private float m_rotationDuration = 0.2f;

	private int m_currentAngle = 0;
	
	private void OnMouseDown()
	{
		if (!Rotating && !PlayerController.Moving)
		{
			Sequence sequence =  DOTween.Sequence();
			RemoveConnectionsOnRotation();
			m_currentAngle += 90;
			if (m_currentAngle >= 360)
			{
				m_currentAngle -= 360;
			}
			Rotating = true;
			EnableColliders(false);
			sequence.Append(m_objectToRotate.transform.DORotate(new Vector3(0f, m_currentAngle, 0f), m_rotationDuration));
			sequence.AppendCallback(OnRotationDone);
		}
	}

	private void OnRotationDone()
	{
		Rotating = false;
		EnableColliders(true);
		if (m_connectionOnRotations != null)
		{
			for (int i = 0; i < m_connectionOnRotations.Count; i++)
			{
				var connection = m_connectionOnRotations[i];
				if ((int)connection.Rotation == m_currentAngle)
				{
					connection.Walkable1.AddWalkablePath(connection.Walkable2);
					connection.Walkable2.AddWalkablePath(connection.Walkable1);
				}
			}
		}
	}

	private void RemoveConnectionsOnRotation()
	{
		if (m_connectionOnRotations != null)
		{
			for (int i = 0; i < m_connectionOnRotations.Count; i++)
			{
				var connection = m_connectionOnRotations[i];
				if ((int)connection.Rotation == m_currentAngle)
				{
					connection.Walkable1.RemoveWalkablePath(connection.Walkable2);
					connection.Walkable2.RemoveWalkablePath(connection.Walkable1);
				}
			}
		}
	}
	
	private void EnableColliders(bool enable)
	{
		for (int i = 0; i < m_walkablesToDisableOnRotate.Count; i++)
		{
			m_walkablesToDisableOnRotate[i].EnableColliders(enable);
		}
	}
}
