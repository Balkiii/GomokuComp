using GomokuPackage;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System;

namespace GomokuPackage;

public class HumanGame
{

    static bool exitRequested = false;
    static int gameTime = 100;
    static bool isHumanTurn;
    static int humanPlayerNumber;
    static int botPlayerNumber;
    static GomokuGame game;
    static IBot botPlayer;
    static string message = ""; // Store messages here
    static int boardStartX = 0; // X position where the board starts
    static int boardStartY = 0; // Y position where the board starts
    static List<(int x, int y)> winningPositions = new List<(int x, int y)>();

    public static void StartHumanGame(List<IBot> bots)
    {
        // Set up the console to receive mouse events
        IntPtr handleIn = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);

        // Enable mouse input mode
        const uint ENABLE_MOUSE_INPUT = 0x0010;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        NativeMethods.SetConsoleMode(handleIn, ENABLE_MOUSE_INPUT | ENABLE_EXTENDED_FLAGS);

        // Initialize game
        game = new GomokuGame(gameTime);
        botPlayer = bots.First();
        isHumanTurn = new Random().Next(2) == 0;

        if (isHumanTurn)
        {
            humanPlayerNumber = 1;
            botPlayerNumber = 2;
            message = "You start first!";
        }
        else
        {
            humanPlayerNumber = 2;
            botPlayerNumber = 1;
            message = "Bot starts first!";
        }

        // Render the initial game screen
        RenderGameScreen();

        // If the bot starts first, make the bot move now
        if (!isHumanTurn)
        {
            BotMove();
            // Flush any residual mouse events
            NativeMethods.FlushConsoleInputBuffer(handleIn);
            RenderGameScreen();
            isHumanTurn = true;
        }

