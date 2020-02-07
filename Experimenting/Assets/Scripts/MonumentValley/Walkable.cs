using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkable : MonoBehaviour
{
    [SerializeField] private bool m_isStair = false;
    [SerializeField] private Collider m_collider;
    [SerializeField] private List<Walkable> m_possiblePaths = new List<Walkable>();
    [SerializeField] private bool m_keepPlayerOnTopCamera;

    public bool KeepPlayerOnTop
    {
        get { return m_keepPlayerOnTopCamera; }
    }
    
    public Walkable PreviousWalkable;
    
    public List<Walkable> PossiblePaths
    {
        get { return m_possiblePaths; }
    }
    
    public Vector3 GetWalkPoint()
    {
        float offset = m_isStair ? 0.2f : 0.5f;
        return transform.position + offset * Vector3.up;
    }

    public void EnableColliders(bool enable)
    {
        
        m_collider.enabled = enable;
        if (!enable)
        {
            for (int i = 0; i < m_possiblePaths.Count; i++)
            {
                m_possiblePaths[i].RemoveWalkablePath(this);
            }
        
            m_possiblePaths.Clear();
        }
    }

    public void AddWalkablePath(Walkable walkable)
    {
        if (!m_possiblePaths.Contains(walkable))
        {
            m_possiblePaths.Add(walkable);
        }
    }

    public void RemoveWalkablePath(Walkable walkable)
    {
        m_possiblePaths.Remove(walkable);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        var myWalkPoint = GetWalkPoint();
        Gizmos.DrawSphere(GetWalkPoint(), .1f);

        if (m_possiblePaths != null)
        {
        
            for (int i = 0; i < m_possiblePaths.Count; i++)
            {
                var walkable = m_possiblePaths[i];
                if (m_possiblePaths[i] != null)
                {
                    Gizmos.color = Color.green;


                    Gizmos.DrawLine(myWalkPoint + 0.1f * walkable.transform.up, 
                        walkable.GetWalkPoint() + 0.1f * walkable.transform.up);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.transform.name);
        var walkable = other.GetComponent<Walkable>();
        if (walkable != null)
        {
            if (!walkable.m_isStair && !m_isStair && Mathf.Round(walkable.transform.position.y) != Mathf.Round(transform.position.y))
            {
                return;
            }
            var vectroBetweenPositions = walkable.transform.position - transform.position;
            // should only be distance 1
            if (Mathf.Round(Mathf.Abs(vectroBetweenPositions.x) + Mathf.Abs(vectroBetweenPositions.z)) <= 1)
            {
                m_possiblePaths.Add(walkable);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var walkable = other.GetComponent<Walkable>();
        if (walkable != null)
        {
            m_possiblePaths.Remove(walkable);
        }
    }
}
