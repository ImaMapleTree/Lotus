using Lotus.Server.Patches;
using VentLib.Options;
using VentLib.Options.Game;
using VentLib.Options.IO;

namespace Lotus.Options.Client;

public class AdvancedOptions
{
    public bool PublicCompatability
    {
        get => publicCompatability;
        set => publicServerCompatabilityPatch.SetValue(value ? 1 : 0);
    }

    private bool publicCompatability;

    private GameOption publicServerCompatabilityPatch;

    public AdvancedOptions()
    {
        OptionManager defaultManager = OptionManager.GetManager();
        publicServerCompatabilityPatch = new GameOptionBuilder()
            .Key("Public Server Compatability Patch")
            .Name("Public Server Compatability Patch")
            .Description("An experimental patch that allows for the discovery of Lotus on public server")
            .Values(0, false, true)
            .BindBool(b =>
            {
                defaultManager.DelaySave(0);
                if (b) ProjectLotus.ServerPatchManager.AddPatch(ProtectionPatchedServerImplementation.Instance);
                else ProjectLotus.ServerPatchManager.RemovePatch(ProtectionPatchedServerImplementation.Instance);
                publicCompatability = b;
            })
            .BuildAndRegister();
    }
}