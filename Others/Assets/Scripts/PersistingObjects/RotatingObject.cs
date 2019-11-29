using System.Collections;
using UnityEngine;

public class RotatingObject : PersistableObject
{
    [SerializeField] private Vector3 angularVelocity;

    private void FixedUpdate()
    {
        transform.Rotate(angularVelocity * Time.deltaTime);
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(transform.localRotation);
    }

    public override void Load(GameDataReader reader)
    {
        transform.localRotation = reader.ReadQuaternion();
    }
}
