using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    // since we dont have proper animation blending yet everything is done through states
    public enum PlayerState
    {
        Idle,
        Walking,
        Aiming,
        Throwing,
        CallingAxe,
    }
    
    [SerializeField] private Axe m_axe;
    [SerializeField] private Animator m_animator;
    [SerializeField] private float m_maxMoveSpeed = 4;
    [SerializeField] private float m_acceleration = 4;
    [SerializeField] private float m_yawRotationSpeed = 90;
    [SerializeField] private Image m_crossHair;

    private PlayerState m_currentState;

    private int WALKING_BOOL = Animator.StringToHash("walking");
    private int FORWARD_MULTIPLIER = Animator.StringToHash("forwardMultiplier");
    private int AIMING_BOOL = Animator.StringToHash("aiming");
    private int CALLING_AXE_BOOL = Animator.StringToHash("callingAxe");
    private int THROW_TRIGGER = Animator.StringToHash("throw");

    private bool m_axeThrown = false;

    private bool m_cancelledThrow;
    
    private float m_currentSpeed;
    private Vector3 m_movementVector;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        switch (m_currentState)
        {
            case PlayerState.Idle:
                if (!m_axeThrown && CheckForAimInput())
                {
                    SwitchState(PlayerState.Aiming);
                }
                else if (m_axeThrown && CheckForCallAxeInput())
                {
                    SwitchState(PlayerState.CallingAxe);
                }
                else if (CheckForMovementInput())
                {
                    SwitchState(PlayerState.Walking);
                }

               
                break;
            case PlayerState.Walking:
                if (!m_axeThrown && CheckForAimInput())
                {
                    SwitchState(PlayerState.Aiming);
                }
                else if (m_axeThrown && CheckForCallAxeInput())
                {
                    SwitchState(PlayerState.CallingAxe);
                }
                else if (!CheckForMovementInput())
                {
                    SwitchState(PlayerState.Idle);
                }
                break;
            case PlayerState.Aiming:
                if (CheckForAimInputRelease())
                {
                    if (m_cancelledThrow)
                    {
                        SwitchState(PlayerState.Idle);
                    }
                    else
                    {
                        SwitchState(PlayerState.Throwing);
                    }
                }
                break;
            case PlayerState.Throwing:
                break;
            case PlayerState.CallingAxe:
                break;
        }
        
        float yawRotation = Input.GetAxis("Mouse X");
        transform.Rotate(transform.up, yawRotation * m_yawRotationSpeed * Time.deltaTime);
    }

    private bool CheckForAimInput()
    {
        bool hasInput = Input.GetMouseButtonDown(0);
        return hasInput;
    }
    
    private bool CheckForCallAxeInput()
    {
        bool hasInput = Input.GetMouseButtonDown(1);
        return hasInput;
    }


    // should only be called in aim state
    private bool CheckForAimInputRelease()
    {
        bool hasInput = false;
        if (Input.GetMouseButtonDown(1))
        {
            hasInput = true;
            m_cancelledThrow = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            hasInput = true;
            m_cancelledThrow = false;
        }

        return hasInput;
    }
    
    private void SwitchState(PlayerState newState)
    {
        Debug.Log("New State: " + newState);
        m_currentState = newState;

        switch (newState)
        {
            case PlayerState.Idle:
                m_animator.SetBool(WALKING_BOOL, false);
                m_animator.SetBool(AIMING_BOOL, false);
                m_animator.SetBool(CALLING_AXE_BOOL, false);
                m_crossHair.gameObject.SetActive(false);
                break;
            case PlayerState.Walking:
                m_currentSpeed = 0;
                m_animator.SetBool(WALKING_BOOL, true);
                break;
            case PlayerState.Aiming:
                m_animator.SetBool(WALKING_BOOL, false);
                m_animator.SetBool(AIMING_BOOL, true);
                m_crossHair.gameObject.SetActive(true);
                break;
            case PlayerState.Throwing:
                m_axeThrown = true;
                m_animator.SetTrigger(THROW_TRIGGER);
                m_crossHair.gameObject.SetActive(false);
               // m_animator.SetBool(AIMING_BOOL, false);
                break;
            case PlayerState.CallingAxe:
                m_animator.SetBool(CALLING_AXE_BOOL, true);
                m_axe.ReturnAxe(OnAxeReturned);
                break;
        }
    }
    
    public void ThrowAnimationComplete()
    {
        Debug.Log("Throw down");
        SwitchState(PlayerState.Idle);
    }

    private void OnAxeReturned()
    {
        Debug.Log("Axe returned");
        m_axeThrown = false;
        SwitchState(PlayerState.Idle);
    }
    
    private bool CheckForMovementInput()
    {
        bool hasInput = false;
        float sideMovement = Input.GetAxis("Horizontal");
        float straightMovement = Input.GetAxis("Vertical");


        if (sideMovement != 0f || straightMovement != 0f)
        {
            m_movementVector = transform.forward * straightMovement + transform.right * sideMovement;
            m_movementVector.Normalize();

            m_currentSpeed += m_acceleration * Time.deltaTime;
            if (m_currentSpeed > m_maxMoveSpeed)
            {
                m_currentSpeed = m_maxMoveSpeed;
            }
            
            m_animator.SetFloat(FORWARD_MULTIPLIER, straightMovement > 0 ? 1f : -1f);

            // TODO: velocity instead
            transform.position += m_movementVector * m_currentSpeed * Time.deltaTime;
            hasInput = true;

        } 
        else
        {
            hasInput = false;
        }

        return hasInput;
    }


    public void ThrowAxe()
    {
        m_axe.FlyForward();
    }
}
