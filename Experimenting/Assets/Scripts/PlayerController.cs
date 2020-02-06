using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static bool Moving { get; set; }

    [SerializeField] private Walkable m_currentCube;
    [SerializeField] private Walkable m_clickedCube;
    [SerializeField] private Transform m_playerTransform;
    [SerializeField] private List<Walkable> m_path;
    [SerializeField] private Camera m_camera;
    [SerializeField] private int m_topWalkablesCollision;
    
    private int m_walkableMask = 1 << 8 | 1 << 9;
    private int m_layerTop = 9;
    private int m_playerCullingMask = 1 << 10;
    private bool m_onTopCamera;
    
    private void Start()
    {
        GetCurrentCube();
        transform.SetParent(m_currentCube.transform);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TouchRaycast(Input.mousePosition);
        }
    }

    private void TouchRaycast(Vector3 mousePosition)
    {
        if (!Moving && !Rotator.Rotating)
        {
            var screenRay = m_camera.ScreenPointToRay(mousePosition);
            RaycastHit touchRaycastHit;

            if (Physics.Raycast(screenRay, out touchRaycastHit, Mathf.Infinity, m_walkableMask))
            {
                var walkable = touchRaycastHit.transform.GetComponent<Walkable>();
                if (walkable)
                {
                    m_path = PathFinder.FindPath(m_currentCube, walkable);
                    if (m_path.Count > 0)
                    {
                        m_clickedCube = walkable;
                        FollowPath();
                    }
                }
               
            }
        }
    }

    private void FollowPath()
    {
        Moving = true;
        transform.SetParent(null);
        Sequence  m_walkSequence = DOTween.Sequence();
        for (int i = 0; i < m_path.Count; i++)
        {
            m_walkSequence.Append(transform.DOMove(m_path[i].GetWalkPoint(), 0.1f).SetEase(Ease.Linear));
        }

        m_walkSequence.AppendCallback(Clear);
    }

    private void Clear()
    {
        Moving = false;
        if (m_path != null)
        {
            for (int i = 0; i < m_path.Count; i++)
            {
                m_path[i].PreviousWalkable = null;
            }
            m_path = null;
        }

        m_currentCube = m_clickedCube;
        transform.SetParent(m_currentCube.transform);
    }
    
    private void GetCurrentCube()
    {
        Ray ray = new Ray(m_playerTransform.position, -transform.up);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, m_walkableMask))
        {
            m_currentCube = raycastHit.transform.GetComponent<Walkable>();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        var walkable = collider.GetComponent<Walkable>();
         if (walkable)
         {
             if (walkable.KeepPlayerOnTop)
             {
                 m_topWalkablesCollision++;
                 if (!m_onTopCamera)
                 {
                     m_camera.cullingMask ^= m_playerCullingMask;
                     m_onTopCamera = true;
                 }
             }
             else if (m_topWalkablesCollision == 0)
             {
                 if (m_onTopCamera)
                 {
                     m_onTopCamera = false;
                     m_camera.cullingMask &= ~m_playerCullingMask;
                 }
             }
         }
    }

    private void OnTriggerExit(Collider collider)
    {   
        var walkable = collider.GetComponent<Walkable>();
        if (walkable)
        {
            if (walkable.KeepPlayerOnTop)
            {
                m_topWalkablesCollision--;
            }
        }
    }
}
