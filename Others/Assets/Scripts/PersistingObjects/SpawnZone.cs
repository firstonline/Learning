using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class SpawnZone : PersistableObject
{
  
    public abstract Vector3 SpawnPoint { get; }
    [SerializeField] private SpawnConfiguration spawnConfig;
    
    public virtual void ConfigureSpawn(Shape shape)
    {
        var t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        if (spawnConfig.UniformColor)
        {
            shape.SetColour(spawnConfig.color.RandomInRange);
        }
        else
        {
            for (int i = 0; i < shape.ColorCount; i++)
            {
                shape.SetColour(spawnConfig.color.RandomInRange, i);
            }
        }
        shape.AngularVelocity = Random.onUnitSphere * spawnConfig.angularSpeed.RandomValueInRange;
        Vector3 direction;

        switch (spawnConfig.spawnMovementDirection)
        {
            case SpawnConfiguration.SpawnMovementDirection.Forward:
                direction = transform.forward;
                break;
            case SpawnConfiguration.SpawnMovementDirection.Upward:
                direction = transform.up;
                break;
            case SpawnConfiguration.SpawnMovementDirection.OutWard:
                direction = (t.localPosition - transform.position).normalized;
                break;
      
            default:
                direction = Random.onUnitSphere;
                break;
        }
        shape.Velocity = direction * spawnConfig.spawnSpeed.RandomValueInRange;
    }
}
