using System;
using System.Collections.Generic;
using Plus.Core.FigureData.Types;
using Plus.HabboHotel.Avatar;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Clothing.Parts;

namespace Plus.Core.FigureData;

public interface IFigureDataManager
{
    public const string DefaultFigure = "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
    
    void Init();
    string ValidateFigure(Habbo habbo, string figure, ClothingGender gender, ICollection<ClothingParts> clothingParts);
    Palette GetPalette(int colorId);
    bool TryGetPalette(int palletId, out Palette palette);
    int GetRandomColor(int palletId);
    public Tuple<SetType, int, int, int?> ParseSetPart(string part);
    public string GenerateRandomSet(SetType type, ClothingGender gender);
}