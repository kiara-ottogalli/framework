using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GlobalCondition : MonoBehaviour
{
	[TextArea(2, 3)]
	[Tooltip("Description of the condition. What is it used for?")]
	public string description;
	[TextArea(2, 3)]
	[Tooltip("Message to show if the condition is satisfied")]
	public string messageSatisfied;
	[TextArea(2, 3)]
	[Tooltip("Message to show if the condition is not satisfied")]
	public string messageNotSatisfied;
	[Tooltip("When checked, the condition has been satisfied.")]
	[SerializeField]
	private bool satisfied;

	public UnityEvent OnConditionChanged;
	public UnityEvent OnConditionSatisfied;
	public UnityEvent OnConditionNotSatisfied;

	public bool Satisfied
	{
		get
		{
			return satisfied;
		}
		set
		{
			if (satisfied != value)
			{
				satisfied = value;
				if (satisfied)
				{
					if (OnConditionSatisfied != null)
					{
						OnConditionSatisfied.Invoke();
					}
				}
				else
				{
					if (OnConditionNotSatisfied != null)
					{
						OnConditionNotSatisfied.Invoke();
					}
				}
				if (OnConditionChanged != null)
				{
					OnConditionChanged.Invoke();
				}
			}
		}
	}

	public void Toggle()
	{
		Satisfied = !Satisfied;
	}
}
