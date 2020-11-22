using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Condition
{
	public GlobalCondition globalCondition;
	public bool satisfied;

	[HideInInspector]
	public UnityEvent OnGlobalConditionChanged;
	[HideInInspector]
	public UnityEvent OnEqualToGlobalCondition;
	[HideInInspector]
	public UnityEvent OnNotEqualToGlobalCondition;

	public Condition(GlobalCondition relatedGlobalCondition, bool satisfiedState)
	{
		globalCondition = relatedGlobalCondition;
		satisfied = satisfiedState;
		OnGlobalConditionChanged = new UnityEvent();
		OnEqualToGlobalCondition = new UnityEvent();
		OnNotEqualToGlobalCondition = new UnityEvent();
		globalCondition.OnConditionChanged.AddListener(CheckChange);
	}

	void CheckChange()
	{
		if (OnGlobalConditionChanged != null)
		{
			OnGlobalConditionChanged.Invoke();
		}
		if (IsSatisfied())
		{
			if (OnEqualToGlobalCondition != null)
			{
				OnEqualToGlobalCondition.Invoke();
			}
		}
		else
		{
			if (OnNotEqualToGlobalCondition != null)
			{
				OnNotEqualToGlobalCondition.Invoke();
			}
		}
	}

	public bool IsSatisfied()
	{
		return globalCondition != null && globalCondition.Satisfied == satisfied;
	}
}
