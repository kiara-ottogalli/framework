using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UnityEngine.Serialization;

[System.Serializable]
class JsonAction
{
	public string actor = "";
	public string name = "";
	public string[] parameters = null;
}

[System.Serializable]
class JsonGoal
{
	public string name = "";
	public bool satisfied = false;
}

[System.Serializable]
class JsonSubtask
{
	public int id = 0;
	public string description = "";
	public int[] dependsOn = null;
	public string mode = "auto";
	public JsonAction action = null;
	public JsonGoal[] goals = null;
}

[System.Serializable]
class JsonTask
{
	public int id = 0;
	public string description = "";
	public int[] dependsOn = null;
	public JsonSubtask[] subtasks = null;
}

[System.Serializable]
class JsonTasks
{
	public JsonTask[] tasks = null;
}

public class TaskManager : MonoBehaviour
{
	[Tooltip("JSON file containing the tasks")]
	[FormerlySerializedAs("file")]
	public string tasksFile;
	[Tooltip("JSON file containing the tasks to resolve unpredicted situations")]
	public string correctiveTasksFile = "AppPriorityTasks.json";
	public List<Task> tasks;
	public List<Task> readyTasks;
	public int numberOfCompletedTasks;

	[SerializeField]
	private Task currentTask;
	public Task CurrentTask
	{
		get
		{
			return currentTask;
		}
		set
		{
			if (currentTask != value)
			{
				currentTask = value;
				if (OnCurrentTaskChange != null)
				{
					OnCurrentTaskChange.Invoke();
				}
			}
		}
	}

	[SerializeField]
	private bool fastForwardModeOn;
	public bool FastForwardModeOn
	{
		get
		{
			return fastForwardModeOn;
		}
		set
		{
			if (value != fastForwardModeOn)
			{
				fastForwardModeOn = value;
			}
		}
	}

	public UnityEvent OnCurrentTaskChange;
	public UnityEvent OnAllTasksCompleted;
	public UnityIntEvent OnTaskReady;
	public UnityEvent OnEnterCorrectiveTaskMode;
	public UnityEvent OnExitCorrectiveTaskMode;


	private int firstPriorityTaskIndex = 0;
	private JsonTasks tasksData;
	private JsonTasks correctiveTasksData;

