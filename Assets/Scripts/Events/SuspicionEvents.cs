using System;
using UnityEngine;

public enum SuspicionType
{
    Theft,
    LoudNoise,
    Sprinting,
    WeaponDrawn
}

public class SuspicionEvent
{
    public Vector3 Position;
    public float Amount;
    public SuspicionType Type;

    public SuspicionEvent(Vector3 position, float amount, SuspicionType type)
    {
        Position = position;
        Amount = amount;
        Type = type;
    }
}

public static class SuspicionEvents
{
    public static event Action<SuspicionEvent> OnRaised;

    public static void Raise(SuspicionEvent e)
    {
        OnRaised?.Invoke(e);
    }
}