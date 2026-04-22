using UnityEngine;

public interface ITargetable 
{
    Vector2Int GridPos { get; }
    bool IsValid { get; }
}
public class LocationTarget : ITargetable
{
    public Vector2Int GridPos { get; private set; }
    public bool IsValid => true;

    public LocationTarget(Vector2Int pos) { GridPos = pos; }
}