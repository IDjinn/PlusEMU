using System.Collections.Generic;
using Plus.HabboHotel.Avatar;

namespace Plus.HabboHotel.FigureData.Types;

public class FigureSet
{
    public int Id { get; init; }
    public SetType Type { get; init; }
    public int PaletteId { get; init; }
    public ClothingGender Gender { get; init; }
    public int ClubLevel { get; init; }
    public bool Colorable { get; init; }
    public bool Selectable { get; init; }
    public bool PreSelectable { get; init; }
    public Dictionary<string, Part> Parts { get; } = new();

    public FigureSet()
    {
    }

    public FigureSet(int id, SetType setType, int paletteId, ClothingGender gender, int clubLevel, bool colorable,
        bool selectable, bool preSelectable)
    {
        Id = id;
        Type = setType;
        PaletteId = paletteId;
        Gender = gender;
        ClubLevel = clubLevel;
        Colorable = colorable;
        Selectable = selectable;
        PreSelectable = preSelectable;
    }
}