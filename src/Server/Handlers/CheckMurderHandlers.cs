using Lotus.API;
using Lotus.Server.Interfaces;

namespace Lotus.Server.Handlers;

internal class CheckMurderHandlers
{
    public static ICheckMurderHandler StandardHandler = new Standard();
    public static ICheckMurderHandler ProtectionPatchedHandler = new ProtectionPatched();

    private class Standard : ICheckMurderHandler
    {
        public void CheckMurder(PlayerControl killer, PlayerControl target)
        {
            ProtectedRpc.Unpatched.CheckMurder(killer, target);
        }
    }

    private class ProtectionPatched: ICheckMurderHandler
    {
        public void CheckMurder(PlayerControl killer, PlayerControl target)
        {
            ProtectedRpc.ProtectionPatch.CheckMurder(killer, target);
        }
    }
}