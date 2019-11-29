using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLevel : PersistableObject
{
	public static GameLevel Current { get; private set; }
	[SerializeField] private PersistableObject[] persistableObjects;
	[SerializeField] private SpawnZone spawnZone;

	
	private void OnEnable()
	{
		Current = this;
		if (persistableObjects == null) 
		{
			persistableObjects = new PersistableObject[0];
		}
	}

	public void ConfigureSpawn(Shape shape)
	{
		spawnZone.ConfigureSpawn(shape);
	}

	public override void Save(GameDataWriter writer)
	{
		writer.Write(persistableObjects.Length);
		for (int i = 0; i < persistableObjects.Length; i++)
		{
			persistableObjects[i].Save(writer);
		}
	}

	public override void Load(GameDataReader reader)
	{
		int count = reader.ReadInt();
		for (int i = 0; i < count; i++)
		{
			persistableObjects[i].Load(reader);
		}
	}
}
