using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
    public struct CollisionInfo
    {
        public bool Above, Bellow;
        public bool Left, Right;
        public bool ClimbingSlope;
        public float SlopAngleOld;
        public float SlopAngle;
        public bool DescendingSlope;
        public Vector2 VelocityOld;

        public void Reset()
        {
            Above = Bellow = Left = Right = false;
            SlopAngleOld = SlopAngle;
            ClimbingSlope = false;
            DescendingSlope = true;
            SlopAngle = 0;
        }
    }
    
    [SerializeField] protected LayerMask m_collisionMask;
    
    private float m_maxClimbAngle = 80f;
    private float m_maxDescendingAngle = 80f;
  
    public CollisionInfo Collisions;


    protected override void Start()
    {
        base.Start();
    }

    public void Move(Vector2 velocity, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        Collisions.Reset(); 
        
        Collisions.VelocityOld = velocity;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }
        
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);

        if (standingOnPlatform)
        {
            Collisions.Bellow = true;
        }
    }
    
    
    private void HorizontalCollisions(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        
        for (int i = 0; i < m_horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? m_raycastOrigins.BottomLeft : m_raycastOrigins.BottomRight;
            rayOrigin += Vector2.up * (m_horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_collisionMask);
            
            Debug.DrawRay(rayOrigin, Vector3.right * directionX * rayLength, Color.red);
            
            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }
                
                // ignore moving platfroms
                if (hit.transform.tag == "MovingPlatform") 
                {
                    continue;
                }
                
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= m_maxClimbAngle)
                {
                    if (Collisions.DescendingSlope)
                    {
                        Collisions.DescendingSlope = false;
                        velocity = Collisions.VelocityOld;

                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != Collisions.SlopAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - SKIN_WIDTH;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                    
                }

                if (!Collisions.ClimbingSlope || slopeAngle > m_maxClimbAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    if (Collisions.ClimbingSlope)
                    {
                        velocity.y = Mathf.Tan(Collisions.SlopAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }
                    
                    Collisions.Left = directionX == -1;
                    Collisions.Right = directionX == 1;
                }
            }
        }
    }

    private void VerticalCollisions(ref Vector2 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < m_verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? m_raycastOrigins.BottomLeft : m_raycastOrigins.TopLeft;
            rayOrigin += Vector2.right * (m_verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_collisionMask);
            
            Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.red);
            
            if (hit)
            {
                // ignore collision when we are inside a platform, shouldn't happen but just in case
                if (hit.distance == 0)
                {
                    continue;
                }
                // if we are going up ignore moving platfroms
                if (velocity.y > 0 && hit.transform.tag == "MovingPlatform") 
                {
                    continue;
                }
                
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                
                rayLength = hit.distance;
                
                if (Collisions.ClimbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(Collisions.SlopAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                
                Collisions.Bellow = directionY == -1;
                Collisions.Above = directionY == 1;
            }
        }

        
        if (Collisions.ClimbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
            Vector2 rayOrigin = (directionX == -1 ? m_raycastOrigins.BottomLeft : m_raycastOrigins.BottomLeft) +Vector2.up * velocity.y ;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != Collisions.SlopAngle)
                {
                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    Collisions.SlopAngle = slopeAngle;
                }
            }
        }

        
    }

    private void ClimbSlope(ref Vector2 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            Collisions.Bellow = true;
            Collisions.ClimbingSlope = true;
            Collisions.SlopAngle = slopeAngle;
        }
    }
    
    private void DescendSlope(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = directionX == -1 ? m_raycastOrigins.BottomRight : m_raycastOrigins.BottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, m_collisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= m_maxDescendingAngle) 
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - SKIN_WIDTH <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;
                        Collisions.SlopAngle = slopeAngle;
                        Collisions.DescendingSlope = true;
                        Collisions.Bellow = true;
                    }
                }
            }
        }
    }

    
}
