﻿using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Packages.Rider.Editor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour
{
    public int gridSize;
    public float radius;
    
    private Color32[] m_cubeUV;
    
    private Vector3[] m_normals;
    private Vector3[] m_vertices;
    private Mesh m_mesh;

    private void Awake () 
    {
        Generate();
    }

    private void Generate () 
    {
        GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
        m_mesh.name = "Procedural Sphere";
        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(gridSize * gridSize) * 12];
        int[] trianglesX = new int[(gridSize * gridSize) * 12];
        int[] trianglesY = new int[(gridSize * gridSize) * 12];
        
        
        int ring = (gridSize + gridSize) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;

        for (int y = 0; y < gridSize; y++, v++) 
        {
            for (int q = 0; q < gridSize; q++, v++) 
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < gridSize; q++, v++) 
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            
            for (int q = 0; q < gridSize; q++, v++) 
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < gridSize - 1; q++, v++) 
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);
        
        m_mesh.subMeshCount = 3;
        m_mesh.SetTriangles(trianglesZ, 0);
        m_mesh.SetTriangles(trianglesX, 1);
        m_mesh.SetTriangles(trianglesY, 2);
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        int v = ring * gridSize;
        for (int x = 0; x < gridSize - 1; x++, v++) {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        
        int vMin = ring * (gridSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < gridSize - 1; z++, vMin --, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
            for (int x = 1; x < gridSize - 1; x++, vMid++) {
                t = SetQuad(
                    triangles, t,
                    vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            }
        
            t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1); 
        }

        int vTop = vMin - 2;

        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < gridSize - 1; x++, vMid++, vTop--)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1); 
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
    }

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int vMid = m_vertices.Length - (gridSize - 1) * (gridSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < gridSize - 1; x++, vMid++, v++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);
        
        int vMin = ring - 2;
        vMid -= gridSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
            for (int x = 1; x < gridSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
            }
            
            t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
        }
        
        
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);

        for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
        return t;
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        int faceVertices = 2 * (
                               (gridSize - 1) * (gridSize - 1)
                               + (gridSize - 1) * (gridSize - 1)
                               + (gridSize - 1) * (gridSize - 1)
                           );
        m_vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        m_normals = new Vector3[m_vertices.Length];
        m_cubeUV = new Color32[m_vertices.Length];
        
        int v = 0;
        for (int y = 0; y <= gridSize; y++) 
        {
            for (int x = 0; x <= gridSize; x++) 
            {
                SetVertices(v++, x, y, 0);
            }
            for (int z = 1; z <= gridSize; z++) 
            {
                SetVertices(v++, gridSize, y, z);
            }
            for (int x = gridSize - 1; x >= 0; x--) 
            {
                SetVertices(v++, x, y, gridSize);
            }
            for (int z = gridSize - 1; z > 0; z--) 
            {
                SetVertices(v++, 0, y, z);
            }
        }

        for (int z = 1; z < gridSize; z++)
        {
            for (int x = 1; x < gridSize; x++)
            {
                SetVertices(v++, x, gridSize, z);
            }
        }
        
        for (int z = 1; z < gridSize; z++)
        {
            for (int x = 1; x < gridSize; x++)
            {
                SetVertices(v++, x, 0, z);
            }
        }

        m_mesh.vertices = m_vertices;
        m_mesh.normals = m_normals;
        m_mesh.colors32 = m_cubeUV;
    }

    private void SetVertices(int i, int x, int y, int z)
    {
        var v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;

        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        

        m_normals[i] = s;
        m_vertices[i] = m_normals[i] * radius;
        m_cubeUV[i] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }
    
    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void CreateColliders()
    {
        gameObject.AddComponent<SphereCollider>();
    }

//    private void OnDrawGizmos()
//    {
//        if (m_vertices == null)
//        {
//            return;
//        }
//
//        for (int i = 0; i < m_vertices.Length; i++)
//        {
//            Gizmos.color = Color.black;
//            Gizmos.DrawSphere(m_vertices[i], 0.1f);
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawRay(m_vertices[i], m_normals[i]);
//        }
//    }
}
