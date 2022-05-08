using System.Collections.Generic;

namespace Plus.HabboHotel.FigureData.Types;

public class Palette
{
    public Palette(int id) => Id = id;
    public int Id { get; }
    public Dictionary<int, Color> Colors { get; } = new();
}