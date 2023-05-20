using System;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using VentLib.Utilities.Debug.Profiling;

namespace Lotus.Utilities;

[NewOnSetup]
public class FixedUpdateLock: ICloneOnSetup<FixedUpdateLock>
{
    public double LockDuration;
    public TimeUnit TimeUnit;
    private DateTime lastAcquire = DateTime.Now;

    public FixedUpdateLock(): this(ModConstants.RoleFixedUpdateCooldown)
    {
    }
    
    public FixedUpdateLock(double duration, TimeUnit timeUnit = TimeUnit.Seconds)
    {
        LockDuration = duration;
        TimeUnit = timeUnit;
    }

    public bool AcquireLock()
    {
        double elapsedTime = DateTime.Now.Subtract(lastAcquire).TotalMilliseconds;
        if (TimeUnit is TimeUnit.Seconds) elapsedTime /= 1000;

        bool acquirable = elapsedTime > LockDuration;
        if (acquirable) lastAcquire = DateTime.Now;

        return acquirable;
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