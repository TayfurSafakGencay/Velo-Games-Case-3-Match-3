public class RainbowPiece : ClearablePiece
{
    public ColorPiece.ColorType Color { get; set; }

    public override void Clear()
    {
        base.Clear();
        
        _piece.BoardRef.ClearColor(Color);
    }
}
