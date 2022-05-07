using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dapper;
using NLog;
using Plus.Core.FigureData.Types;
using Plus.Database;
using Plus.HabboHotel.Avatar;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities;
using Plus.Utilities.Collections;

namespace Plus.Core.FigureData;

//#define DJINN_FIGURE_MANAGER_INSERT_TEST

public class FigureDataManager : IFigureDataManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Core.FigureData");
    private readonly Dictionary<SetType, Dictionary<int, FigureSet>> _setTypes = new();
    private readonly Dictionary<int, Palette> _palettes = new();

    private static readonly SetType[] Requirements = {
        SetType.Hd,
        SetType.Ch,
        SetType.Lg,
    };
    

    private readonly IDatabase db;
    
    public FigureDataManager(IDatabase db)
    {
        this.db = db;
    }


    public void Init()
    {
        if (_setTypes.Any()) _setTypes.Clear();
        if (_palettes.Any()) _palettes.Clear();

        foreach (var setType in Enum.GetValues<SetType>())
        {
            _setTypes[setType] = new Dictionary<int, FigureSet>();
        }
        
        using var connection = db.Connection();
        var sets = connection.Query("SELECT * FROM figure_sets");
        foreach (var setObj in sets)
        {
            var figureSet = new FigureSet
            {
                Id = setObj.id,
                Type = SetTypeExtensions.ParseFromString(setObj.type),
                PaletteId = setObj.palette_id,
                Gender = ClothingGenderExtensions.ParseFromString(setObj.gender),
                ClubLevel = setObj.club_level,
                Colorable = (bool) setObj.colorable,
                Selectable = (bool) setObj.selectable,
                PreSelectable = (bool) setObj.pre_selectable,
            };
            
            _setTypes[figureSet.Type][figureSet.Id] = figureSet;
        }
        
        Log.Info("Loaded " + _palettes.Count + " Color Palettes");
        Log.Info("Loaded " + _setTypes.Count + " Set Types");
    }

    public string ValidateFigure(Habbo habbo, string figure, ClothingGender gender,
        ICollection<ClothingParts> clothingParts)
    {
        var sb = new StringBuilder(figure.Length);
        var parts = figure.Trim().ToLower().Split('.');
        var partBuilder = new StringBuilder();
        var validSetTypes = new List<SetType>();
        foreach (var part in parts)
        {
            try
            {
                var (setType, setId, colorId, secondColorId) = ParseSetPart(part);
                partBuilder.Clear();

                if (!_setTypes.ContainsKey(setType))
                    continue;

                var _sets = _setTypes[setType];
                if (!_sets.ContainsKey(setId))
                    continue;

                var set = _sets[setId];
                if (set.ClubLevel > habbo.VipRank)
                    continue;

                partBuilder.Append(setType.AsString());
                partBuilder.Append('-');
                partBuilder.Append(setId);
                if (!_palettes.ContainsKey(set.PaletteId))
                    continue;

                var palette = _palettes[set.PaletteId];
                if (!palette.Colors.ContainsKey(colorId))
                {
                    partBuilder.Append('-');
                    partBuilder.Append(GetRandomColor(set.PaletteId));
                    goto appendValidPart;
                }

                var color = palette.Colors[colorId];
                if (color.ClubLevel > habbo.VipRank)
                {
                    partBuilder.Append('-');
                    partBuilder.Append(GetRandomColor(set.PaletteId));
                    goto appendValidPart;
                }

                partBuilder.Append('-');
                partBuilder.Append(colorId);
                if (secondColorId is int secondColor and > 0)
                {
                    if (set.Colorable && !palette.Colors.ContainsKey(secondColor))
                        goto appendValidPart;

                    partBuilder.Append('-');
                    partBuilder.Append(secondColor);
                }

                appendValidPart:
                {
                    partBuilder.Append('.');
                    sb.Append(partBuilder);
                    validSetTypes.Add(setType);
                }
            }
            catch (ArgumentOutOfRangeException){}
        }

        foreach (var requirement in Requirements)
        {
            if(validSetTypes.Contains(requirement))
                continue;

            sb.Append(GenerateRandomSet(requirement, gender));
            sb.Append('.');
        }
        
        return sb.ToString(0 , sb.Length - 1);
    }

    public string GenerateRandomSet(SetType type, ClothingGender gender)
    {
        var (setId, set) = _setTypes[type].Where(x =>
            x.Value.ClubLevel == 0 
            && x.Value.Gender == ClothingGender.Unisex || x.Value.Gender == gender
        ).GetRandomValue();
        
        var color = GetRandomColor(set.PaletteId);
        return $"{type.AsString()}-{setId}-{color}";
    }

    /// <summary>
    /// Parse a set type from a figure string.
    /// </summary>
    /// <example>
    /// <code>
    /// string part = "hd-180-2-0";
    /// var (type, id, color, secondColor) = ParseSetType(part);
    /// // type = "hd"
    /// // id = 180
    /// // color = 2
    /// // secondColor = 0
    /// </code>
    /// </example>
    /// <param name="part"></param>
    /// <returns></returns>
    public Tuple<SetType, int, int, int?> ParseSetPart(string part)
    {
        var split = part.Split('-');
        var type = SetTypeExtensions.ParseFromString(split[0]);
        var setId = Convert.ToInt32(split[1]);
        var colorId = Convert.ToInt32(split[2]);
        int? secondColorId = split.Length > 3 ? Convert.ToInt32(split[3]) : null;
        
        return Tuple.Create(type, setId, colorId, secondColorId);
    }

    public Palette GetPalette(int colorId) => _palettes.FirstOrDefault(x => x.Value.Colors.ContainsKey(colorId)).Value;

    public bool TryGetPalette(int palletId, out Palette palette) => _palettes.TryGetValue(palletId, out palette);

    public int GetRandomColor(int palletId) => _palettes[palletId].Colors.FirstOrDefault(x => x.Value.ClubLevel == 0).Value.Id;
}