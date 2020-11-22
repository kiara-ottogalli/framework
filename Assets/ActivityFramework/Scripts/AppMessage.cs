using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppMessage
{
	public enum MessageType
	{
		INFO,
		WARNING,
		ERROR
	}

	public MessageType type;
	public Dictionary<AppLanguage, string> languages = new Dictionary<AppLanguage, string>();

	public AppMessage(string stringType, string[] langs)
	{
		type = (MessageType) System.Enum.Parse(typeof(MessageType), stringType);
		int i = 0;
		while (i < langs.Length)
		{
			try
			{
				AppLanguage myLang = (AppLanguage) System.Enum.Parse(typeof(AppLanguage), langs[i], true);
				languages.Add(myLang, langs[i + 1]);
			}
			catch (System.ArgumentException)
			{
				Debug.LogError("An element with Key = " + langs[i] + " already exists.");
			}
			i += 2;
		}
	}

	public string GetMessage(AppLanguage language)
	{
		string message = "";
		if (!languages.TryGetValue(language, out message))
		{
			languages.TryGetValue(AppLanguage.EN, out message);
		}
		return message;
	}
}
