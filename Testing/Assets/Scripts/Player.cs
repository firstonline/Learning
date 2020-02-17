using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    private Controller2D m_controller;
    private Vector2 m_velocity;

    [SerializeField] private float m_jumpHeight = 4f;
    [SerializeField] private float m_timeToJumpApex = 0.4f;
    [SerializeField] private float m_wallSlideSpeedMax = 3f;

    

    private float m_velocityXSmoothing;
    private float m_gravity;
    private float m_jumpVelocity;

    private float m_moveSpeed = 10;

    private float m_accelerationTimeAirborne = 0.2f;
    private float m_accelerationTimeGrounded = 0.1f;

    private void Start()
    {
        m_controller = GetComponent<Controller2D>();

        m_gravity = -(2 * m_jumpHeight) / Mathf.Pow(m_timeToJumpApex, 2);
        m_jumpVelocity = Mathf.Abs(m_gravity) * m_timeToJumpApex;
        Debug.Log("Gravity: " + m_gravity + " Jump Velocity: " + m_jumpVelocity);
    }

    private void Update()
    {
        bool wallSliding = true;

        if ((m_controller.Collisions.Left || m_controller.Collisions.Right) && !m_controller.Collisions.Bellow && m_velocity.y < 0)
        {
            wallSliding = true;

            if (m_velocity.y < -m_wallSlideSpeedMax)
            {
                m_velocity.y = m_wallSlideSpeedMax;
            }
        }
        
        if (m_controller.Collisions.Bellow || m_controller.Collisions.Above)
        {
            m_velocity.y = 0;
        }
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if (m_controller.Collisions.Bellow && Input.GetKeyDown(KeyCode.Space))
        {
            m_velocity.y = m_jumpVelocity;
        }

        
        float targetVelocity = input.x * m_moveSpeed;

        m_velocity.x = Mathf.SmoothDamp(m_velocity.x, targetVelocity, ref m_velocityXSmoothing, 
            (m_controller.Collisions.Bellow ? m_accelerationTimeGrounded : m_accelerationTimeAirborne));
        
        m_velocity.y += m_gravity * Time.deltaTime;
        m_controller.Move(m_velocity * Time.deltaTime);

    }
}
