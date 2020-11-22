using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

[RequireComponent(typeof(Subtask))]
public abstract class SubtaskAction : MonoBehaviour
{
	public GameObject actor;
	public string actionName;
	public List<string> parameters;
	[FormerlySerializedAs("done")]
	public bool performed = false;

	public UnityEvent OnPerformed = new UnityEvent();

	public virtual void Perform()
	{
		performed = true;
		if (OnPerformed != null)
		{
			OnPerformed.Invoke();
		}
	}

	public virtual void PreAction()
	{

	}

	public virtual void PostAction()
	{

	}

	public virtual void Reset()
	{
		performed = false;
	}
}
