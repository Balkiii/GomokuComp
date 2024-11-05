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
            }
        }

        private static void RenderGameScreen()
        {
            // Clear the board area only
            for (int y = boardStartY; y < boardStartY + game.BoardSize + 2; y++)
            {
                Console.SetCursorPosition(boardStartX, y);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            // Draw the board
            game.DisplayBoard(boardStartX, boardStartY);

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
