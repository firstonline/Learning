using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField] private Shape[] prefabs;
    [SerializeField] private Material[] materials;
	[SerializeField] private bool m_recycle;
	[SerializeField] private List<Shape>[] pools;

	private Scene poolScene;

    public Shape Get(int shapeId = 0, int materialId = 0)
	{
		Shape instance = null;
		if (m_recycle)
		{
			if (pools == null)
			{
				CreatePools();
			}

			List<Shape> pool = pools[shapeId];
			int lastIndex = pool.Count - 1;
			if (lastIndex >= 0)
			{
				instance = pool[lastIndex];
				pool.RemoveAt(lastIndex);
			}
			else
			{
				instance = Instantiate(prefabs[shapeId]);
				instance.MShapeId = shapeId;
				SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
			}
			instance.gameObject.SetActive(true);
		}
		else
		{
			instance = Instantiate(prefabs[shapeId]);
			instance.MShapeId = shapeId;
		}
		
		instance.SetMaterial(materials[materialId], materialId);
		return instance;
	}

	public void Reclaim(Shape shapeToRecycle)
	{
		if (m_recycle)
		{
			if (pools == null)
			{
				CreatePools();
			}

			pools[shapeToRecycle.MShapeId].Add(shapeToRecycle);
			shapeToRecycle.gameObject.SetActive(false);
		}
		else
		{
			Destroy(shapeToRecycle.gameObject);
		}
	}

    public Shape GetRandom()
	{
		return Get(
			Random.Range(0, prefabs.Length), 
			Random.Range(0, materials.Length)
			);
	}

	private void CreatePools()
	{
		pools = new List<Shape>[prefabs.Length];
		for (int i = 0; i < pools.Length; i++)
		{
			pools[i] = new List<Shape>();
		}

		if (Application.isEditor)
		{
			poolScene = SceneManager.GetSceneByName(name);
			if (poolScene.isLoaded)
			{
				GameObject[] rootObjects = poolScene.GetRootGameObjects();
				for (int i = 0; i < rootObjects.Length; i++)
				{
					Shape pooledShape = rootObjects[i].GetComponent<Shape>();
					if (!pooledShape.gameObject.activeSelf)
					{
						pools[pooledShape.MShapeId].Add(pooledShape);
					}
				}
				
				
				return;
			}
		}

		poolScene = SceneManager.CreateScene(name);
	}
}
