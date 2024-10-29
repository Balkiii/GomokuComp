using GomokuPackage;

public class Human : IBot
{
    public string Name => "HumanPlayer";

    public (int x, int y) MakeMove(int[,] board, int player)
    {
        // This method will be overridden by the GUI input.
        // You can return a default value or throw an exception here.
        throw new NotImplementedException("Human move should be provided by the GUI.");
    }
}
