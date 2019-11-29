using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Nucleon : MonoBehaviour
{
    public float m_attractionForce;
    private Rigidbody m_body;
    // Start is called before the first frame update
    void Awake()
    {
        m_body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        m_body.AddForce(transform.localPosition * -m_attractionForce);
    }
}
