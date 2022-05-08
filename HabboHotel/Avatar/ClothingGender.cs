using System;

namespace Plus.HabboHotel.Avatar;

public enum ClothingGender
{
    Unisex,
    Male,
    Female
}

public static class ClothingGenderExtensions
{
    public static ClothingGender ParseFromString(string gender) => gender switch
    {
        "U" or "u" => ClothingGender.Unisex,
        "M" or "m" => ClothingGender.Male,
        "F" or "f" => ClothingGender.Female,
        _ => throw new ArgumentException("Invalid ClothingGender type.")
    };

    public static string ToString(this ClothingGender clothingGender) => clothingGender switch
    {
        ClothingGender.Female => "f",
        ClothingGender.Male => "m",
        _ => "u",
    };
}
