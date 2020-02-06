using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private GameObject m_objectToManipulate;
    [SerializeField] private List<Walkable> m_walkablesToDisableOnRotate;

    private bool m_triggered;

    private void Awake()
    {
        for (int i = 0; i < m_walkablesToDisableOnRotate.Count; i++)
        {
            m_walkablesToDisableOnRotate[i].EnableColliders(false);
        }
    }
    
    private void OnTriggerEnter(Collider collider)
    {
        if (!m_triggered)
        {
            m_triggered = true;
            // can only collide with player
            Sequence sequence = DOTween.Sequence();
            sequence.Append(m_objectToManipulate.transform.DORotate(new Vector3(0f, 0f, 90f), 0.2f));
            sequence.AppendCallback(OnActionDone);
        }
    }

    private void OnActionDone()
    {
        for (int i = 0; i < m_walkablesToDisableOnRotate.Count; i++)
        {
            m_walkablesToDisableOnRotate[i].EnableColliders(true);
        }
    }
}
