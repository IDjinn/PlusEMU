using System.Collections.Generic;
using Plus.HabboHotel.Avatar;

namespace Plus.Core.FigureData.Types;

internal record FigureSet(
    int Id, 
    SetType Type,
    int PaletteId, 
    ClothingGender Gender, 
    int ClubLevel, 
    bool Colorable, 
    bool Selectable, 
    bool PreSelectable, 
    Dictionary<string, Part> Parts
    );