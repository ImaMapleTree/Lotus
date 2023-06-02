using System;
using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.GUI.Name.Impl;
using Lotus.API;
using VentLib.Utilities.Optionals;

namespace Lotus.GUI.Name.Interfaces;

public interface INameModelComponent
{
    public void AddPrefix(Ubifix prefix);

    public void AddSuffix(Ubifix suffix);

    public void AddViewer(PlayerControl player);

    public INameModelComponent Clone();

    public GameState[] GameStates();

    public string GenerateText();

    public void SetMainText(LiveString liveString);

    public void SetViewerSupplier(Func<List<PlayerControl>> viewers);

    public Optional<float> Size();

    public void RemovePrefix(Ubifix prefix);

    public void RemoveSuffix(Ubifix suffix);

    public void RemoveViewer(byte playerId);

    public List<PlayerControl> Viewers();

    public ViewMode ViewMode();


}