using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : PersistableObject
{
	public float CreationSpeed { get; set; }
	public float DestructionSpeed { get; set; }

	[SerializeField] private ShapeFactory shapeFactory;
	[SerializeField] private KeyCode createKey = KeyCode.C;
	[SerializeField] private KeyCode newGameKey = KeyCode.N;
	[SerializeField] private KeyCode saveKey = KeyCode.S;
	[SerializeField] private KeyCode loadKey = KeyCode.L;
	[SerializeField] private KeyCode destroyKey = KeyCode.X;
	[SerializeField] private PersistentStorage m_storage;
	[SerializeField] private int levelCount;
	[SerializeField] private bool reseedOnLoad;
	[SerializeField] private Slider creationSpeedSlider;
	[SerializeField] private Slider destructionSpeed;
	
	
	private List<Shape> shapes;
	private string savePath;
	private const int saveVersion = 6;
	private float creationProgress;
	private float destructionProgress;
	private int loadedLevelBuildIndex;
	private Random.State mainRandomState;

	void Start()
	{
		shapes = new List<Shape>();

		if (Application.isEditor)
		{
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				Scene loadedLevel = SceneManager.GetSceneAt(i);
				if (loadedLevel.name.Contains("Level"))
				{
					SceneManager.SetActiveScene(loadedLevel);
					loadedLevelBuildIndex = loadedLevel.buildIndex;
					return;
				}
			}
		}
		mainRandomState = Random.state;
		BeginNewGame();
		StartCoroutine(LoadLevel(1));
	}
	
	void Update()
	{
		if (Input.GetKeyDown(createKey))
		{
			CreateShape();
		}
		else if (Input.GetKeyDown(newGameKey))
		{
			BeginNewGame();
			StartCoroutine(LoadLevel(loadedLevelBuildIndex));
		}
		else if (Input.GetKeyDown(saveKey))
		{
			m_storage.Save(this, saveVersion);
		}
		else if (Input.GetKeyDown(loadKey))
		{
			m_storage.Load(this);
		}
		else if (Input.GetKeyDown(destroyKey))
		{
			DestroyShape();
		}
		else
		{
			for (int i = 1; i <= levelCount; i++)
			{
				if (Input.GetKeyDown(KeyCode.Alpha0 + i))
				{
					BeginNewGame();
					StartCoroutine(LoadLevel(i));
					return;
				}
				
			}
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < shapes.Count; i++)
		{
			shapes[i].GameUpdate();
		}
			
		creationProgress += Time.deltaTime * CreationSpeed;
		while (creationProgress >= 1f)
		{
			creationProgress -= 1f;
			CreateShape();
		}
		
		destructionProgress += Time.deltaTime * DestructionSpeed;
		while (destructionProgress >= 1f)
		{
			destructionProgress -= 1f;
			DestroyShape();
		}
		
	}

	public override void Save(GameDataWriter writer)
	{
		writer.Write(shapes.Count);
		writer.Write(Random.state);
		writer.Write(CreationSpeed);
		writer.Write(creationProgress);
		writer.Write(DestructionSpeed);
		writer.Write(destructionProgress);
		writer.Write(loadedLevelBuildIndex);
		GameLevel.Current.Save(writer);
		for (int i = 0; i < shapes.Count; i++)
		{
			writer.Write(shapes[i].MShapeId);
			writer.Write(shapes[i].MaterialId);
			shapes[i].Save(writer);
		}
	}

	public override void Load(GameDataReader reader)
	{
		int version = reader.Version;
	
		if (version > saveVersion)
		{
			Debug.LogError("Unsupported future save version " + version);
			return;
		}

		StartCoroutine(LoadGame(reader));
	}

	private IEnumerator LoadGame(GameDataReader reader)
	{
		int version = reader.Version;
		int count = version <= 0 ? -version : reader.ReadInt();
		
		if (version >= 3)
		{
			Random.State state = reader.ReadRandomState();
			if (!reseedOnLoad)
			{
				Random.state = state;
			}
			CreationSpeed = reader.ReadFloat();
			creationProgress = reader.ReadFloat();
			DestructionSpeed = reader.ReadFloat();
			destructionProgress = reader.ReadFloat();
			destructionSpeed.value = DestructionSpeed;
			creationSpeedSlider.value = CreationSpeed;
		}
		
		yield return LoadLevel(version < 2 ? 1 : reader.ReadInt());

		if (version >= 3)
		{
			GameLevel.Current.Load(reader);
		}
		
		for (int i = 0; i < count; i++)
		{
			int shapeId = version > 0 ? reader.ReadInt() : 0;
			int materialId = version > 0 ? reader.ReadInt() : 0;
			Shape instance = shapeFactory.Get(shapeId, materialId);
			instance.Load(reader);
			shapes.Add(instance);
		}
	}
	
	
	private void DestroyShape()
	{
		if (shapes.Count > 0)
		{
			int index = Random.Range(0, shapes.Count);
			shapeFactory.Reclaim(shapes[index]);
			int lastIndex = shapes.Count - 1;
			shapes[index] = shapes[lastIndex];
			shapes.RemoveAt(lastIndex);
		}
	}

	private void CreateShape()
	{
		var instance = shapeFactory.GetRandom();
		GameLevel.Current.ConfigureSpawn(instance);
		shapes.Add(instance);
	}

	private void BeginNewGame()
	{
		Random.state = mainRandomState;
		int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
		mainRandomState = Random.state;
		Random.InitState(seed);

		CreationSpeed = 0;
		DestructionSpeed = 0;
		creationSpeedSlider.value = 0;
		destructionSpeed.value = 0;
		
		for (int i = 0; i < shapes.Count; i++)
		{
			shapeFactory.Reclaim(shapes[i]);
		}
		shapes.Clear();
	}

	private IEnumerator LoadLevel(int levelBuildIndex)
	{
		enabled = false;
		if (loadedLevelBuildIndex > 0)
		{
			yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
		}
		yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
		SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
		loadedLevelBuildIndex = levelBuildIndex;
		enabled = true;
	}
	
}
