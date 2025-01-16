using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadSafeInvoker : MonoBehaviour
{
    private static Queue<Action> actionsToExecuteOnMainThread = new Queue<Action>();

    public static void ExecuteInMainThread(Action action)
    {
        lock (actionsToExecuteOnMainThread)
        {
            actionsToExecuteOnMainThread.Enqueue(action);
        }
    }

    private void Update()
    {
        while (actionsToExecuteOnMainThread.Count > 0)
        {
            Action action = null;
            lock (actionsToExecuteOnMainThread)
            {
                if (actionsToExecuteOnMainThread.Count > 0)
                {
                    action = actionsToExecuteOnMainThread.Dequeue();
                }
            }

            action?.Invoke();
        }
    }
}