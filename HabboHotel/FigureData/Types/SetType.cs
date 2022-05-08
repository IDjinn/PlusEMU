using System;

namespace Plus.HabboHotel.FigureData.Types;

public enum SetType
{
    Hr,
    Hd,
    Ch,
    Lg,
    Sh,
    Ha,
    He,
    Ea,
    Fa,
    Ca,
    Wa,
    Cc,
    Cp,
    Hrb,
    Bd,
    Ey,
    Fc,
    Lh,
    Rh,
    Ls,
    Rs,
    Lc,
    Rc
}

public static class SetTypeExtensions
{
    public static string AsString(this SetType type) => type switch
    {
        SetType.Hr => "hr",
        SetType.Hd => "hd",
        SetType.Ch => "ch",
        SetType.Lg => "lg",
        SetType.Sh => "sh",
        SetType.Ha => "ha",
        SetType.He => "he",
        SetType.Ea => "ea",
        SetType.Fa => "fa",
        SetType.Ca => "ca",
        SetType.Wa => "wa",
        SetType.Cc => "cc",
        SetType.Cp => "cp",
        SetType.Hrb => "hrb",
        SetType.Bd => "bd",
        SetType.Ey => "ey",
        SetType.Fc => "fc",
        SetType.Lh => "lh",
        SetType.Rh => "rh",
        SetType.Ls => "ls",
        SetType.Rs => "rs",
        SetType.Lc => "lc",
        SetType.Rc => "rc",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown set type.")
        
    };
    
    public static SetType ParseFromString(string type) => type.ToLowerInvariant() switch
    {
        "hr" => SetType.Hr, 
        "hd" => SetType.Hd,
        "ch" => SetType.Ch,
        "lg" => SetType.Lg,
        "sh" => SetType.Sh,
        "ha" => SetType.Ha,
        "he" => SetType.He,
        "ea" => SetType.Ea,
        "fa" => SetType.Fa,
        "ca" => SetType.Ca,
        "wa" => SetType.Wa,
        "cc" => SetType.Cc,
        "cp" => SetType.Cp,
        "hrb" => SetType.Hrb,
        "bd" => SetType.Bd,
        "ey" => SetType.Ey,
        "fc" => SetType.Fc,
        "lh" => SetType.Lh,
        "rh" => SetType.Rh,
        "ls" => SetType.Ls,
        "rs" => SetType.Rs,
        "lc" => SetType.Lc,
        "rc" => SetType.Rc,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown set type.")
    };
}
