using System;
using System.Collections.Generic;
using TOHTOR.API;
using TOHTOR.API.Odyssey;
using TOHTOR.GUI.Name.Impl;
using VentLib.Utilities.Optionals;

namespace TOHTOR.GUI.Name.Interfaces;

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