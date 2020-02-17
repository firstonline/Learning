using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    protected struct RaycastOrigins
    {
        public Vector2 TopLeft;
        public Vector2 TopRight;
        public Vector2 BottomLeft;
        public Vector2 BottomRight;
    }
    
    [SerializeField] protected int m_horizontalRayCount = 4;
    [SerializeField] protected int m_verticalRayCount = 4;

    protected RaycastOrigins m_raycastOrigins;
    protected const float SKIN_WIDTH = 0.015f;
    protected float m_horizontalRaySpacing;
    protected float m_verticalRaySpacing;

    private BoxCollider2D m_boxCollider;

    protected virtual void Start()
    {
        m_boxCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }
    
    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = m_boxCollider.bounds;
        bounds.Expand(SKIN_WIDTH * - 2);
        
        m_raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        m_raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        m_raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        m_raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    protected void CalculateRaySpacing()
    {
        Bounds bounds = m_boxCollider.bounds;
        bounds.Expand(SKIN_WIDTH * - 2);

        m_horizontalRayCount = Mathf.Clamp(m_horizontalRayCount, 2, int.MaxValue);
        m_verticalRayCount = Mathf.Clamp(m_verticalRayCount, 2, int.MaxValue);
        m_horizontalRaySpacing = bounds.size.y / (m_horizontalRayCount - 1);
        m_verticalRaySpacing = bounds.size.x / (m_verticalRayCount - 1);
    }

}
