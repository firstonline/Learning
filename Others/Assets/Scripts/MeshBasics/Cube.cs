using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Packages.Rider.Editor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    public int xSize, ySize, zSize;
    public int roundness;
    
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
        m_mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(xSize * ySize) * 12];
        int[] trianglesX = new int[(ySize * zSize) * 12];
        int[] trianglesY = new int[(xSize * zSize) * 12];
        
        
        int ring = (xSize + zSize) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;

        for (int y = 0; y < ySize; y++, v++) 
        {
            for (int q = 0; q < xSize; q++, v++) 
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize; q++, v++) 
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            
            for (int q = 0; q < xSize; q++, v++) 
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize - 1; q++, v++) 
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
        int v = ring * ySize;
        for (int x = 0; x < xSize - 1; x++, v++) {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);
        
        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin --, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
            for (int x = 1; x < xSize - 1; x++, vMid++) {
                t = SetQuad(
                    triangles, t,
                    vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }
        
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1); 
        }

        int vTop = vMin - 2;

        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < xSize - 1; x++, vMid++, vTop--)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1); 
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
    }

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int vMid = m_vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < xSize - 1; x++, vMid++, v++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);
        
        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }
        
        
        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);

        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);
        return t;
    }

    private void CreateVertices()
    {

        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = 2 * (
                               (xSize - 1) * (ySize - 1)
                               + (xSize - 1) * (zSize - 1)
                               + (ySize - 1) * (zSize - 1)
                           );
        m_vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        m_normals = new Vector3[m_vertices.Length];
        m_cubeUV = new Color32[m_vertices.Length];
        
        int v = 0;
        for (int y = 0; y <= ySize; y++) 
        {
            for (int x = 0; x <= xSize; x++) 
            {
                SetVertices(v++, x, y, 0);
            }
            for (int z = 1; z <= zSize; z++) 
            {
                SetVertices(v++, xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--) 
            {
                SetVertices(v++, x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--) 
            {
                SetVertices(v++, 0, y, z);
            }
        }

        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertices(v++, x, ySize, z);
            }
        }
        
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
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
        Vector3 inner = m_vertices[i] = new Vector3(x, y, z);

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > xSize - roundness)
        {
            inner.x = xSize - roundness;
        }

        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }

        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }
        
        m_normals[i] = (m_vertices[i] - inner).normalized;
        m_vertices[i] = inner + m_normals[i] * roundness;
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
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);
        
        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;
        
        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);
		
        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);
		
        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(x, y, z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(x, y, z);
        c.direction = direction;
        c.radius = roundness;
        c.height = c.center[direction] * 2f;
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
