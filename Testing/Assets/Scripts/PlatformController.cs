using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    private struct PassengerMovement
    {
        public Transform Transform;
        public Vector2 Velocity;
        public bool StandingOnPlatform; // only used to define vertical movement up
        public bool MoveBeforePlatform;

        public PassengerMovement(Transform transform, Vector2 velocity, bool standingOnPlatform,
            bool moveBeforePlatform)
        {
            Transform = transform;
            Velocity = velocity;
            StandingOnPlatform = standingOnPlatform;
            MoveBeforePlatform = moveBeforePlatform;
        }
    }
    
    [SerializeField] private Vector3[] m_wayPoints;
    [SerializeField] private LayerMask m_passengerMask;
    [SerializeField] private float m_speed;
    [SerializeField] private bool m_cyclic;
    [SerializeField] private float m_waitTime;
    [Range(0f, 2f)]
    [SerializeField] private float m_easeAmount;
    
    private float m_nextMoveTime;
    private int m_fromWaypointIndex;
    private float m_percentageBetweenWaypoints;

    private Vector3[] m_globalWaypoints;
    private List<PassengerMovement> m_passengerMovement = new List<PassengerMovement>();
    private Dictionary<Transform, Controller2D> m_passengerControllers = new Dictionary<Transform, Controller2D>();
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        m_globalWaypoints = new Vector3[m_wayPoints.Length];
        for (int i = 0; i < m_wayPoints.Length; i++)
        {
            m_globalWaypoints[i] = m_wayPoints[i] + transform.position;
        }
    }

    private void Update()
    {
        UpdateRaycastOrigins();
        Vector3 velocity = CalculatePlatformMovement();
        CalculatePassengerMovement(velocity);
        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }

    private void OnDrawGizmos()
    {
        if (m_wayPoints != null)
        {
            Gizmos.color = Color.green;
            float size = 0.3f;
            
            for (int i = 0; i < m_wayPoints.Length; i++)
            {
                Vector3 globalWaypoint = Application.isPlaying ? m_globalWaypoints[i] : m_wayPoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypoint - Vector3.up * size, globalWaypoint + Vector3.up * size);
                Gizmos.DrawLine(globalWaypoint - Vector3.left * size, globalWaypoint + Vector3.left * size);
            }
        }
    }

    private Vector3 CalculatePlatformMovement()
    {
        if (Time.time < m_nextMoveTime)
        {
            return Vector3.zero;
        }
        m_fromWaypointIndex %= m_globalWaypoints.Length;
        int toWaypointIndex = (m_fromWaypointIndex + 1) % m_globalWaypoints.Length;
        float distanceBetweeWaypoints =
            Vector3.Distance(m_globalWaypoints[m_fromWaypointIndex], m_globalWaypoints[toWaypointIndex]);
        m_percentageBetweenWaypoints += Time.deltaTime * m_speed / distanceBetweeWaypoints;
        m_percentageBetweenWaypoints = Mathf.Clamp01(m_percentageBetweenWaypoints);

        float easedPecentBetweenWaypoints = Ease(m_percentageBetweenWaypoints);
        
        Vector3 newPos = Vector3.Lerp(m_globalWaypoints[m_fromWaypointIndex], m_globalWaypoints[toWaypointIndex], easedPecentBetweenWaypoints);

        if (m_percentageBetweenWaypoints >= 1)
        {
            m_percentageBetweenWaypoints = 0;
            m_fromWaypointIndex++;

            if (!m_cyclic)
            {
                if (m_fromWaypointIndex >= m_globalWaypoints.Length - 1)
                {
                    m_fromWaypointIndex = 0;
                    System.Array.Reverse(m_globalWaypoints);
                }
            }

            m_nextMoveTime = Time.time + m_waitTime;
        }
        
        return newPos - transform.position;
    }

    private float Ease(float x)
    {
        float a = m_easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private void MovePassengers(bool beforeMovingPlatform)
    {
        for (int i = 0; i < m_passengerMovement.Count; i++)
        {
            var passenger = m_passengerMovement[i];
            if (passenger.MoveBeforePlatform == beforeMovingPlatform)
            {
                if (!m_passengerControllers.ContainsKey(passenger.Transform))
                {
                    m_passengerControllers.Add(passenger.Transform, passenger.Transform.GetComponent<Controller2D>());
                }

                // this will prevent jumping
                if (m_passengerControllers[passenger.Transform].Collisions.Bellow)
                {
                    m_passengerControllers[passenger.Transform].Move(passenger.Velocity, passenger.StandingOnPlatform);
                }
            }
        }
    }
    
    private void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        HashSet<Transform> passengerToIgnore = new HashSet<Transform>(); // passengers that are inside of the object
        m_passengerMovement.Clear();
        
        float directionY = Mathf.Sign(velocity.y);
        float directionX = Mathf.Sign(velocity.x);


        
        if (velocity.y != 0 || velocity.x != 0 || (directionY == -1 || velocity.y == 0 && velocity.x != 0))
        {
            float rayLength = Vector3.Distance(m_raycastOrigins.BottomLeft,m_raycastOrigins.TopLeft);

            for (int i = 0; i < m_verticalRayCount; i++)
            {
                Vector2 rayOrigin = m_raycastOrigins.BottomLeft;
                rayOrigin += Vector2.right * (m_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_passengerMask);
                
                if (hit)
                {
                    if (!passengerToIgnore.Contains(hit.transform))
                    {
                        passengerToIgnore.Add(hit.transform);
                    }
                }
            }
        }
        
        // vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
            for (int i = 0; i < m_verticalRayCount; i++)
            {
                Vector2 rayOrigin = directionY == -1 ? m_raycastOrigins.BottomLeft : m_raycastOrigins.TopLeft;
                rayOrigin += Vector2.right * (m_verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_passengerMask);
                
                if (hit && !passengerToIgnore.Contains(hit.transform))
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = directionY == 1 ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;
                        
                        m_passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), 
                            directionY == 1, true));
                    }
                }
            }
        }
        
        // horizontally moving platform
//        if (velocity.x != 0)
//        {
//
//            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
//            for (int i = 0; i < m_horizontalRayCount; i++)
//            {
//                Vector2 rayOrigin = directionX == -1 ? m_raycastOrigins.BottomLeft : m_raycastOrigins.BottomRight;
//                rayOrigin += Vector2.up * (m_horizontalRaySpacing * i);
//                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_passengerMask);
//
//                if (hit && !passengerToIgnore.Contains(hit.transform))
//                {
//                    if (!movedPassengers.Contains(hit.transform))
//                    {
//                        movedPassengers.Add(hit.transform);
//
//                        float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
//                        float pushY = -SKIN_WIDTH;
//                        m_passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), 
//                            false, true));
//                    }
//                }
//            }
//        }
        
        // passenger is on top of moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = SKIN_WIDTH * 2;
            for (int i = 0; i < m_verticalRayCount; i++)
            {
                Vector2 rayOrigin = m_raycastOrigins.TopLeft + Vector2.right * (m_verticalRaySpacing * i);
                
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, m_passengerMask);
                
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);

                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        m_passengerMovement.Add(new PassengerMovement(hit.transform, new Vector2(pushX, pushY), 
                            true, false));
                    }
                }
            }
        }
    }
    
    
}
