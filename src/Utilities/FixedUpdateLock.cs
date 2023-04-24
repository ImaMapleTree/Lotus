using System;
using VentLib.Utilities.Debug.Profiling;

namespace TOHTOR.Utilities;

public class FixedUpdateLock
{
    public double LockDuration;
    public TimeUnit TimeUnit;
    private DateTime lastAcquire = DateTime.Now;

    public FixedUpdateLock(double duration, TimeUnit timeUnit = TimeUnit.Seconds)
    {
        LockDuration = duration;
    }

    public bool AcquireLock()
    {
        double elapsedTime = DateTime.Now.Subtract(lastAcquire).TotalMilliseconds;
        if (TimeUnit is TimeUnit.Seconds) elapsedTime *= 1000;

        bool acquirable = elapsedTime > LockDuration;
        if (acquirable) lastAcquire = DateTime.Now;

        return acquirable;
    }

    public FixedUpdateLock Clone()
    {
        return (FixedUpdateLock)this.MemberwiseClone();
    }
}