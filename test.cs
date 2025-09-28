using System;
using System.Collections.Generic;

namespace RightHandMaze
{
    // --- Směry pohledu příšery v pevném pořadí (kvůli otáčení mod 4) ---
    enum Direction { Up = 0, Right = 1, Down = 2, Left = 3 }

    // --- Jednoduchý bod (kompatibilní s C#6 / mcs) ---
    struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x; Y = y;
        }

        // Posun o 1 políčko ve směru d
        public Point Move(Direction d)
        {
            switch (d)
            {
                case Direction.Up:    return new Point(X, Y - 1);
                case Direction.Right: return new Point(X + 1, Y);
                case Direction.Down:  return new Point(X, Y + 1);
                case Direction.Left:  return new Point(X - 1, Y);
                default:              return this;
            }
        }
    }

    // --- Reprezentace bludiště ---
    sealed class Maze
    {
        private readonly char[,] _cells; // 'X' = zeď, '.' = volno
        public int Width  { get; private set; }
        public int Height { get; private set; }

        public Maze(int width, int height, char[,] cells)
        {
            Width = width;
            Height = height;
            _cells = cells;
        }

        /// <summary>
        /// Načte bludiště ze stdin.
        /// Vstup: nejprve šířka, výška; poté Height řádků mapy.
        /// Mapa: 'X' zeď, '.' volno, '^','>','v','<' = start příšery (pole je volné).
        /// </summary>
        public static Maze FromInput(out Point monsterPos, out Direction monsterDir)
        {
            monsterPos = new Point(0, 0);
            monsterDir = Direction.Right;

            // 1) robustní načtení šířky a výšky (mohou být na jednom řádku)
            var tokens = new Queue<string>();
            while (tokens.Count < 2)
            {
                string line = Console.ReadLine();
                if (line == null) throw new InvalidOperationException("Neúplný vstup (chybí šířka/výška).");
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++) tokens.Enqueue(parts[i]);
            }
            int width  = int.Parse(tokens.Dequeue());
            int height = int.Parse(tokens.Dequeue());

            // 2) načtení mapy a nalezení startu
            var cells = new char[height, width];

            for (int y = 0; y < height; y++)
            {
                string line = Console.ReadLine();
                if (line == null) throw new InvalidOperationException("Chybí řádek mapy.");
                if (line.Length < width)
                    throw new InvalidOperationException("Řádek mapy je kratší než zadaná šířka.");

                for (int x = 0; x < width; x++)
                {
                    char ch = line[x];
                    if (ch == 'X' || ch == '.')
                    {
                        cells[y, x] = ch;
                    }
                    else if (ch == '^' || ch == '>' || ch == 'v' || ch == '<')
                    {
                        // Ulož start a směr; do mřížky dáme volno
                        monsterPos = new Point(x, y);
                        switch (ch)
                        {
                            case '^': monsterDir = Direction.Up; break;
                            case '>': monsterDir = Direction.Right; break;
                            case 'v': monsterDir = Direction.Down; break;
                            case '<': monsterDir = Direction.Left; break;
                        }
                        cells[y, x] = '.';
                    }
                    else
                    {
                        throw new InvalidOperationException("Neznámý znak mapy: " + ch);
                    }
                }
            }

            return new Maze(width, height, cells);
        }

        // true = zeď nebo mimo mapu (mimo mapu bereme jako zeď)
        public bool IsWall(Point p)
        {
            if (p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height)
                return true;
            return _cells[p.Y, p.X] == 'X';
        }

        public bool IsFree(Point p) { return !IsWall(p); }

        // Vytiskne mapu s příšerou ve stejném tvaru jako vstup (včetně symbolu směru)
        public void PrintWithMonster(Point monsterPos, Direction monsterDir)
        {
            for (int y = 0; y < Height; y++)
            {
                var line = new char[Width];
                for (int x = 0; x < Width; x++)
                    line[x] = _cells[y, x];

                if (y == monsterPos.Y)
                    line[monsterPos.X] = DirToChar(monsterDir);

                Console.WriteLine(new string(line));
            }
            Console.WriteLine(); // prázdný řádek po každém výpisu
        }

        private static char DirToChar(Direction d)
        {
            switch (d)
            {
                case Direction.Up:    return '^';
                case Direction.Right: return '>';
                case Direction.Down:  return 'v';
                case Direction.Left:  return '<';
                default:              return '?';
            }
        }
    }

    // --- Stav a logika pohybu příšery ---
    sealed class Monster
    {
        public Point Position { get; private set; }   // souřadnice (x,y)
        public Direction Facing { get; private set; } // kam je otočená

        public Monster(Point start, Direction facing)
        {
            Position = start;
            Facing = facing;
        }
				// 1) je-li vpravo volno -> otoč vpravo A hned krokuj,
				// 2) jinak je-li vpředu volno -> krokuj,
				// 3) jinak je-li vlevo volno -> otoč vlevo (bez kroku),
				// 4) jinak -> otoč o 180° (bez kroku).
				public void StepRightHand(Maze maze)
				{
    			Direction rightDir = TurnRight(Facing);
    			Direction leftDir  = TurnLeft(Facing);
					
					// Zda jsou sousední buňky volné (relativně ke Facing)
    			bool rightFree = maze.IsFree(Position.Move(rightDir)); // buňka "po pravici"
    			bool aheadFree = maze.IsFree(Position.Move(Facing));   // buňka před příšerou
    			bool leftFree  = maze.IsFree(Position.Move(leftDir));  // buňka "po levici"

    			// 1) pravá ruka má přednost: když je vpravo volno, jen se otoč doprava
    			if (rightFree)
    			{
        		Facing = rightDir;
        		return; // žádný posun v tomto tahu
    			}

    			// 2) když vpředu volno, jdi vpřed (jen posun, žádná otočka)
    			if (aheadFree)
    			{
        		Position = Position.Move(Facing);
        		return;
    			}

    			// 3) když vlevo volno, jen se otoč doleva
    			if (leftFree)
    			{
        		Facing = leftDir;
        		return;
    			}

    			Facing = TurnRight(TurnRight(Facing));
				}

        // Pomocné rotace směru (mod 4)
        private static Direction TurnRight(Direction d) { return (Direction)(((int)d + 1) & 3); }
        private static Direction TurnLeft(Direction d)  { return (Direction)(((int)d + 3) & 3); }
    }

    // --- Hlavní program: načti, vypiš počáteční stav, simuluj 20 kroků s číslováním ---
    static class Program
    {
        static void Main()
        {
            // Načtení bludiště a startovní pozice/směru
            Point startPos;
            Direction startDir;
            Maze maze = Maze.FromInput(out startPos, out startDir);
            var monster = new Monster(startPos, startDir);

            // 0) vypiš šířku a výšku, pak počáteční mapu (přesně jak chceš)
            Console.WriteLine(maze.Width);
            Console.WriteLine(maze.Height);
            maze.PrintWithMonster(monster.Position, monster.Facing);

            // 1..20) simulace kroků: nejdřív hlavička "N. krok", pak mapa + prázdný řádek
            for (int i = 1; i <= 20; i++)
            {
                Console.WriteLine(i + ". krok");
                monster.StepRightHand(maze);
                maze.PrintWithMonster(monster.Position, monster.Facing);
            }
        }
    }
}

