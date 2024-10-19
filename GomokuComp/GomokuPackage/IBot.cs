namespace GomokuPackage;

public interface IBot
{
    string Name { get; }
    (int x, int y) MakeMove(int[,] board, int player);
}