        while (!exitRequested)
        {
            if (isHumanTurn)
            {
                // Human's turn: Wait for mouse input
                while (true)
                {
                    INPUT_RECORD[] record = new INPUT_RECORD[1];
                    uint numRead = 0;

                    bool success = NativeMethods.ReadConsoleInput(handleIn, record, 1, out numRead);

                    if (success && numRead > 0)
                    {
                        if (record[0].EventType == NativeMethods.MOUSE_EVENT)
                        {
                            var mouseEvent = record[0].MouseEvent;
                            if (mouseEvent.dwEventFlags == 0) // Mouse button pressed
                            {
                                if ((mouseEvent.dwButtonState & 0x0001) != 0) // Left button pressed
                                {
                                    int consoleX = mouseEvent.dwMousePosition.X;
                                    int consoleY = mouseEvent.dwMousePosition.Y;

                                    // Map console coordinates to board positions
                                    if (TryGetBoardPosition(consoleX, consoleY, out int x, out int y))
                                    {
                                        if (game.MakeMove(humanPlayerNumber, x, y))
                                        {
                                            message = "";
                                            if (game.CheckWinner() == humanPlayerNumber)
                                            {
                                                message = "Congratulations! You win!";
                                                winningPositions = FindWinningLine(game.GetBoard(), humanPlayerNumber);
                                                exitRequested = true;
                                            }
                                            else if (game.IsBoardFull())
                                            {
                                                message = "It's a draw!";
                                                exitRequested = true;
                                            }
                                            else
                                            {
                                                isHumanTurn = false;
                                            }
                                            RenderGameScreen();
                                            break; // Exit the inner while loop
                                        }
                                        else
                                        {
                                            message = "Invalid move. Please try again.";
                                            RenderGameScreen();
                                        }
                                    }
                                    else
                                    {
                                        // Click outside the board; ignore
                                        // Optionally, display a message
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Bot's turn: Make a move
                BotMove();
                // Flush any residual mouse events
                NativeMethods.FlushConsoleInputBuffer(handleIn);
                if (exitRequested)
                    break;
                isHumanTurn = true;
                RenderGameScreen();
            }
        }

        // Clean up and exit
        Console.CursorVisible = true;
        RenderGameScreen();
        Console.SetCursorPosition(0, boardStartY + game.BoardSize + 3);
        Console.WriteLine("Game over. Press any key to exit.");
        Console.ReadKey();
    }

    private static void BotMove()
    {
        message = "Bot is thinking...";
        RenderGameScreen();
        game.sw.Restart();
        var move = botPlayer.MakeMove(game.GetBoard(), botPlayerNumber);
        game.sw.Stop();

        if (game.sw.ElapsedMilliseconds > gameTime)
        {
            message = "Bot ran out of time to think. You win by default!";
            exitRequested = true;
            return;
        }

        if (game.MakeMove(botPlayerNumber, move.x, move.y))
        {
            if (game.CheckWinner() == botPlayerNumber)
            {
                message = "Bot wins! Better luck next time.";
                winningPositions = FindWinningLine(game.GetBoard(), botPlayerNumber);
                exitRequested = true;
            }
            else if (game.IsBoardFull())
            {
                message = "It's a draw!";
                exitRequested = true;
            }
            else
            {
                message = "";
            }
        }
        else
        {
            message = "Bot made an invalid move. You win by default!";
            exitRequested = true;
            return;
        }
    }

    private static void DisplayBoardWithHighlight(int[,] board, List<(int x, int y)> highlightPositions, int startX, int startY)
    {
        // Column headers
        Console.SetCursorPosition(startX, startY);
        Console.Write("   "); // Spaces to align with row numbers
        for (int i = 0; i < game.BoardSize; i++)
        {
            Console.Write(" " + (char)('A' + i) + " ");
        }
        Console.WriteLine();

        for (int x = 0; x < game.BoardSize; x++)
        {
            // Move the cursor to the correct position
            Console.SetCursorPosition(startX, startY + x + 1);

            // Row numbers
            Console.Write((x + 1).ToString("D2") + " ");
            for (int y = 0; y < game.BoardSize; y++)
            {
                char symbol = board[x, y] switch
                {
                    1 => 'X',
                    2 => 'O',
                    _ => '.'
                };

                if (highlightPositions.Contains((x, y)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" " + symbol + " ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(" " + symbol + " ");
                }
            }
        }
    }


    private static List<(int x, int y)> FindWinningLine(int[,] board, int playerNumber)
    {
        List<(int x, int y)> winningLine = new List<(int x, int y)>();

        for (int x = 0; x < game.BoardSize; x++)
        {
            for (int y = 0; y < game.BoardSize; y++)
            {
                if (board[x, y] != playerNumber)
                    continue;

                // Check in all four directions
                // Right
                if (y + 4 < game.BoardSize &&
                    Enumerable.Range(0, 5).All(i => board[x, y + i] == playerNumber))
                {
                    for (int i = 0; i < 5; i++)
                        winningLine.Add((x, y + i));
                    return winningLine;
                }

                // Down
                if (x + 4 < game.BoardSize &&
                    Enumerable.Range(0, 5).All(i => board[x + i, y] == playerNumber))
                {
                    for (int i = 0; i < 5; i++)
                        winningLine.Add((x + i, y));
                    return winningLine;
                }

                // Diagonal down-right
                if (x + 4 < game.BoardSize && y + 4 < game.BoardSize &&
                    Enumerable.Range(0, 5).All(i => board[x + i, y + i] == playerNumber))
                {
                    for (int i = 0; i < 5; i++)
                        winningLine.Add((x + i, y + i));
                    return winningLine;
                }

                // Diagonal up-right
                if (x - 4 >= 0 && y + 4 < game.BoardSize &&
                    Enumerable.Range(0, 5).All(i => board[x - i, y + i] == playerNumber))
                {
                    for (int i = 0; i < 5; i++)
                        winningLine.Add((x - i, y + i));
                    return winningLine;
                }
            }
        }

        return winningLine; // Empty list if no winning line found
    }


    private static void RenderGameScreen()
    {
        // Clear the board area only
        for (int y = boardStartY; y < boardStartY + game.BoardSize + 2; y++)
        {
            Console.SetCursorPosition(boardStartX, y);
            Console.Write(new string(' ', Console.WindowWidth));
        }

        // Draw the board with possible highlighting
        DisplayBoardWithHighlight(game.GetBoard(), winningPositions, boardStartX, boardStartY);

        // Write the message below the board
        int messageLine = boardStartY + game.BoardSize + 2;
        Console.SetCursorPosition(0, messageLine);
        Console.Write(new string(' ', Console.WindowWidth)); // Clear previous message
        Console.SetCursorPosition(0, messageLine);
        Console.WriteLine(message);
    }


    private static bool TryGetBoardPosition(int consoleX, int consoleY, out int x, out int y)
    {
        x = -1;
        y = -1;

        // Calculate relative positions
        int relX = consoleX - (boardStartX + 3); // +3 to account for row numbers and spaces
        int relY = consoleY - (boardStartY + 1); // +1 to account for column headers

        // Each cell is 3 characters wide (" X ")
        int cellWidth = 3;
        int cellHeight = 1;

        if (relX < 0 || relY < 0)
            return false;

        y = relX / cellWidth;
        x = relY / cellHeight;

        if (x >= 0 && x < game.BoardSize && y >= 0 && y < game.BoardSize)
        {
            return true;
        }
        return false;
    }
}

[StructLayout(LayoutKind.Explicit)]
struct INPUT_RECORD
{
    [FieldOffset(0)]
    public ushort EventType;
    [FieldOffset(4)]
    public KEY_EVENT_RECORD KeyEvent;
    [FieldOffset(4)]
    public MOUSE_EVENT_RECORD MouseEvent;
}

[StructLayout(LayoutKind.Sequential)]
struct KEY_EVENT_RECORD
{
    public bool bKeyDown;
    public ushort wRepeatCount;
    public ushort wVirtualKeyCode;
    public ushort wVirtualScanCode;
    public char UnicodeChar;
    public uint dwControlKeyState;
}

[StructLayout(LayoutKind.Sequential)]
struct MOUSE_EVENT_RECORD
{
    public COORD dwMousePosition;
    public uint dwButtonState;
    public uint dwControlKeyState;
    public uint dwEventFlags;
}

[StructLayout(LayoutKind.Sequential)]
struct COORD
{
    public short X;
    public short Y;
}

class NativeMethods
{
    public const int STD_INPUT_HANDLE = -10;
    public const ushort KEY_EVENT = 0x0001;
    public const ushort MOUSE_EVENT = 0x0002;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadConsoleInput(
        IntPtr hConsoleInput,
        [Out] INPUT_RECORD[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);
}
