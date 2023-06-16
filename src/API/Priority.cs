namespace Lotus.API;

public enum Priority: uint
{
    First = 0,
    VeryHigh = 200,
    High = 400,
    Normal = 600,
    Low = 800,
    VeryLow = 1000,
    Last = uint.MaxValue
}