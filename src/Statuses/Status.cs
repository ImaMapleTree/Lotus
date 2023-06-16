using System;
using UnityEngine;

namespace Lotus.Statuses;

public class CustomStatus: IStatus
{
    public string Name { get; set; }
    public Color Color { get; set; } = Color.white;
    public string Description { get; set; }
    public StatusFlag StatusFlags { get; set; }

    private Action<PlayerControl>? applyFunc;
    private Action? clearAction;

    public static CustomStatusBuilder Of(string name)
    {
        return new CustomStatusBuilder(name);
    }

    public void Clear()
    {
        clearAction?.Invoke();
    }

    public void Apply(PlayerControl player)
    {
        applyFunc?.Invoke(player);
    }

    public class CustomStatusBuilder
    {
        private CustomStatus status = new CustomStatus();

        public CustomStatusBuilder(string name)
        {
            status.Name = name;
        }

        public CustomStatusBuilder Name(string name)
        {
            status.Name = name;
            return this;
        }

        public CustomStatusBuilder Color(Color color)
        {
            status.Color = color;
            return this;
        }

        public CustomStatusBuilder Description(string description)
        {
            status.Description = description;
            return this;
        }

        public CustomStatusBuilder StatusFlags(StatusFlag flag)
        {
            status.StatusFlags = flag;
            return this;
        }

        public CustomStatusBuilder Clear(Action clearAction)
        {
            status.clearAction = clearAction;
            return this;
        }

        public CustomStatusBuilder Apply(Action<PlayerControl> applyFunc)
        {
            status.applyFunc = applyFunc;
            return this;
        }

        public CustomStatus Build()
        {
            return status;
        }
    }
}