using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;
using System;

[System.Serializable]
class JsonObject
{
	public string name = "";
}

[System.Serializable]
class JsonObjects
{
	public JsonObject[] objects = null;
}

public class AppController : MonoBehaviour
{
	public static AppController instance;
	public string file = "AppObjects.json";
	public StateManager stateManager;
	public TaskManager taskManager;
	public ErrorManager errorManager;
	public MessageManager messageManager;
	public List<GameObject> appObjects;
	
	private JsonObjects data;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			if (instance != this)
			{
				Destroy(gameObject);
			}
		}
	}

	void Start ()
	{
		appObjects = null;

		if (stateManager != null && taskManager != null && messageManager != null && errorManager != null)
		{
			StartCoroutine("LoadingCoroutine");
		}
		else
		{
			Debug.LogError("App Manager references are not set.");
		}
	}

	IEnumerator LoadingCoroutine()
	{
		messageManager.ReadMessageData();
		messageManager.LoadMessageData();
		ReadObjects();
		LoadObjects();
		taskManager.ReadTasks();
		taskManager.LoadTasks();
		yield return new WaitForSeconds(2.0f);
		taskManager.StartTasks();
	}

	public void ReadObjects()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, file);
		if(File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath);
			data = JsonUtility.FromJson<JsonObjects>(dataAsJson);
		}
		else
		{
			Debug.LogError("Cannot load " + file + " data.");
		}
	}

	void LoadObjects()
	{
		if (data != null)
		{
			AppController.instance.appObjects = new List<GameObject>();
			for (int i = 0; i < data.objects.Length; i++)
			{
				GameObject currentGameObject = GameObject.Find(data.objects[i].name);
				if (currentGameObject != null)
				{
					AppController.instance.appObjects.Add(currentGameObject);
				}
				else
				{
					Debug.LogError(data.objects[i].name + " couldn't be found.");
				}
			}
			if (AppController.instance.appObjects.Count != data.objects.Length)
			{
				Debug.LogError("Some objects couldn't be found. Check the spelling and try again.");
			}
		}
	}

	public GameObject FindObjectByName(string name)
	{
		GameObject gameObject = null;
		int i = 0;
		while (gameObject == null && i < appObjects.Count)
		{
			if (appObjects[i].name == name)
			{
				gameObject = appObjects[i];
			}
			i++;
		}
		return gameObject;
	}

}