	void Start ()
	{
		tasks = new List<Task>();
		readyTasks = new List<Task>();
		numberOfCompletedTasks = 0;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			FastForwardModeOn = !FastForwardModeOn;
		}
	}

	public void ReadTasks()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, tasksFile);
		tasksData = ReadTasksFile(filePath);

		if (correctiveTasksFile != "")
		{
			filePath = Path.Combine(Application.streamingAssetsPath, correctiveTasksFile);
			correctiveTasksData = ReadTasksFile(filePath);
		}
	}

	private JsonTasks ReadTasksFile(string file)
	{
		JsonTasks data = null;
		string filePath = Path.Combine(Application.streamingAssetsPath, file);
		if(File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath); 
			data = JsonUtility.FromJson<JsonTasks>(dataAsJson);
		}
		else
		{
			Debug.LogError("Cannot load " + file + " data.");
		}
		return data;
	}

	public void LoadTasks()
	{
		LoadTasksData(tasks, tasksData);
		firstPriorityTaskIndex = tasks.Count;
		LoadTasksData(tasks, correctiveTasksData);
	}

	private void LoadTasksData(List<Task> taskList, JsonTasks data)
	{
		if (data != null)
		{
			for (int i = 0; i < data.tasks.Length; i++)
			{
				GameObject newTask = new GameObject(data.tasks[i].description);
				newTask.transform.parent = transform;

				Task taskComponent = newTask.AddComponent<Task>();
				taskComponent.id = data.tasks[i].id + firstPriorityTaskIndex;
				taskComponent.description = data.tasks[i].description;
				LoadSubtasks(taskComponent, data.tasks[i].subtasks);
				LoadTaskDependencies(taskComponent, data.tasks[i]);
				AddTask(taskList, taskComponent);
			}
		}
	}

	private void LoadSubtasks(Task parentTask, JsonSubtask[] jsonSubtasks)
	{
		for (int i = 0; i < jsonSubtasks.Length; i++)
		{
			GameObject newSubtask = new GameObject(jsonSubtasks[i].description);
			newSubtask.transform.parent = parentTask.transform;

			Subtask subtaskComponent = newSubtask.AddComponent<Subtask>();
			subtaskComponent.id = jsonSubtasks[i].id;
			subtaskComponent.description = jsonSubtasks[i].description;
			LoadSubtaskMode(subtaskComponent, jsonSubtasks[i].mode);
			LoadAction(subtaskComponent, jsonSubtasks[i].action, jsonSubtasks[i].mode);
			LoadGoals(subtaskComponent, jsonSubtasks[i].goals);
			LoadSubtaskDependencies(parentTask, subtaskComponent, jsonSubtasks[i]);
			parentTask.AddSubtask(subtaskComponent);
		}
	}

	private void LoadTaskDependencies(Task task, JsonTask jsonTask)
	{
		for (int i = 0; i < jsonTask.dependsOn.Length; i++)
		{
			Task dependencyTask = FindTaskById(jsonTask.dependsOn[i] + firstPriorityTaskIndex);
			if (dependencyTask != null)
			{
				task.AddDependency(dependencyTask);
			}
		}
	}

	private void LoadSubtaskMode(Subtask subtask, string mode)
	{
		string modeLowercase = mode.ToLower();
		switch (modeLowercase)
		{
			case "manual":
				subtask.mode = Subtask.SubtaskMode.MANUAL;
				break;
			case "auto":
				subtask.mode = Subtask.SubtaskMode.AUTO;
				break;
			case "fast":
				subtask.mode = Subtask.SubtaskMode.FAST;
				break;
			default:
				subtask.mode = Subtask.SubtaskMode.MANUAL;
				break;
		}
	}

	private void LoadAction(Subtask subtask, JsonAction jsonAction, string mode)
	{
		AppAction action = subtask.gameObject.AddComponent<AppAction>();
		action.actor = AppController.instance.FindObjectByName(jsonAction.actor);
		action.actionName = jsonAction.name;
		action.parameters = new List<string>(jsonAction.parameters);
		action.parameters.Insert(0, mode.ToLower());
		subtask.Action = action;
	}

	private void LoadGoals(Subtask parentSubtask, JsonGoal[] jsonGoals)
	{
		for (int i = 0; i < jsonGoals.Length; i++)
		{
			GlobalCondition currentGlobalCondition = AppController.instance.stateManager.FindGlobalConditionByName(jsonGoals[i].name);
			if (currentGlobalCondition != null)
			{
				Condition newGoal = new Condition(currentGlobalCondition, jsonGoals[i].satisfied);
				parentSubtask.AddGoal(newGoal);
			}
		}
	}

	private void LoadSubtaskDependencies(Task parentTask, Subtask subtask, JsonSubtask jsonSubtask)
	{
		for (int i = 0; i < jsonSubtask.dependsOn.Length; i++)
		{
			Subtask dependencySubtask = parentTask.FindSubtaskById(jsonSubtask.dependsOn[i]);
			if (dependencySubtask != null)
			{
				subtask.AddDependency(dependencySubtask);
			}
		}
	}

	private void AddTask(List<Task> taskList, Task newTask)
	{
		taskList.Add(newTask);
		newTask.OnReady.AddListener(AddReadyTask);
		newTask.OnReady.AddListener(InformReadyTask);
		newTask.OnCompleted.AddListener(RemoveCompletedTask);
		if (firstPriorityTaskIndex == 0)
		{
			newTask.OnCompleted.AddListener(CheckAllTasksCompleted);
		}
	}

	public Task FindTaskById(int taskId)
	{
		Task task = null;
		int i = 0;
		while (task == null && i < tasks.Count)
		{
			if (tasks[i].id == taskId)
			{
				task = tasks[i];
			}
			i++;
		}
		return task;
	}

	public void StartTasks()
	{
		for (int i = 0; i < firstPriorityTaskIndex; i++)
		{
			tasks[i].StartTask();
		}
	}

	private void AddReadyTask(int taskId)
	{
		Task readyTask = FindTaskById(taskId);
		if (readyTask != null)
		{
			if(taskId < firstPriorityTaskIndex)
			{
				readyTasks.Add(readyTask);
			}
			else
			{
				// Add priority tasks at the beginning
				int i = 0;
				while(i < readyTasks.Count && readyTasks[i].id >= firstPriorityTaskIndex)
				{
					i++;
				}
				readyTasks.Insert(i, readyTask);
				// Call OnEnterCorrectiveTaskMode only for the first corrective task
				// if there're more corrective tasks activated during this time
				// there's no need to run it again
				if(i == 0 && OnEnterCorrectiveTaskMode != null)
				{
					OnEnterCorrectiveTaskMode.Invoke();
				}
			}
			if (readyTasks.Count == 1)
			{
				CurrentTask = readyTask;
			}
		}
	}

	private void RemoveCompletedTask(int taskId)
	{
		Task task = null;
		int i = 0;
		while (task == null && i < readyTasks.Count)
		{
			if (readyTasks[i].id == taskId)
			{
				task = readyTasks[i];
			}
			else
			{
				i++;
			}
		}
		if (task != null)
		{
			readyTasks.RemoveAt(i);
			if (i == 0)
			{
				if (readyTasks.Count > 0)
				{
					CurrentTask = readyTasks[0];
				}
			}
			if (task.id >= firstPriorityTaskIndex)
			{
				// Only get out of priority mode after all priority tasks are completed
				if (readyTasks.Count == 0 || readyTasks[0].id < firstPriorityTaskIndex)
				{
					if (OnExitCorrectiveTaskMode != null)
					{
						OnExitCorrectiveTaskMode.Invoke();
					}
				}
			}
		}
	}

	private void CheckAllTasksCompleted(int taskId)
	{
		numberOfCompletedTasks++;
		if (numberOfCompletedTasks == firstPriorityTaskIndex)
		{
			if (OnAllTasksCompleted != null)
			{
				OnAllTasksCompleted.Invoke();
			}
		}
	}

	public void ToogleFastForwardMode()
	{
		FastForwardModeOn = !FastForwardModeOn;
	}

	private void InformReadyTask(int taskId)
	{
		if (OnTaskReady != null)
		{
			OnTaskReady.Invoke(taskId);
		}
	}

	public void ExecuteCorrectiveTask(string correctiveTaskName)
	{
		Task correctiveTask = FindPriorityTaskByName(correctiveTaskName);
		if (correctiveTask != null)
		{
			correctiveTask.ResetTask();
			correctiveTask.StartTask();
		}
	}

	public void ResumeTasks()
	{
		for (int i = 0; i < readyTasks.Count; i++)
		{
			Task task = readyTasks[i];
			for (int j = 0; j < task.subtasks.Count; j++)
			{
				Subtask subtask = task.subtasks[j];
				if (subtask.Status == Subtask.SubtaskStatus.CANT_COMPLETE)
				{
					subtask.ResetSubtask();
					subtask.StartSubtask();
				}
			}
		}
	}

	private Task FindPriorityTaskByName(string correctiveTaskName)
	{
		Task foundTask = null;
		int i = firstPriorityTaskIndex;
		while(foundTask == null && i < tasks.Count)
		{
			if (tasks[i].description == correctiveTaskName)
			{
				foundTask = tasks[i];
			}
			else
			{
				i++;
			}
		}
		return foundTask;
	}

	public bool IsPriorityTask(Task taskToCheck)
	{
		return taskToCheck.id >= firstPriorityTaskIndex;
	}
}
