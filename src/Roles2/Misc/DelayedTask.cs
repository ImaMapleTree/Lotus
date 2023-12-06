using System;
using System.Collections.Generic;
using Lotus.Roles2.Attributes;
using Lotus.Roles2.Interfaces;

namespace Lotus.Roles2.Misc;

[SetupInjected(useCloneIfPresent: true), InstantiateOnSetup(true)]
public class DelayedTask
{
    private Queue<Spawn> spawns = new();

    public float Delay { get; set; }
    public bool IsManual { get; }

    public DelayedTask() { }

    public DelayedTask(float delay)
    {
        Delay = delay;
    }

    public DelayedTask(float delay, bool isManual)
    {
        Delay = delay;
        IsManual = isManual;
    }

    public int CheckTasks()
    {
        int size = spawns.Count;
        for (int i = 0; i < size; i++)
        {
            Spawn spawn = spawns.Dequeue();
            if (!spawn.IsCancelled) spawns.Enqueue(spawn);
        }

        return spawns.Count - size;
    }

    public void ExecuteAll()
    {

    }

    public Spawn Start(float delay = -1f)
    {

    }

    public class Spawn
    {
        private Action action;

        internal DateTime CreationTime { get; } = DateTime.Now;
        private DateTime finishTime;

        public bool IsCancelled {
            get {
                if (isCancelled) return true;
                if ((DateTime.Now < finishTime)) return false;
                Finish();
                return true;
            }
        }

        private bool isCancelled;

        public Spawn(Action action, float delay)
        {
            this.action = action;
            finishTime = CreationTime.AddSeconds(delay);
        }

        public void Execute() => action();

        public void Finish()
        {
            isCancelled = true;
            action();
        }

        public void Cancel() => isCancelled = true;
    }
}