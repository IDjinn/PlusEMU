namespace Plus.HabboHotel.Avatar;

public enum ClothingGender
{
    None,
    
    Unisex,
    Male,
    Female
}

public static class ClothingGenderExtensions
{
    public static ClothingGender FromString(this string gender) => gender switch
    {
        "U" or "u" => ClothingGender.Unisex,
        "M" or "m" => ClothingGender.Male,
        "F" or "f" => ClothingGender.Female,
        _ => ClothingGender.None
    };

    public static string ToString(this ClothingGender clothingGender) => clothingGender switch
    {
        ClothingGender.Female => "F",
        ClothingGender.Male => "M",
        ClothingGender.Unisex => "U",
        _ => string.Empty
    };
}
