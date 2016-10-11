/*     INFINITY CODE 2013-2016      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections.Generic;

#if !UNITY_WEBGL
using System.Threading;
#endif

/// <summary>
/// This class manages the background threads.\n
/// <strong>Please do not use it if you do not know what you're doing.</strong>
/// </summary>
public class OnlineMapsThreadManager
{
#if !UNITY_WEBGL
    private static OnlineMapsThreadManager instance;

    private bool isAlive;

#if !NETFX_CORE
    private Thread thread;
#else
    private OnlineMapsThreadWINRT thread;
#endif
    private List<Action> threadActions;
#endif

    private OnlineMapsThreadManager(Action action)
    {
#if !UNITY_WEBGL
        instance = this;
        threadActions = new List<Action>();
        Add(action);

        isAlive = true;

#if !NETFX_CORE
        thread = new Thread(StartNextAction);
#else
        thread = new OnlineMapsThreadWINRT(StartNextAction);
#endif
        thread.Start();
#endif
    }

    /// <summary>
    /// Adds action queued for execution in a separate thread.
    /// </summary>
    /// <param name="action">Action to be executed.</param>
    public static void AddThreadAction(Action action)
    {
#if !UNITY_WEBGL
        if (instance == null) instance = new OnlineMapsThreadManager(action);
        else instance.Add(action);
#else
        throw new Exception("AddThreadAction not supported for WebGL.");
#endif
    }

    private void Add(Action action)
    {
#if !UNITY_WEBGL
        lock (threadActions)
        {
            threadActions.Add(action);
        }
#endif
    }

    /// <summary>
    /// Disposes of thread manager.
    /// </summary>
    public static void Dispose()
    {
#if !UNITY_WEBGL
        if (instance != null)
        {
            instance.isAlive = false;
            instance.thread = null;
            instance = null;
        }
#endif
    }

    private void StartNextAction()
    {
#if !UNITY_WEBGL
        while (isAlive)
        {
            bool actionInvoked = false;
            lock (threadActions)
            {
                if (threadActions.Count > 0)
                {
                    Action action = threadActions[0];
                    threadActions.RemoveAt(0);
                    action();
                    actionInvoked = true;
                }
            }
            if (!actionInvoked)
            {
#if !NETFX_CORE
                Thread.Sleep(1);
#else
                OnlineMapsThreadWINRT.Sleep(1);
#endif
            }
        }
        threadActions = null;
#endif
    }
}