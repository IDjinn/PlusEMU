using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Dapper;
using NLog;
using NLog.Fluent;
using Plus.Core.FigureData.Types;
using Plus.Database;
using Plus.HabboHotel.Avatar;

namespace Plus.Utilities.Figuredata
{
    public struct FigureDataParseResult
    {
        public Dictionary<SetType, Dictionary<int, FigureSet>> Sets;
        public Dictionary<int, Palette> Palettes;
    }

    public static class FigureDataParser
    {
        private static readonly Logger Log = LogManager.GetLogger("Plus.Utilities.Figuredata.FigureDataParser");

        public static FigureDataParseResult Parse(string path = "//Config//figuredata.xml")
        {
            var xDoc = new XmlDocument();
            xDoc.Load(Directory.GetCurrentDirectory() + path);
            var colors = xDoc.GetElementsByTagName("colors");
            var setTypes = new Dictionary<SetType, Dictionary<int, FigureSet>>();
            var palettes = new Dictionary<int, Palette>();
            foreach (XmlNode node in colors)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    var id = Convert.ToInt32(child.Attributes!["id"]!.Value);
                    var palette = new Palette(id);
                    palettes.Add(id, palette);
                    foreach (XmlNode sub in child.ChildNodes)
                    {
                        var subId = Convert.ToInt32(sub.Attributes!["id"]!.Value);
                        var subIndex = Convert.ToInt32(sub.Attributes!["index"]!.Value);
                        var subClubLevel = Convert.ToInt32(sub.Attributes!["club"]!.Value);
                        var selectable = Convert.ToInt32(sub.Attributes!["selectable"]!.Value) == 1;
                        var color = new Color(subId, subIndex, subClubLevel, selectable, sub.InnerText);
                        palettes[id].Colors.Add(subId, color);
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

                        var set = new FigureSet(subId, setType, paletteId,
                            ClothingGenderExtensions.ParseFromString(gender), clubLevel, colorable, selectable,
                            preSelectable);
                        setTypes[setType].Add(subId, set);
                        foreach (XmlNode subPart in sub.ChildNodes)
                        {
                            if (subPart.Attributes!["type"] is null)
                                continue;

                            var subPartId = Convert.ToInt32(subPart.Attributes!["id"]!.Value);
                            var subPartSetType = SetTypeExtensions.ParseFromString(subPart.Attributes!["type"]!.Value);
                            var subPartColorable = Convert.ToInt32(subPart.Attributes!["colorable"]!.Value) == 1;
                            var subPartIndex = Convert.ToInt32(subPart.Attributes!["index"]!.Value);
                            var subPartColorIndex = Convert.ToInt32(subPart.Attributes!["colorindex"]!.Value);
                            var subPartKey = $"{subPartId}-{subPart.Attributes!["type"]!.Value}";

                            var part = new Part(subPartId, subPartSetType, subPartColorable, subPartIndex,
                                subPartColorIndex);

                            setTypes[setType][subId].Parts.Add(subPartKey, part);
                        }
                    }
                }
            }

            setTypes[SetType.Hd].Add(99999, new FigureSet(99999, SetType.Hd, 99999, ClothingGender.Unisex, 0, true, false, false));
            return new FigureDataParseResult
            {
                Sets = setTypes,
                Palettes = palettes
            };
        }

        public static void Insert(FigureDataParseResult data, IDatabase database)
        {
            const string insertClothingSetsQuery =
                "INSERT INTO figure_sets (id, type, palette_id, gender, club_level, colorable, selectable, pre_selectable) VALUES ";
            const string insertPalettesQuery = "INSERT INTO `clothing_palettes` (id, color_id) VALUES ";
            using var connection = database.Connection();
            var sb = new StringBuilder(1024);
            var count = 0;
            var total = 0;
            foreach (var sets in data.Sets.Values)
            {
                foreach (var (id, set) in sets)
                {
                    sb.Append('(');
                    sb.Append(id);
                    sb.Append(',');
                    sb.Append(set.Type.AsString());
                    sb.Append(',');
                    sb.Append(set.PaletteId);
                    sb.Append(',');
                    sb.Append(ClothingGenderExtensions.ToString(set.Gender));
                    sb.Append(',');
                    sb.Append(set.ClubLevel);
                    sb.Append(',');
                    sb.Append(set.Colorable);
                    sb.Append(',');
                    sb.Append(set.Selectable);
                    sb.Append(',');
                    sb.Append(set.PreSelectable);
                    sb.Append(')');
                    count++;

                    if (count >= 100)
                    {
                        total += count;
                        sb.Append(';');
                        connection.Execute(insertClothingSetsQuery + sb);
                        sb.Clear();
                        count = 0;
                        continue;
                    }

                    sb.Append(',');
                }
            }

            sb.Clear();
            Log.Info("Inserted {total} sets", total);

            const string colorsInsertQuery =
                "INSERT INTO `clothing_colors` (id, `index`, club_level, selectable, color_value) VALUES ";
            foreach (var (id, palette) in data.Palettes)
            {
                foreach (var (_, color) in palette.Colors)
                {
                    sb.Append('(');
                    sb.Append(color.Id);
                    sb.Append(',');
                    sb.Append(color.Index);
                    sb.Append(',');
                    sb.Append(color.ClubLevel);
                    sb.Append(',');
                    sb.Append(color.Selectable);
                    sb.Append(',');
                    sb.Append(color.Value);
                    sb.Append(')');
                    count++;

                    if (count >= 100)
                    {
                        total += count;
                        sb.Append(';');
                        connection.Execute(colorsInsertQuery + sb);
                        sb.Clear();
                        count = 0;
                        continue;
                    }

                    sb.Append(',');
                }
            }

            sb.Clear();
            Log.Info("Inserted {total} colors", total);
/*
            const string palettesInsertQuery = "INSERT INTO `clothing_palettes` (id, color_id) VALUES ";
            foreach (var (id, palette) in data.Palettes)
            {
                foreach (var (_ color) in palette.Colors)
                {
                    sb.Append('(');
                    sb.Append(id);
                    sb.Append(',');
                    sb.Append(color.Id);
                    sb.Append(')');
                    count++;

                    if (count >= 100)
                    {
                        total += count;
                        sb.Append(';');
                        connection.Execute(palettesInsertQuery + sb);
                        sb.Clear();
                        count = 0;
                        continue;
                    }

                    sb.Append(',');
                }
            }
*/
            sb.Clear();
            Log.Info("Inserted {total} palettes", total);
        }
    }
}