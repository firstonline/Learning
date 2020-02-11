using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class Axe : MonoBehaviour
{
    [SerializeField] private Transform m_parent;
    [SerializeField] private Transform m_curevePoint;
    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private float m_rotationSpeed = 10f;
    [SerializeField] private float m_returnTime = 2f;
    [SerializeField] private GameObject m_particlesOnCatch;
    [SerializeField] private CinemachineImpulseSource m_inpulseSource;
    [SerializeField] private GameObject m_trail;
    [SerializeField] private float m_throwRotationRandom = 20f;

    private Vector3 m_landPosition;
    private Quaternion m_allignmentAngle;
    private Quaternion m_origRotation;
    private Vector3 m_originalPosition;
    private bool m_isReturning;
    private bool m_flying;
    private bool m_alligning = false;
    private float m_currentReturningTime;
    private Action m_onAxeReturnedCallback;

    private float m_handAdjustmentTime;
    private float m_handAdjustmentDuration;
    
    private void Awake()
    {
        m_originalPosition = transform.localPosition;
        m_origRotation = transform.localRotation;

        m_handAdjustmentTime = 0.9f * m_returnTime;
        m_handAdjustmentDuration = 0.1f * m_returnTime;
    }

    private void Update()
    {
        if (m_flying)
        {
            transform.Rotate(m_rotationSpeed * Time.deltaTime, 0, 0);
        }
        else if (m_isReturning)
        {
            m_currentReturningTime += Time.deltaTime;

            if (m_currentReturningTime > m_handAdjustmentTime)
            {
                if (!m_alligning)
                {
                    m_alligning = true;
                    m_allignmentAngle = transform.localRotation;
                    m_rigidBody.isKinematic = true;
                }
                else
                {
                    transform.SetParent(m_parent, true);
                    float a = m_currentReturningTime - m_handAdjustmentTime;
                    float b = m_handAdjustmentDuration;
                    float delta = a / b;
                    transform.localRotation = Quaternion.Lerp(m_allignmentAngle,
                        m_origRotation, delta);
                }
            }
            else
            {
                transform.Rotate(m_rotationSpeed * Time.deltaTime, 0, 0);
            }
            
            if (m_currentReturningTime >= m_returnTime)
            {
                m_currentReturningTime = m_returnTime;
                m_isReturning = false;
                transform.SetParent(m_parent, true);
                transform.localPosition = m_originalPosition;
                transform.localRotation = m_origRotation;
                m_onAxeReturnedCallback();
                m_particlesOnCatch.gameObject.SetActive(true);
                m_inpulseSource.GenerateImpulse(Vector3.right);
                m_rigidBody.isKinematic = true;
                m_trail.gameObject.SetActive(false);
            }
            else
            {
                transform.position = GetBezierCurve(m_currentReturningTime / m_returnTime,  m_landPosition,m_curevePoint.position, m_parent.position);
            }

           
        }
    }

    public void FlyForward()
    {
        m_particlesOnCatch.gameObject.SetActive(false);
        m_flying = true;
        m_rigidBody.isKinematic = false;
        transform.parent = null;
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
        transform.Rotate(transform.forward, Random.Range(-m_throwRotationRandom, m_throwRotationRandom));
        var lookRotation = Quaternion.LookRotation(Camera.main.transform.forward, transform.up);
        transform.rotation = lookRotation;
        
        m_rigidBody.AddForce(Camera.main.transform.forward * 20f, ForceMode.Impulse);
        m_trail.gameObject.SetActive(true);
    }

    public void ReturnAxe(Action onAxeReturnedCallback)
    {
        m_isReturning = true;
        m_flying = false;
        m_rigidBody.isKinematic = false;
        m_rigidBody.velocity = Vector3.zero;
        m_currentReturningTime = 0f;
        var lookRotation = Quaternion.LookRotation(Camera.main.transform.forward, transform.up);
        transform.rotation = lookRotation;
        m_landPosition = transform.position;
        m_onAxeReturnedCallback = onAxeReturnedCallback;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        m_flying = false;
        m_rigidBody.isKinematic = true;
    }

    Vector3 GetBezierCurve(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return p;
    }
}
