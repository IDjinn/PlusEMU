using System.Collections.Generic;

namespace Plus.Core.FigureData.Types;

internal record FigureSet(
    int Id, 
    SetType Type,
    int PaletteId, 
    string Gender, 
    int ClubLevel, 
    bool Colorable, 
    bool Selectable, 
    bool PreSelectable, 
    Dictionary<string, Part> Parts
    );