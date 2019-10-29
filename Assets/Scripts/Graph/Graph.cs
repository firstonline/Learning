using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField] private GameObject m_point;

    [Range(1,  1000)]
    [SerializeField] private int m_resolution = 1;

    [SerializeField] private GraphFunctionName m_function;

    private GraphFunction[] m_functions =
    {
        SineFunction, 
        MultiSineFunction, 
        Sine2DFunction, 
        MultiSine2DFunction, 
        Ripple,
        Cylinder,
        Sphere,
        Torus, 
    };
    private Transform[] m_points;
    
    const float PI = Mathf.PI;
    
    private void Awake()
    {
        float step = 2f / m_resolution;
        m_points = new Transform[m_resolution * m_resolution];
        Vector3 scale = Vector3.one * step;
        Vector3 position;
        position.y = 0f;
        position.z = 0f;
        for (int i = 0, z = 0; z < m_resolution; z++)
        {
            position.z = (z + 0.5f) * step - 1f;
            for (int x = 0; x < m_resolution; x++, i++)
            {
                Transform point = Instantiate(m_point).transform;
                position.x = (x + 0.5f) * step - 1f;
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform, false);
                m_points[i] = point;
            }
        }
    }

    private void Update()
    {
        float t = Time.time;
        GraphFunction f = m_functions[(int) m_function];
        float step = 2f / m_resolution;
        for (int i = 0, z = 0; z < m_resolution; z++) 
        {
            float v = (z + 0.5f) * step - 1f;
            for (int x = 0; x < m_resolution; x++, i++) 
            {
                float u = (x + 0.5f) * step - 1f;
                m_points[i].localPosition = f(u, v, t);
            }
        }
    }

    private static Vector3 SineFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(PI * (x + t));
        return new Vector3(x, y, z);
    }

    private static Vector3 MultiSineFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(PI * (x + t));
        y += Mathf.Sin(2f * PI * (x + 3 * t)) / 2f;
        y *= 2f / 3f;
        return new Vector3(x, y, z);
    }
    
    static Vector3 Sine2DFunction(float x, float z, float t) 
    {
        float y = Mathf.Sin(PI * (x + t));
        y += Mathf.Sin(PI * (z + t));
        y *= 0.5f;
        return new Vector3(x, y, z);
    }
    
    static Vector3 MultiSine2DFunction(float x, float z, float t) 
    {
        float y = 4 * Mathf.Sin(PI * (x + z + t * 0.5f));
        y += Mathf.Sin(PI * (x + t));
        y += Mathf.Sin(2f * PI * (z + 2f * t)) * 0.5f;
        y *= 1f / 5.5f;
        return new Vector3(x, y, z);
    }

    static Vector3 Ripple(float x, float z, float t)
    {
        float d = Mathf.Sqrt(x * x + z * z);
        float y =  Mathf.Sin(4 * (PI * d - t));
        // reduce the sound from outer edges
        y /= 1f + 10f * d;
        return new Vector3(x, y, z);
    }
    
    static Vector3 Cylinder(float u, float v, float t)
    {
        Vector3 p;
        float r = 0.8f + Mathf.Sin(PI * (6f * u + 2f * v + t)) * 0.2f;
        p.x = r * Mathf.Sin(PI * u);
        p.y = v;
        p.z = r * Mathf.Cos(PI * u);
        return p;
    }
    
    static Vector3 Sphere(float u, float v, float t) {
        Vector3 p;
        // normal sphere r = Mathf.Cos(pi * 0.5f * v)
        float r = 0.8f + Mathf.Sin(PI * (6f * u + t)) * 0.1f;
        r += Mathf.Sin(PI * (4f * v + t)) * 0.1f;
        float s = r * Mathf.Cos(PI * 0.5f * v);
        p.x = s * Mathf.Sin(PI * u);
        p.y = r * Mathf.Sin(PI * 0.5f * v);
        p.z = s * Mathf.Cos(PI * u);
        return p;
    }
    
    static Vector3 Torus(float u, float v, float t) {
        Vector3 p;
        
        // normal torus r1 = 1f, r2 = 0.5f
        float r1 = 0.65f + Mathf.Sin(PI * (6f * u + t)) * 0.1f;
        float r2 = 0.2f + Mathf.Sin(PI * (4f * v + t)) * 0.05f;
        float s = r2 * Mathf.Cos(PI * v) + r1;
        p.x = s * Mathf.Sin(PI * u);
        p.y = r2 * Mathf.Sin(PI * v);
        p.z = s * Mathf.Cos(PI * u);
        return p;
    }
}
