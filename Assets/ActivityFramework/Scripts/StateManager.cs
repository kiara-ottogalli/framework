using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
	[Tooltip("Name of the new Global Condition")]
	public string globalConditionName;
	[Tooltip("Is the global condition initially satisfied?")]
	public bool isSatisfied;
	[TextArea(2, 3)]
	[Tooltip("Description of the condition. What is it used for?")]
	public string globalConditionDescription;
	[TextArea(2, 3)]
	[Tooltip("Message if true")]
	public string globalConditionMessageSatisfied;
	[TextArea(2, 3)]
	[Tooltip("Message if false")]
	public string globalConditionMessageNotSatisfied;
	[Tooltip("In which category should this global condition be?")]
	public string categoryName;

	void Start ()
	{
		globalConditionName = "";
		globalConditionDescription = "";
	}

	public void AddNewGlobalCondition(string name, bool satisfied, string description, string messageSatisfied, string messageNotSatisfied)
	{
		Transform categoryObjectTransform = transform.Find(categoryName);
		if (categoryObjectTransform == null)
		{
			GameObject categoryObject = new GameObject(categoryName);
			categoryObject.transform.parent = transform;
			categoryObjectTransform = categoryObject.transform;
		}
		GameObject globalConditionObject = new GameObject(name);
		globalConditionObject.transform.parent = categoryObjectTransform;
		GlobalCondition globalCondition = globalConditionObject.AddComponent<GlobalCondition>();
		globalCondition.description = description;
		globalCondition.messageSatisfied = messageSatisfied;
		globalCondition.messageNotSatisfied = messageNotSatisfied;
		globalCondition.Satisfied = satisfied;
	}

	public GlobalCondition FindGlobalConditionByName(string name)
	{
		GlobalCondition globalCondition = null;
		int i = 0;
		Transform grandchild = null;
		while (grandchild == null && i < transform.childCount)
		{
			Transform child = transform.GetChild(i);
			grandchild = child.Find(name);
			i++;
		}
		if (grandchild != null)
		{
			globalCondition = grandchild.GetComponent<GlobalCondition>();
		}
		return globalCondition;
	}

	public bool GlobalConditionExists(string name)
	{
		int i = 0;
		Transform grandchild = null;
		while (grandchild == null && i < transform.childCount)
		{
			Transform child = transform.GetChild(i);
			grandchild = child.Find(name);
			i++;
		}
		return grandchild != null;
	}
}
