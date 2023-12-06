using System;
using System.Collections;
using System.Collections.Generic;
using Lotus.Logging;
using UnityEngine;
using VentLib.Utilities;

namespace Lotus.Managers;

public class CoroutineManager
{
    private bool atomicBoolean;
    private Queue<IEnumerator> enumerators = new();

    public void Start()
    {
        atomicBoolean = true;
        DevLogger.Log($"Enmerators: {enumerators.Count}");
        while (enumerators.TryDequeue(out IEnumerator? enumerator))
            Async.Execute(enumerator);
    }

    public void Stop()
    {
        atomicBoolean = false;
    }

    public void CreateLoop(Func<float> func) => CreateLoopInternal(WaitingEnumerator(func));

    public void CreateLoop(Action action) => CreateLoopInternal(WaitingEnumerator(action));

    private void CreateLoopInternal(IEnumerator enumerator)
    {
        if (atomicBoolean) Async.Execute(enumerator);
        else enumerators.Enqueue(enumerator);
    }

    private IEnumerator WaitingEnumerator(Action action)
    {
        return WaitingEnumerator(() =>
        {
            action();
            return 0.25f;
        });
    }

    private IEnumerator WaitingEnumerator(Func<float> action)
    {
        while (atomicBoolean) yield return new WaitForSeconds(action());
        yield return null;
    }
}