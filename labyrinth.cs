/*
	Author: Jan Holy
	Date: 28. 9. 2025
*/
using System;

public class Maze
{
    private char[,] mazeGrid;
    private int mazeWidth, mazeHeight;
		private bool alreadyRotated; // pro pouze jednu rotaci prisery

    private int beastX, beastY;     // pozice prisery
    private int beastDirection;     // smer: 0 = nahoru, 1 = doprava, 2 = dolu, 3 = doleva

    // Posuny pro jednotlive smery (nahoru, doprava, dolu, doleva)
    private readonly int[] deltaX = { 0, 1, 0, -1 };
    private readonly int[] deltaY = { -1, 0, 1, 0 };

    // Znakove reprezentace smeru
    private readonly char[] directionSymbols = { '^', '>', 'v', '<' };

    public void LoadFromInput()
    {
        mazeWidth = int.Parse(Console.ReadLine());
        mazeHeight = int.Parse(Console.ReadLine());
        mazeGrid = new char[mazeHeight, mazeWidth];

        for (int y = 0; y < mazeHeight; y++)
        {
            string line = Console.ReadLine();
            for (int x = 0; x < mazeWidth; x++)
            {
                char cell = line[x];
                mazeGrid[y, x] = cell;

                // Najdi pocatecni pozici prisery a jeji smer
                for (int direction = 0; direction < 4; direction++) // predejiti if kaskade a switch kaskade
                {
                    if (cell == directionSymbols[direction])
                    {
                        beastX = x;
                        beastY = y;
                        beastDirection = direction;
                        mazeGrid[y, x] = '.'; // Odstran priseru z mapy
                    }
                }
            }
        }
    }

    public void Simulate(int numberOfSteps)
    {
        for (int step = 1; step <= numberOfSteps; step++)
        {
            Console.WriteLine(step + ". krok"); // formalni stranka
            PerformStep();
            PrintMaze();
        }
    }

    private void PerformStep()
    {
        int rightDirection = (beastDirection + 1) % 4; // ^ - 0, > - 1, v - 2, < - 3, cyklicky
        int rightX = beastX + deltaX[rightDirection];
        int rightY = beastY + deltaY[rightDirection];


				// 1. Pokud je po prave strane volno, otoc se doprava
				if (IsCellFree(rightX, rightY) && alreadyRotated == false)
				{
					alreadyRotated = true; // uz se jednou otocila
					beastDirection = rightDirection;
					return;
				}

        // 2. Pokud je pred priserou volno, jdi dopredu
        int frontX = beastX + deltaX[beastDirection];
        int frontY = beastY + deltaY[beastDirection];
        if (IsCellFree(frontX, frontY))
        {
					alreadyRotated = false;
          beastX = frontX;
          beastY = frontY;
          return;
        }


        // 3. Pokud je vlevo volno, otoc se doleva
        int leftDirection = (beastDirection + 3) % 4;
        int leftX = beastX + deltaX[leftDirection];
        int leftY = beastY + deltaY[leftDirection];
        if (IsCellFree(leftX, leftY))
        {
            beastDirection = leftDirection;
            return;
        }

        // 4. Jinak otoc se doprava
        beastDirection = rightDirection;
    }

		// bool pro identifikaci prazdnosti :)
    private bool IsCellFree(int x, int y)
    {
        return x >= 0 && x < mazeWidth &&
               y >= 0 && y < mazeHeight &&
               mazeGrid[y, x] == '.';
    }

		// vypis labyrintu
    private void PrintMaze()
    {
        for (int y = 0; y < mazeHeight; y++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                if (x == beastX && y == beastY)
                    Console.Write(directionSymbols[beastDirection]);
                else
                    Console.Write(mazeGrid[y, x]);
            }
            Console.WriteLine();
        }
        Console.WriteLine(); // prazdny radek mezi kroky
    }

    public static void Main()
    {
        Maze maze = new Maze();
        maze.LoadFromInput(); // nacteni labyrintu
				Console.WriteLine(); // mezera mezi vystupem a vstupem
        maze.Simulate(20); // test
    }
}
