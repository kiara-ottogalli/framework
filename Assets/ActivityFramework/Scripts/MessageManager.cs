using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public enum AppLanguage
{
	EN,
	ES
}

[System.Serializable]
class JsonMessage
{
	public string id = "";
	public string type = "";
	public string[] languages = null;
}

[System.Serializable]
class JsonMessages
{
	public JsonMessage[] messages = null;
}

public class MessageManager : MonoBehaviour
{
	public AppLanguage language = AppLanguage.EN;
	public Dictionary<string, AppMessage> messages = new Dictionary<string, AppMessage>();

	JsonMessages jsonMessages;

	public void ReadMessageData()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "AppMessages.json");
		if(File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath); 
			jsonMessages = JsonUtility.FromJson<JsonMessages>(dataAsJson);
		}
		else
		{
			Debug.LogError("Cannot load messsages.");
		}
	}

	public void LoadMessageData()
	{
		if (jsonMessages != null)
		{
			for (int i = 0; i < jsonMessages.messages.Length; i++)
			{
				AppMessage message = new AppMessage(jsonMessages.messages[i].type, jsonMessages.messages[i].languages);
				AddMessage(jsonMessages.messages[i].id, message);
			}
		}
	}

	public void AddMessage(string messageId, AppMessage message)
	{
		try
		{
			messages.Add(messageId, message);
		}
		catch (System.ArgumentException)
		{
			Debug.LogError("An element with Key = " + messageId + " already exists.");
		}
	}

	public AppMessage GetMessage(string messageId)
	{
		AppMessage message = null;
		messages.TryGetValue(messageId, out message);
		return message;
	}

	public string GetMessageString(string messageId)
	{
		string messageString = "";
		AppMessage message = GetMessage(messageId);
		if (message != null)
		{
			messageString = message.GetMessage(language);
		}
		return messageString;
	}
}
