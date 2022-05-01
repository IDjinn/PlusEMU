using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NLog;
using Plus.Core.FigureData.Types;
using Plus.HabboHotel.Users.Clothing.Parts;
using Plus.Utilities;

namespace Plus.Core.FigureData;

public class FigureDataManager : IFigureDataManager
{
    private static readonly ILogger Log = LogManager.GetLogger("Plus.Core.FigureData");
    private readonly Dictionary<int, Palette> _palettes; //pallet id, Pallet

    private readonly List<string> _requirements;
    private readonly Dictionary<string, FigureSet> _setTypes; //type (hr, ch, etc), Set

    public FigureDataManager()
    {
        _palettes = new Dictionary<int, Palette>();
        _setTypes = new Dictionary<string, FigureSet>();
        _requirements = new List<string>
        {
            "hd",
            "ch",
            "lg"
        };
    }

    public void Init()
    {
        if (_palettes.Count > 0)
            _palettes.Clear();
        if (_setTypes.Count > 0)
            _setTypes.Clear();
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
                var type = child.Attributes!["type"]!.Value;
                var setType = SetTypeUtility.GetSetType(type);
                var paletteId = Convert.ToInt32(child.Attributes!["paletteid"]!.Value);
                var figureSet = new FigureSet(setType, paletteId);
                _setTypes.Add(type, figureSet);
                foreach (XmlNode sub in child.ChildNodes)
                {
                    var subId = Convert.ToInt32(sub.Attributes!["id"]!.Value);
                    var gender = sub.Attributes!["gender"]!.Value;
                    var clubLevel = Convert.ToInt32(sub.Attributes!["club"]!.Value);
                    var colorable = Convert.ToInt32(sub.Attributes!["colorable"]!.Value) == 1;
                    var selectable = Convert.ToInt32(sub.Attributes!["selectable"]!.Value) == 1;
                    var preSelectable = Convert.ToInt32(sub.Attributes!["preselectable"]!.Value) == 1;

                    var set = new Set(subId, gender, clubLevel, colorable, selectable, preSelectable);
                    _setTypes[type].Sets.Add(subId,set);
                    foreach (XmlNode subPart in sub.ChildNodes)
                    {
                        if (subPart.Attributes!["type"] is null)
                            continue;
                        
                        var subPartId = Convert.ToInt32(subPart.Attributes!["id"]!.Value);
                        var subPartSetType = SetTypeUtility.GetSetType(subPart.Attributes!["type"]!.Value);
                        var subPartColorable = Convert.ToInt32(subPart.Attributes!["colorable"]!.Value) == 1;
                        var subPartIndex = Convert.ToInt32(subPart.Attributes!["index"]!.Value);
                        var subPartColorIndex = Convert.ToInt32(subPart.Attributes!["colorindex"]!.Value);

                        var subPartKey = $"{subPartId}-{subPart.Attributes!["type"]!.Value}";
                        
                        var part = new Part(subPartId, subPartSetType, subPartColorable, subPartIndex,
                            subPartColorIndex);
                        
                        _setTypes[type].Sets[subId].Parts.Add(subPartKey, part);
                    }
                }
            }
        }

        //Faceless.
        _setTypes["hd"].Sets.Add(99999, new Set(99999, "U", 0, true, false, false));
        Log.Info("Loaded " + _palettes.Count + " Color Palettes");
        Log.Info("Loaded " + _setTypes.Count + " Set Types");
    }

    public string ProcessFigure(string figure, string gender, ICollection<ClothingParts> clothingParts, bool hasHabboClub)
    {
        figure = figure.ToLower();
        gender = gender.ToUpper();
        var rebuildFigure = string.Empty;
        var figureParts = figure.Split('.');
        foreach (var part in figureParts.ToList())
        {
            var type = part.Split('-')[0];
            if (_setTypes.TryGetValue(type, out var figureSet))
            {
                var partId = Convert.ToInt32(part.Split('-')[1]);
                var colorId = 0;
                var secondColorId = 0;
                if (figureSet.Sets.TryGetValue(partId, out var set))
                {
                    if (set.Gender != gender && set.Gender != "U")
                    {
                        if (figureSet.Sets.Count(x => x.Value.Gender == gender || x.Value.Gender == "U") > 0)
                        {
                            partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;

                            //Fetch the new set.
                            figureSet.Sets.TryGetValue(partId, out set);
                            colorId = GetRandomColor(figureSet.PalletId);
                        }
                    }
                    if (set.Colorable)
                    {
                        //Couldn't think of a better way to split the colors, if I looped the parts I still have to remove Type-PartId, then loop color 1 & color 2. Meh
                        var splitterCounter = part.Count(x => x == '-');
                        if (splitterCounter == 2 || splitterCounter == 3)
                        {
                            if (!string.IsNullOrEmpty(part.Split('-')[2]))
                            {
                                if (int.TryParse(part.Split('-')[2], out colorId))
                                {
                                    colorId = Convert.ToInt32(part.Split('-')[2]);
                                    var palette = GetPalette(colorId);
                                    if (palette != null && colorId != 0)
                                    {
                                        if (figureSet.PalletId != palette.Id) colorId = GetRandomColor(figureSet.PalletId);
                                    }
                                    else if (palette == null && colorId != 0) colorId = GetRandomColor(figureSet.PalletId);
                                }
                                else
                                    colorId = 0;
                            }
                            else
                                colorId = 0;
                        }
                        if (splitterCounter == 3)
                        {
                            if (!string.IsNullOrEmpty(part.Split('-')[3]))
                            {
                                if (int.TryParse(part.Split('-')[3], out secondColorId))
                                {
                                    secondColorId = Convert.ToInt32(part.Split('-')[3]);
                                    var palette = GetPalette(secondColorId);
                                    if (palette != null && secondColorId != 0)
                                    {
                                        if (figureSet.PalletId != palette.Id) secondColorId = GetRandomColor(figureSet.PalletId);
                                    }
                                    else if (palette == null && secondColorId != 0) secondColorId = GetRandomColor(figureSet.PalletId);
                                }
                                else
                                    secondColorId = 0;
                            }
                            else
                                secondColorId = 0;
                        }
                    }
                    else
                    {
                        var ignore = new[] { "ca", "wa" };
                        if (ignore.Contains(type))
                            if (!string.IsNullOrEmpty(part.Split('-')[2]))
                                colorId = Convert.ToInt32(part.Split('-')[2]);
                    }
                    if (set.ClubLevel > 0 && !hasHabboClub)
                    {
                        partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U" && x.Value.ClubLevel == 0).Value.Id;
                        figureSet.Sets.TryGetValue(partId, out set);
                        colorId = GetRandomColor(figureSet.PalletId);
                    }
                    if (secondColorId == 0)
                        rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + ".";
                    else
                        rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + "-" + secondColorId + ".";
                }
            }
        }
        foreach (var requirement in _requirements)
        {
            if (!rebuildFigure.Contains(requirement))
            {
                if (requirement == "ch" && gender == "M")
                    continue;
                if (_setTypes.TryGetValue(requirement, out var figureSet))
                {
                    var set = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value;
                    if (set != null)
                    {
                        var partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;
                        var colorId = GetRandomColor(figureSet.PalletId);
                        rebuildFigure = rebuildFigure + requirement + "-" + partId + "-" + colorId + ".";
                    }
                }
            }
        }
        if (clothingParts != null)
        {
            var purchasableParts = PlusEnvironment.GetGame().GetCatalog().GetClothingManager().GetClothingAllParts;
            figureParts = rebuildFigure.TrimEnd('.').Split('.');
            foreach (var part in figureParts.ToList())
            {
                var partId = Convert.ToInt32(part.Split('-')[1]);
                if (purchasableParts.Count(x => x.PartIds.Contains(partId)) > 0)
                {
                    if (clothingParts.Count(x => x.PartId == partId) == 0)
                    {
                        var type = part.Split('-')[0];
                        if (_setTypes.TryGetValue(type, out var figureSet))
                        {
                            var set = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value;
                            if (set != null)
                            {
                                partId = figureSet.Sets.FirstOrDefault(x => x.Value.Gender == gender || x.Value.Gender == "U").Value.Id;
                                var colorId = GetRandomColor(figureSet.PalletId);
                                rebuildFigure = rebuildFigure + type + "-" + partId + "-" + colorId + ".";
                            }
                        }
                    }
                }
            }
        }
        return rebuildFigure;
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