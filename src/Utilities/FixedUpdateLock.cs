using System;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Interfaces;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.Utilities;

[NewOnSetup]
public class FixedUpdateLock: ICloneOnSetup<FixedUpdateLock>
{
    public double LockDuration;
    public TimeUnit TimeUnit;
    private DateTime lastAcquire = DateTime.Now;

    public FixedUpdateLock(bool beginUnlocked = true): this(ModConstants.RoleFixedUpdateCooldown, beginUnlocked: beginUnlocked)
    {
    }

    public FixedUpdateLock(double duration, TimeUnit timeUnit = TimeUnit.Seconds, bool beginUnlocked = true)
    {
        LockDuration = duration;
        TimeUnit = timeUnit;
        if (beginUnlocked) Unlock();
    }

    public bool AcquireLock()
    {
        bool acquirable = IsUnlocked();
        if (acquirable) lastAcquire = DateTime.Now;
        return acquirable;
    }

    public bool IsUnlocked()
    {
        double elapsedTime = DateTime.Now.Subtract(lastAcquire).TotalMilliseconds;
        if (TimeUnit is TimeUnit.Seconds) elapsedTime /= 1000;
        return elapsedTime > LockDuration;
    }

    public void Unlock()
    {
        lastAcquire = DateTime.MinValue;
    }

    public FixedUpdateLock Clone()
    {
        return (FixedUpdateLock)this.MemberwiseClone();
    }
}