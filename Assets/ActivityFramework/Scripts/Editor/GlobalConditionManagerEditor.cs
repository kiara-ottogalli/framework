using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StateManager))]
public class GlobalConditionManagerEditor : Editor
{

	StateManager instance;
	public bool showGlobalConditions;
	public bool[] showGlobalConditionEditor;
	private bool showMessage;
	public string message;
	private MessageType messageType;

	void Awake()
	{
		showGlobalConditions = true;
		showMessage = false;
		message = "";
	}

	void OnEnable()
	{
		instance = target as StateManager;
		showMessage = false;
		message = "";
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.BeginVertical();

		base.OnInspectorGUI();

		if (instance != null)
		{
			if (instance.globalConditionName == "" || instance.categoryName == "")
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button("Add"))
			{
				if (!instance.GlobalConditionExists(instance.globalConditionName))
				{
					instance.AddNewGlobalCondition(instance.globalConditionName, instance.isSatisfied, instance.globalConditionDescription, instance.globalConditionMessageSatisfied, instance.globalConditionMessageNotSatisfied);
					message = "Global condition added.";
					messageType = MessageType.Info;
				}
				else
				{
					message = "The global condition already exists.";
					messageType = MessageType.Error;
				}
				showMessage = true;
			}
			if (instance.globalConditionName == "" || instance.categoryName == "")
			{
				GUI.enabled = true;
			}
			if (showMessage)
			{
				EditorGUILayout.HelpBox(message, messageType);
			}
		}

		EditorGUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}

}
