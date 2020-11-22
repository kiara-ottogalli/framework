using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Subtask : MonoBehaviour
{
	public enum SubtaskStatus
	{
		WAITING,      // Waiting for predecessor tasks to finish
		READY,        // Predecessor tasks have finished
		RUNNING,      // Some goals are satisfied
		COMPLETED,    // All goals are satisfied
		CANT_COMPLETE // If perform finished but the goals have not been accomplished. This is due to some error.
		              // In this case, the error has to be solved (possibly in manual form) and then perform again.
	}

	public enum SubtaskMode
	{
		MANUAL,       // To let the worker do the task manually/voice commands
		AUTO,         // To make the tasks perform automatically by the automated devices
		FAST          // To skip the task
	}

	public int id;
	public string description;

	public SubtaskStatus status;
	public SubtaskStatus Status
	{
		get
		{
			return status;
		}
		set
		{
			if (status != value)
			{
				status = value;
				if (status == SubtaskStatus.READY)
				{
					if (OnReady != null)
					{
						OnReady.Invoke(parentTask.id, id);
					}
					if (mode == SubtaskMode.AUTO || mode == SubtaskMode.FAST)
					{
						ExecuteAction();
					}
					else
					{
						PrepareForAction();
					}
				}
				if (status == SubtaskStatus.COMPLETED)
				{
					if (mode == SubtaskMode.MANUAL)
					{
						AdjustAfterAction();
					}
					if (OnCompleted != null)
					{
						OnCompleted.Invoke(parentTask.id, id);
					}
				}
			}
		}
	}

	public Task parentTask;
	public List<Subtask> dependsOn;
	public SubtaskMode mode;
	public List<Condition> goals;
	[SerializeField]
	private SubtaskAction action;
	public SubtaskAction Action
	{
		get
		{
			return action;
		}
		set
		{
			if (action != value)
			{
				action = value;
				if (action != null)
				{
					action.OnPerformed.AddListener(WaitActionToFinish);
				}
			}
		}
	}

	public bool parentTaskReady;
	public bool allPreviousSubtasksCompleted;
	public int goalsSatisfied;

	public UnityIntIntEvent OnReady;
	public UnityIntIntEvent OnCompleted;

	private bool hasCalledExecuteAction = false;

	void Awake()
	{
		parentTask = transform.GetComponentInParent<Task>();
	}

	void OnEnable()
	{
		if (parentTask == null)
		{
			parentTaskReady = true;
		}
		else
		{
			parentTask.OnReady.AddListener(SetParentTaskReadyAndDetermineNewStatus);
		}
	}

	void OnDisable()
	{
		if (parentTask != null)
		{
			parentTask.OnReady.RemoveListener(SetParentTaskReadyAndDetermineNewStatus);
		}
	}

	public Subtask()
	{
		id = 0;
		description = "";
		status = SubtaskStatus.WAITING;
		parentTask = null;
		dependsOn = new List<Subtask>();
		mode = SubtaskMode.MANUAL;
		goals = new List<Condition>();
		action = null;

		allPreviousSubtasksCompleted = false;
		goalsSatisfied = 0;

		OnReady = new UnityIntIntEvent();
		OnCompleted = new UnityIntIntEvent();
	}

	public void StartSubtask()
	{
		CheckPreviousSubtasksCompleted();
		CheckGoalsSatisfied();
		DetermineNewStatus();
	}

	public void ResetSubtask()
	{
		action.Reset();
		StopAllCoroutines();
		status = SubtaskStatus.WAITING;
		allPreviousSubtasksCompleted = false;
		goalsSatisfied = 0;
	}

	public void AddGoal(Condition newGoal)
	{
		goals.Add(newGoal);
		newGoal.OnGlobalConditionChanged.AddListener(CheckGoalsSatisfiedAndDetermineNewStatus);
	}

	public void AddDependency(Subtask subtaskDependency)
	{
		dependsOn.Add(subtaskDependency);
		subtaskDependency.OnCompleted.AddListener(CheckPreviousSubtasksCompletedAndDetermineNewStatus);
	}

	public void SetParentTaskReadyAndDetermineNewStatus(int taskId)
	{
		parentTaskReady = true;
		DetermineNewStatus();
	}

	void CheckPreviousSubtasksCompleted()
	{
		bool allCompleted = true;
		int i = 0;
		while (allCompleted && i < dependsOn.Count)
		{
			allCompleted = allCompleted && dependsOn[i].Status == SubtaskStatus.COMPLETED;
			i++;
		}
		allPreviousSubtasksCompleted = allCompleted;
	}

	void CheckPreviousSubtasksCompletedAndDetermineNewStatus(int parentId, int taskId)
	{
		CheckPreviousSubtasksCompleted();
		DetermineNewStatus();
	}

	public void CheckGoalsSatisfied()
	{
		int numSatisfied = 0;
		for (int i = 0; i < goals.Count; i++)
		{
			if (goals[i].IsSatisfied())
			{
				numSatisfied++;
			}
		}
		goalsSatisfied = numSatisfied;
	}

	public void CheckGoalsSatisfiedAndDetermineNewStatus()
	{
		CheckGoalsSatisfied();
		DetermineNewStatus();
	}

	void DetermineNewStatus()
	{
		if (Status == SubtaskStatus.WAITING && allPreviousSubtasksCompleted && parentTaskReady)
		{
			Status = SubtaskStatus.READY;
		}
		if (Status == SubtaskStatus.READY && (goalsSatisfied > 0 || hasCalledExecuteAction || mode == SubtaskMode.MANUAL))
		{
			Status = SubtaskStatus.RUNNING;
		}
		if (Status == SubtaskStatus.RUNNING || Status == SubtaskStatus.CANT_COMPLETE)
		{
			if (mode == SubtaskMode.MANUAL)
			{
				if (goalsSatisfied == goals.Count)
				{
					Status = SubtaskStatus.COMPLETED;
				}
				else
				{
					if (AppController.instance.errorManager.Error)
					{
						Status = SubtaskStatus.CANT_COMPLETE;
					}
				}
			}
			else
			{
				if (action != null && action.performed)
				{
					if (goalsSatisfied == goals.Count)
					{
						Status = SubtaskStatus.COMPLETED;
					}
					else
					{
						Status = SubtaskStatus.CANT_COMPLETE;
					}
				}
			}
		}
	}

	void WaitActionToFinish()
	{
		StartCoroutine(WaitActionToFinishCoroutine());
	}

	IEnumerator WaitActionToFinishCoroutine()
	{
		while (!action.performed)
		{
			yield return null;
		}
		DetermineNewStatus();
	}

	public void ExecuteAction()
	{
		if (action != null)
		{
			action.Perform();
			hasCalledExecuteAction = true;
		}
	}

	public void PrepareForAction()
	{
		if (action != null)
		{
			action.PreAction();
		}
	}

	public void AdjustAfterAction()
	{
		if (action != null)
		{
			action.PostAction();
		}
	}
}
