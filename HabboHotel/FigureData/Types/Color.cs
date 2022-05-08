namespace Plus.HabboHotel.FigureData.Types;

public class Color
{
    public Color()
    {
        
    }
    public Color(int id, int paletteId, int index, int clubLevel, bool selectable, string value)
    {
        Id = id;
        PaletteId = paletteId;
        Index = index;
        ClubLevel = clubLevel;
        Selectable = selectable;
        Value = value;
    }

    public int Id { get; init; }
    public int PaletteId { get; init; }
    public int Index { get; init; }
    public int ClubLevel { get; init; }
    public bool Selectable { get; init; }
    public string Value { get; init; }
}