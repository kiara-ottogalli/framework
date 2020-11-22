using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Task : MonoBehaviour
{
	public enum TaskStatus
	{
		WAITING,      // Waiting for predecessor tasks to finish
		READY,        // Predecessor tasks have finished
		RUNNING,      // Some postconditions are satisfied
		COMPLETED,    // All postconditions are satisfied
		CANT_COMPLETE // Not defined yet. Some kind of error maybe.
	}

	public int id;
	public string description;

	public TaskStatus status;
	public TaskStatus Status
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
				if (status == TaskStatus.READY)
				{
					if (OnReady != null)
					{
						OnReady.Invoke(id);
					}
				}
				if (status == TaskStatus.COMPLETED)
				{
					if (OnCompleted != null)
					{
						OnCompleted.Invoke(id);
					}
				}
			}
		}
	}

	public List<Task> dependsOn;
	public List<Subtask> subtasks;

	public List<Subtask> readySubtasks;

	[SerializeField]
	private Subtask currentSubtask;
	public Subtask CurrentSubtask
	{
		get
		{
			return currentSubtask;
		}
		set
		{
			if (currentSubtask != value)
			{
				currentSubtask = value;
				if (OnCurrentSubtaskChange != null)
				{
					OnCurrentSubtaskChange.Invoke();
				}
			}
		}
	}

	public int subtasksCompleted;
	public bool allPreviousTasksCompleted;

	public UnityIntEvent OnReady;
	public UnityIntEvent OnCompleted;

	public UnityEvent OnPreConditionChange;
	public UnityEvent OnCurrentSubtaskChange;

    public Task()
	{
		id = 0;
		description = "";
		dependsOn = new List<Task>();
        OnCurrentSubtaskChange = new UnityEvent();
        OnPreConditionChange = new UnityEvent();
		subtasks = new List<Subtask>();
		status = TaskStatus.WAITING;
		subtasksCompleted = 0;
		allPreviousTasksCompleted = false;
		readySubtasks = new List<Subtask>();
		OnReady = new UnityIntEvent();
		OnCompleted = new UnityIntEvent();
	}

	public void StartTask()
	{
		CheckPreviousTasksCompleted();
		subtasksCompleted = 0;
		DetermineNewStatus();
		for (int i = 0; i < subtasks.Count; i++)
		{
			subtasks[i].StartSubtask();
		}
	}

	public void ResetTask()
	{
		for (int i = 0; i < subtasks.Count; i++)
		{
			subtasks[i].ResetSubtask();
		}
		status = TaskStatus.WAITING;
		subtasksCompleted = 0;
		allPreviousTasksCompleted = false;
	}

	public void AddSubtask(Subtask newSubtask)
	{
		subtasks.Add(newSubtask);
		newSubtask.OnReady.AddListener(AddReadySubtask);
		newSubtask.OnCompleted.AddListener(CheckSubtasksCompletedAndDetermineNewStatus);
	}

	public void AddDependency(Task taskDependency)
	{
		dependsOn.Add(taskDependency);
		taskDependency.OnCompleted.AddListener(CheckPreviousTasksCompletedAndDetermineNewStatus);
	}

	public Subtask FindSubtaskById(int subtaskId)
	{
		Subtask subtask = null;
		int i = 0;
		while (subtask == null && i < subtasks.Count)
		{
			if (subtasks[i].id == subtaskId)
			{
				subtask = subtasks[i];
			}
			i++;
		}
		return subtask;
	}

	private void CheckPreviousTasksCompleted()
	{
		bool allCompleted = true;
		int i = 0;
		while (allCompleted && i < dependsOn.Count)
		{
			allCompleted = allCompleted && dependsOn[i].Status == TaskStatus.COMPLETED;
			i++;
		}
		allPreviousTasksCompleted = allCompleted;
	}

	private void CheckPreviousTasksCompletedAndDetermineNewStatus(int taskId)
	{
		CheckPreviousTasksCompleted();
		DetermineNewStatus();
	}

	private void AddReadySubtask(int parentId, int taskId)
	{
		Subtask readySubtask = FindSubtaskById(taskId);
		if (readySubtask != null)
		{
			readySubtasks.Add(readySubtask);
			if (readySubtasks.Count == 1)
			{
				CurrentSubtask = readySubtask;
			}
		}
	}

	private void CheckSubtasksCompletedAndDetermineNewStatus(int parentId, int subtaskId)
	{
		CheckSubtasksCompleted(subtaskId);
		DetermineNewStatus();
	}

	private void CheckSubtasksCompleted(int subtaskId)
	{
		Subtask subtask = null;
		int i = 0;
		while (subtask == null && i < readySubtasks.Count)
		{
			if (readySubtasks[i].id == subtaskId)
			{
				subtask = readySubtasks[i];
			}
			else
			{
				i++;
			}
		}
		if (subtask != null)
		{
			readySubtasks.RemoveAt(i);
			if (i == 0)
			{
				if (readySubtasks.Count > 0)
				{
					CurrentSubtask = readySubtasks[0];
				}
			}
		}
		subtasksCompleted++;
	}

	private void DetermineNewStatus()
	{
		if (Status == TaskStatus.WAITING && allPreviousTasksCompleted)
		{
			Status = TaskStatus.READY;
		}
		if (Status == TaskStatus.READY && subtasksCompleted > 0)
		{
			Status = TaskStatus.RUNNING;
		}
		if (Status == TaskStatus.RUNNING && subtasksCompleted == subtasks.Count)
		{
			Status = TaskStatus.COMPLETED;
		}
	}

}
