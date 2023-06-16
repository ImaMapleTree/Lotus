using UnityEngine;

namespace Lotus.Statuses;

public interface IStatus
{
    public string Name { get; set; }

    public Color Color { get; set; }

    public string Description { get; set; }

    public StatusFlag StatusFlags { get; set; }

    public void Clear();

    public void Apply(PlayerControl player);
}