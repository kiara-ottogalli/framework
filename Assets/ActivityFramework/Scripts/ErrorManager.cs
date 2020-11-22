using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class AppError
{
    public GameObject errorGameObject;
    public string errorName;
    public string errorMessage;
}

public class ErrorManager : MonoBehaviour
{
    [SerializeField]
    private bool error = false;
    public bool Error
    {
        get
        {
            return error;
        }
        set
        {
            if (error != value)
            {
                error = value;
            }
        }
    }

    public List<AppError> errors = new List<AppError>();

    public UnityStringEvent OnErrorChanged;


    public void AddError(GameObject errorObject)
    {
        AppError appError = null;
        AppError appNewError = GetNewAppError(errorObject);
        int position = FindPositionOfGameObjectError(errorObject);
        if (position == -1)
        {
            appError = appNewError;
            errors.Add(appError);
            if (errors.Count == 1)
            {
                if (OnErrorChanged != null)
                {
                    OnErrorChanged.Invoke(appError.errorName);
                }
            }
        }
        else
        {
            appError = errors[position];
            bool updated = UpdateError(appError, appNewError);
            if (updated && position == 0)
            {
                if (OnErrorChanged != null)
                {
                    OnErrorChanged.Invoke(appError.errorName);
                }
            }
        }
        Error = true;
    }

    public void RemoveError(GameObject errorObject)
    {
        int position = FindPositionOfGameObjectError(errorObject);
        if (position != -1 && position < errors.Count)
        {
            errors.RemoveAt(position);
            // If the position removed was the first
            if (position == 0)
            {
                if (OnErrorChanged != null)
                {
                    OnErrorChanged.Invoke(errors.Count == 0 ? "" : errors[0].errorName);
                }
            }
            // If all errors have been removed
            if (errors.Count == 0)
            {
                Error = false;
            }
        }
    }

    public string GetErrorMessage()
    {
        if (errors.Count > 0)
        {
            return errors[0].errorMessage;
        }
        return "";
    }

    private bool UpdateError(AppError appError, AppError appNewError)
    {
        if (appError.errorName != appNewError.errorName)
        {
            appError.errorName = appNewError.errorName;
            appError.errorMessage = appNewError.errorMessage;
            return true;
        }
        return false;
    }

    private int FindPositionOfGameObjectError(GameObject gameObject)
    {
        bool found = false;
        int i = 0;
        while (!found && i < errors.Count)
        {
            if (errors[i].errorGameObject == gameObject)
            {
                found = true;
            }
            else
            {
                i++;
            }
        }
        if (found)
        {
            return i;
        }
        else
        {
            return -1;
        }
    }

    private AppError GetNewAppError(GameObject errorObject)
    {
        AppError appError = new AppError();
        appError.errorGameObject = errorObject;
        switch (appError.errorGameObject.name)
        {
            // Configure the errors according to the actors
            default:
                break;
        }
        return appError;
    }
}
