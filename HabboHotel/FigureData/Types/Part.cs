namespace Plus.HabboHotel.FigureData.Types;

public class Part
{
    public Part(int id, SetType setType, bool colorable, int index, int colorIndex)
    {
        Id = id;
        SetType = setType;
        Colorable = colorable;
        Index = index;
        ColorIndex = colorIndex;
    }

    public int Id { get; }
    public SetType SetType { get; }
    public bool Colorable { get; }
    public int Index { get; }
    public int ColorIndex { get; }
}