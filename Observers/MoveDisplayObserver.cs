public class MoveDisplayObserver : IMoveObserver
{
    public void OnMoveMade(MoveRecord move)
    {
        Console.WriteLine($"Move {move.MoveNumber}: {move.Player} moved {move.Piece} from ({move.FromCol}, {move.FromRow}) to ({move.ToCol}, {move.ToRow})");
    }
}

