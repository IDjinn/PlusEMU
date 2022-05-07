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
using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities;
using Plus.Utilities.Collections;

namespace Plus.Core.FigureData;

//#define DJINN_FIGURE_MANAGER_INSERT_TEST

public class FigureDataManager : IFigureDataManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Core.FigureData");
    private readonly Dictionary<int, Palette> _palettes = new(); //pallet id, Pallet

    private static readonly SetType[] Requirements = {
        SetType.Hd,
        SetType.Ch,
        SetType.Lg,
    };
    
    private readonly Dictionary<SetType, Dictionary<int, FigureSet>> _setTypes = new(); //type (hr, ch, etc), Set

    private readonly IDatabase db;
    
    public FigureDataManager(IDatabase db)
    {
        this.db = db;
    }


    public void Init()
    {
        if (_setTypes.Any()) _setTypes.Clear();
        if (_palettes.Any()) _palettes.Clear();
        using var connection = db.Connection();

        foreach (var setType in Enum.GetValues<SetType>())
        {
            _setTypes[setType] = new Dictionary<int, FigureSet>();
        }

        var projectSolutionPath = Directory.GetCurrentDirectory();
        var xDoc = new XmlDocument();
        xDoc.Load(projectSolutionPath + "//Config//figuredata.xml");
        var colors = xDoc.GetElementsByTagName("colors");
        foreach (XmlNode node in colors)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var id = Convert.ToInt32(child.Attributes!["id"]!.Value);
                var palette = new Palette(id);
                _palettes.Add(id, palette);
                foreach (XmlNode sub in child.ChildNodes)
                {
                    var subId = Convert.ToInt32(sub.Attributes!["id"]!.Value);
                    var subIndex = Convert.ToInt32(sub.Attributes!["index"]!.Value);
                    var subClubLevel = Convert.ToInt32(sub.Attributes!["club"]!.Value);
                    var selectable = Convert.ToInt32(sub.Attributes!["selectable"]!.Value) == 1;
                    var color = new Color(subId, subIndex, subClubLevel, selectable, sub.InnerText);
                    _palettes[id].Colors.Add(subId, color);
                }
            }
        }

        var sets = xDoc.GetElementsByTagName("sets");
        foreach (XmlNode node in sets)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var setType = SetTypeExtensions.ParseFromString(child.Attributes!["type"]!.Value);
                var paletteId = Convert.ToInt32(child.Attributes!["paletteid"]!.Value);
                foreach (XmlNode sub in child.ChildNodes)
                {
                    var subId = Convert.ToInt32(sub.Attributes!["id"]!.Value);
                    var gender = sub.Attributes!["gender"]!.Value;
                    var clubLevel = Convert.ToInt32(sub.Attributes!["club"]!.Value);
                    var colorable = Convert.ToInt32(sub.Attributes!["colorable"]!.Value) == 1;
                    var selectable = Convert.ToInt32(sub.Attributes!["selectable"]!.Value) == 1;
                    var preSelectable = Convert.ToInt32(sub.Attributes!["preselectable"]!.Value) == 1;

                    var set = new FigureSet(subId, setType, paletteId, ClothingGenderExtensions.ParseFromString(gender), clubLevel, colorable, selectable,
                        preSelectable, new());
                    _setTypes[setType].Add(subId, set);
                    foreach (XmlNode subPart in sub.ChildNodes)
                    {
                        if (subPart.Attributes!["type"] is null)
                            continue;

                        var subPartId = Convert.ToInt32(subPart.Attributes!["id"]!.Value);
                        var subPartSetType =
                            SetTypeExtensions.ParseFromString(subPart.Attributes!["type"]!.Value);
                        var subPartColorable =
                            Convert.ToInt32(subPart.Attributes!["colorable"]!.Value) == 1;
                        var subPartIndex = Convert.ToInt32(subPart.Attributes!["index"]!.Value);
                        var subPartColorIndex = Convert.ToInt32(subPart.Attributes!["colorindex"]!.Value);

                        var subPartKey = $"{subPartId}-{subPart.Attributes!["type"]!.Value}";

                        var part = new Part(subPartId, subPartSetType, subPartColorable, subPartIndex,
                            subPartColorIndex);

                        _setTypes[setType][subId].Parts.Add(subPartKey, part);
                    }
                }
            }
        }

        //Faceless.
        _setTypes[SetType.Hd].Add(99999, new FigureSet(99999, SetType.Hd, 99999, ClothingGender.Unisex, 0, true, false, false, new()));
        
        #if DJINN_FIGURE_MANAGER_INSERT_TEST 
        foreach (var (setType, _sets) in _setTypes)
        {
            foreach (var (setId, set) in _sets)
            {
                const string query =
                    @"INSERT INTO `clothing_sets` (set_id, type, palette_id, gender, club_level, colorable, selectable, pre_selectable) VALUE 
                (@set_id, @type, @palette_id, @gender, @club_level, @colorable, @selectable, @pre_selectable);";

                try
                {
                    connection.Execute(query, new
                    {
                        set_id = set.Id,
                        type = set.Type.ToString(),
                        palette_id = set.PaletteId,
                        gender = set.Gender,
                        club_level = set.ClubLevel,
                        colorable = set.Colorable,
                        selectable = set.Selectable,
                        pre_selectable = set.PreSelectable
                    });
                }
                catch (Exception e)
                {
                }
               
            }
        }
        #endif
        
        Log.Info("Loaded " + _palettes.Count + " Color Palettes");
        Log.Info("Loaded " + _setTypes.Count + " Set Types");
    }

    public string ProcessFigure(string figure, ClothingGender gender, ICollection<ClothingParts> clothingParts,
        bool hasHabboClub)
    {
        var sb = new StringBuilder(figure.Length);
        var parts = figure.Trim().ToLower().Split('.');
        foreach (var part in parts)
        {
            var (setType, setId, colorId, secondColorId) = ParseSetPart(part);

            if (!_setTypes.ContainsKey(setType))
                continue; // illegal, we cant filter it out. Should never happen. need logging

            var _sets = _setTypes[setType];
            if (!_sets.ContainsKey(setId))
            {
                sb.Append(GenerateRandomSet(setType, gender));
                sb.Append('.');
                continue;
            }

            var set = _sets[setId];
            if (set.ClubLevel > 0 && !hasHabboClub)
            {
                sb.Append(GenerateRandomSet(setType, gender));
                sb.Append('.');
                continue;
            }
            
            sb.Append(setType.ToStringg());
            sb.Append('-');
            sb.Append(setId);
            if(!_palettes.ContainsKey(set.PaletteId))
                continue; // should never happen...
            
            var pallete = _palettes[set.PaletteId];
            if (!pallete.Colors.ContainsKey(colorId))
            {
                var something = pallete.Colors.Values.Where(x => x.Id == colorId);
                sb.Append('-');
                sb.Append(GetRandomColor(set.PaletteId));
                sb.Append('.');
                continue;
            }

            sb.Append('-');
            sb.Append(colorId);
            if (secondColorId is int secondColor and > 0)
            {
                if (set.Colorable && !pallete.Colors.ContainsKey(secondColor)){
                    sb.Append('.');
                    continue; // second color may not be available, skip it
                }
                
                sb.Append('-');
                sb.Append(secondColor);
            }
            sb.Append('.');
        }

        return sb.ToString(0 , sb.Length - 1);
    }

    private string GenerateRandomSet(SetType type, ClothingGender gender)
    {
        var (setId, set) = _setTypes[type].Where(x =>
            x.Value.Gender == ClothingGender.Unisex || x.Value.Gender == gender
        ).GetRandomValue();
        
        var color = GetRandomColor(set.PaletteId);
        return $"{type}-{setId}-{color}";
    }

    /// <summary>
    /// Parse a set type from a figure string.
    /// </summary>
    /// <example>
    /// <code>
    /// string part = "hd-180-2";
    /// var (type, id, color, secondColor) = ParseSetType(part);
    /// // type = "hd"
    /// // id = 180
    /// // color = 2
    /// // secondColor = 0
    /// </code>
    /// </example>
    /// <param name="part"></param>
    /// <returns></returns>
    private static Tuple<SetType, int, int, int?> ParseSetPart(string part)
    {
        var split = part.Split('-');
        var type = SetTypeExtensions.ParseFromString(split[0]);
        var setId = Convert.ToInt32(split[1]);
        var colorId = Convert.ToInt32(split[2]);
        int? secondColorId = split.Length > 3 ? Convert.ToInt32(split[3]) : null;
        
        return Tuple.Create(type, setId, colorId, secondColorId);
    }

    public Palette GetPalette(int colorId)
    {
        return _palettes.FirstOrDefault(x => x.Value.Colors.ContainsKey(colorId)).Value;
    }

    public bool TryGetPalette(int palletId, out Palette palette) => _palettes.TryGetValue(palletId, out palette);

    public int GetRandomColor(int palletId) => _palettes[palletId].Colors.FirstOrDefault().Value.Id;

    public string FilterFigure(string figure)
    {
        return StringCharFilter.IsValid(figure) ? figure : IFigureDataManager.DefaultFigure;
    }
}