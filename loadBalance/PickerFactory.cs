using System.Collections.Concurrent;

namespace mini_gateway.loadBalance;

public delegate IPicker Factory();

public static class PickerFactory
{
    static PickerFactory()
    {
        Register("rotation", (() => new RotationPicker()));
    }

    private static readonly ConcurrentDictionary<string, Factory> Map = new();

    public static void Register(string name, Factory factory)
    {
        Map.TryAdd(name, factory);
    }

    public static Factory? GetPicker(string name)
    {
        return Map.TryGetValue(name, out var factory) ? factory : null;
    }
}