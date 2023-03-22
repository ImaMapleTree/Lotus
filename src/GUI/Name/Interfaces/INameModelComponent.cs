using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.GUI.Name.Impl;
using VentLib.Utilities.Optionals;

namespace TOHTOR.GUI.Name.Interfaces;

public interface INameModelComponent
{
    public void AddPrefix(Ubifix prefix);

    public void RemovePrefix(Ubifix prefix);

    public void AddSuffix(Ubifix suffix);

    public void RemoveSuffix(Ubifix suffix);

    public void SetMainText(LiveString liveString);

    public GameState[] GameStates();

    public Optional<float> Size();

    public List<PlayerControl> Viewers();

    public void SetViewerSupplier(Func<List<PlayerControl>> viewers);

    public void AddViewer(PlayerControl player);

    public void RemoveViewer(byte playerId);

    public string GenerateText();

    public ViewMode ViewMode();

    public INameModelComponent Clone();
}