using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

This program

namespace Computationele_Intelligentie_P2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load individual mode:
            // parameter 0: CBT or FW; 1: MCV or not?;
            // 2: Only relevant when not using AskInput, which uses an interactive window;
            //Sudoku sud = new Sudoku("CBT", true, 0);

            
            // Analytics mode:
            // List of timers per algorithm.
            List<long> CBT = new List<long>();
            List<long> CBT_MCV = new List<long>();
            List<long> FW = new List<long>();
            List<long> FW_MCV = new List<long>();

            //For every sudoku, time it given the algorithm.
            for (int s = 0; s < 10; s++)
            {
                CBT.Add(new Sudoku("CBT", false, s).solve_time);
                CBT_MCV.Add(new Sudoku("CBT", true, s).solve_time);
                FW.Add(new Sudoku("FW", false, s).solve_time);
                FW_MCV.Add(new Sudoku("FW", true, s).solve_time);
            }

            double avg_CBT = CBT.Average();
            long min_CBT = CBT.Min();
            long max_CBT = CBT.Max();
            double avg_CBT_MCV = CBT_MCV.Average();
            long min_CBT_MCV = CBT_MCV.Min();
            long max_CBT_MCV = CBT_MCV.Max();
            double avg_FW = FW.Average();
            long min_FW = FW.Min();
            long max_FW = FW.Max();
            double avg_FW_MCV = FW_MCV.Average();
            long min_FW_MCV = FW_MCV.Min();
            long max_FW_MCV = FW_MCV.Max();

            Console.WriteLine("CBT: " + " avg: " + avg_CBT + " minimum: " + min_CBT + " max: " + max_CBT);
            Console.WriteLine("CBT_MCV: " + " avg: " + avg_CBT_MCV + " mininimum: " + min_CBT_MCV + " max: " + max_CBT_MCV);
            Console.WriteLine("FW: " + " avg: " + avg_FW + " minimum: " + min_FW + " max: " + max_FW);
            Console.WriteLine("FW_MCV: " + " avg: " + avg_FW_MCV + " mininmum: " + min_FW_MCV + " max: " + max_FW_MCV);
            
        }
        
    }

    class Cell
    {
        public int value;
        public int x;
        public int y;
        public bool anchored;
        public List<int> domain;

        public Cell(int value, int x, int y, bool anchored)
        {
            this.value = value;
            this.x = x;
            this.y = y;
            this.anchored = anchored;

            // If anchored: Domain size will be 1, its own value;
            if (this.anchored)
            {
                this.domain = new List<int>() { this.value };
            }
            else
            {
                this.domain = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
        }

        // Constructor for deep-copying domains.
        public Cell(int value, int x, int y, bool anchored, List<int> domain)
        {
            this.value = value;
            this.x = x;
            this.y = y;
            this.anchored = anchored;

            // If anchored: Domain size will be 1, its own value;
            if (this.anchored)
            {
                this.domain = new List<int>() { this.value };
            }
            else
            {
                this.domain = domain;
            }
        }
    }

    class Sudoku
    {
        // The original board's initial test sudoku, overwritten as different sudoku's are chosen.
        int[,] originalboard = new int[,]
                {
                                            {0,0,0,0,0,0,0,0,0 },
                                            {0,0,0,0,0,3,0,8,5 },
                                            {0,0,1,0,2,0,0,0,0 },
                                            {0,0,0,5,0,7,0,0,0 },
                                            {0,0,4,0,0,0,1,0,0 },
                                            {0,9,0,0,0,0,0,0,0 },
                                            {5,0,0,0,0,0,0,7,3 },
                                            {0,0,2,0,1,0,0,0,0 },
                                            {0,0,0,0,4,0,0,0,9 }
                };


        Cell[,] cells = new Cell[9, 9];
        List<Cell> mcvList; // List with all cells sorted by domain size statically.
        public bool solved = false; // True when the solver has ended and the sudoku is solved.
        string algorithm; // algorithm in use.
        bool MCV; // Use of Most Constrained Variable heuristic (or not).
        public long solve_time; // Timespan of current algorithm.
        Stopwatch timer = new Stopwatch();

        public Sudoku(string algorithm, bool MCV, int sudokuNumber)
        {
                timer.Start();
                // Remove parameter of Initialize and follow instructions in method for disabling analytics mode.
                Initialize(sudokuNumber);

                InitializeCells();  // Prepares domains and cells for the original board.
                this.algorithm = algorithm;
                this.MCV = MCV;

                
                if (algorithm == "CBT")
                {
                    // Chronological Backtracking with no additional heuristic
                    if (!MCV)
                    {
                        Cell[,] tempcell = (Cell[,])cells.Clone();
                        cells = ChronoBackTracking(tempcell, 0, 0);
                    }
                    // Chronological Backtracking using Most Constrained Variable heuristic
                    else MCV_BCT_Init();

                }
                if (algorithm == "FW")
                {
                    // Forward Checking with no additional heuristic
                    if (!MCV)
                    {
                        ForwardChecking(cells, 0);
                    }
                    // Forward Checking using Most Constrained Variable heuristic
                    else MCV_FWDChecking_init();
                }
                timer.Stop();
                solve_time = timer.ElapsedMilliseconds;

            // Print the solved sudoku.
            // PrintBoard(null);
            
            // See summary "CheckEmpty"
            //CheckEmpty();

            // Main debug function: See summary "Help"
            // Help();

        }


        public void InitializeCells()
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    cells[x, y] = new Cell(originalboard[y, x], x, y, !originalboard[y, x].Equals(0));
                }
            }
            // Now the cells are created, and we can take care of their initial domain!
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    // Remove anchored values from the domain of their column's and row's cells
                    Cell c = cells[x, y];
                    if (c.anchored)
                    {

                        // Row domains
                        for (int i = 0; i < 9; i++)
                        {
                            Cell rowCell = cells[i, y];
                            // For all cells except theirselves:
                            if (i!=c.x)
                            {
                                rowCell.domain.Remove(c.value);
                            }
                            // For anchored cells: already initiated domain
                        }

                        // Column domains 
                        for (int j = 0; j < 9; j++)
                        {
                            Cell columnCell = cells[x, j];
                            // For all cells except theirselves:
                            if (j!=c.y)
                            {
                                columnCell.domain.Remove(c.value);
                            }
                            // For anchored cells: already initiated 
                        }

                        // // Check whether square constraints have been violated...
                        int sqX = c.x / 3;
                        int sqY = c.y / 3;
                        for (int i = sqX * 3; i < sqX * 3 + 3; i++)
                        {
                            for (int j = sqY * 3; j < sqY * 3 + 3; j++)
                            {
                                Cell squareCell = cells[i, j];
                                // ... defined by same value within square, but not same location.
                                if (!((squareCell.x ==c.x)&&(squareCell.y==c.y)))
                                {
                                    squareCell.domain.Remove(c.value);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        ///  Chronological Backtracking: For each non-fixed number, attempt to assign values 1 to 9, and assign
        ///  when no constraints are violated. If no number satisfies this, go back one cell and try a different number etc.
        ///  until the sudoku is completed.
        /// </summary>
        /// <param name="partSol"> Given an layer in the recursion, this is the partial solution</param>
        /// <param name="iteration"> The current iteration (purely for debugging)</param>
        /// <returns></returns>
        Cell[,] ChronoBackTracking(Cell[,] partSol, int cell, int iteration)
        {
            //Console.WriteLine("Iteration: " + iteration + ". Curr cell: " + c + " : " + r + ". Previous sudoku:");

            // Definition of cell's column
            int c = cell % 9;
            // Definition of cell's row 
            int r = cell / 9;

            // As long as the game is not finished, meaning every cell does not yet satisfy the constraints of a sudoku:
            try
            {
                // Use CBT to fill in every empty cell.
                if (!partSol[c, r].anchored)
                {
                    for (int i = 1; i < 10; i++)
                    {

                        // Attempt a value before assigning c.
                        if (ConstraintsViolated(partSol, i, c, r) == false)
                        {
                            partSol[c, r].value = i;
                            ChronoBackTracking(partSol, cell + 1, iteration + 1);
                        }
                    }
                    // If not ended and back from recursion: assign 0, not to constrain left neighbors.
                    if (!solved)
                    {
                        partSol[c, r].value = 0;
                    }
                }
                else
                {
                    // If fixed: Try next cell.
                    ChronoBackTracking(partSol, cell + 1, iteration + 1);
                }
            }
            // Definition of end-of-game: Out of board's range. 
            // Every cell has passed on the recursion and does not violate constraints.
            catch (IndexOutOfRangeException)
            {
                solved = true;
                return partSol;
            }

            // Return current partial solution.
            return partSol;
        }

        /// <summary>
        /// Before the MCV CBT can be called, the 2D array with the cells has to be converted to a list, which is sorted on domainsize. Afterwards MCV CBT is called.
        /// </summary>
        void MCV_BCT_Init()
        {
            mcvList = cells.Cast<Cell>().ToList();
            List<Cell> realMCV = new List<Cell>();
            foreach (Cell c in mcvList)

            {
                // It is redundant to iterate over anchored cells, so delete them.
                if (!c.anchored)
                    realMCV.Add(c);

            }
            mcvList = realMCV;

            mcvList.Sort((a, b) => a.domain.Count - b.domain.Count);

            Cell[,] tempcells = (Cell[,])cells.Clone();
            cells = MCVChronoBackTracking(tempcells, 0, 0);
        }

        /// <summary>
        ///  Chronological Backtracking with MCV: For each non-fixed number, attempt to assign values 1 to 9, and assign
        ///  when no constraints are violated. If no number satisfies this, go back one cell and try a different number etc.
        ///  until the sudoku is completed. The order of expansion is determinated by the size of the domain. From small to large [1, 9].
        /// </summary>
        /// <param name="partSol"> Given an layer in the recursion, this is the partial solution</param>
        /// <param name="cell"> The current cell </param>
        /// <param name="iteration"> The current iteration (purely for debugging)</param>
        /// <returns></returns>
        Cell[,] MCVChronoBackTracking(Cell[,] partSol, int cell, int iteration)
        {
            // As long as the game is not finished, meaning every cell does not yet satisfy the constraints of a sudoku:
            try
            {
                // Get the current cell from the mcv list. So when "cell" is increased you iterate over the list.
                Cell currentCell = mcvList[cell];
                // Get the current collumn, so the code is the same as cbt
                int c = currentCell.x;
                // Get the current row, so the code is the same as cbt
                int r = currentCell.y;

                // Use CBT to fill in every empty cell.
                // the catch indicates a solved sudoku.
                if (!partSol[c, r].anchored)
                {
                    for (int i = 1; i < 10; i++)
                    {

                        // Attempt a value before assigning c
                        if (ConstraintsViolated(partSol, i, c, r) == false)
                        {
                            partSol[c, r].value = i;
                            MCVChronoBackTracking(partSol, cell + 1, iteration + 1);
                        }
                    }
                    // If not ended and back from recursion: assign 0, not to constrain left neighbors.
                    if (!solved)
                    {
                        partSol[c, r].value = 0;
                    }
                }
                else
                {
                    // If fixed: Try next cell.
                    MCVChronoBackTracking(partSol, cell + 1, iteration + 1);
                }
            }
            // Definition of end-of-game: Out of board's range. 
            // Every cell has passed on the recursion and does not violate constraints.
            catch (ArgumentOutOfRangeException)
            {
                //PrintBoard(null);
                solved = true;
                return partSol;
            }

            // Return current partial solution.
            return partSol;
        }

        bool ForwardChecking(Cell[,] partSol, int cell)
        {
            // Definition of cell's column
            int c = cell % 9;
            // Definition of cell's row 
            int r = cell / 9;
            Cell[,] temp = CloneArray(partSol);

            // Use Forward Checking to fill in every empty cell.
            // The catch indicates a solved sudoku.
            try
            {
                if (!temp[c, r].anchored)
                {
                    // Domain only shrinks: Count limit will not be reached before end of sudoku.
                    for (int i = 0; i < temp[c, r].domain.Count; i++)
                    {
                        // Deep-copy the current state, for reversal after trying a domain value.
                        temp = CloneArray(partSol);

                        // Attempt a value before assigning c, returning bool solved?
                        // With either the updated, or the old state of the board.
                        Tuple<bool, Cell[,]> tupleResult = DomainViolated(temp, temp[c, r].domain[i], c, r);

                        if (tupleResult.Item1 == false)
                        {
                            // No constraints violated! Adjust to new state.
                            temp = tupleResult.Item2;
                            temp[c, r].value = temp[c, r].domain[i];
                            solved = ForwardChecking(temp, cell + 1);

                            if (solved)
                                return true;
                        }
                        // Restore to old state, with new value attempt.
                        if (!solved)
                            temp[c, r].value = 0;
                    }
                }
                else
                {
                    // If fixed: Try next cell.
                    ForwardChecking(temp, cell + 1);
                    if (solved)
                        return true;
                }
                return false;
            }
            // Definition of end-of-game: Out of board's range. 
            // Every cell has passed on the recursion and does not violate constraints.
            catch (IndexOutOfRangeException)
            {
                solved = true;
                return true;
            }
        }

        void MCV_FWDChecking_init()
        {
            List<Cell> mcv_fwdList;
            mcv_fwdList = CloneArray(cells).Cast<Cell>().ToList();
            List<Cell> realMCV = new List<Cell>();
                 
            foreach (Cell c in mcv_fwdList)
                
            {
                // It is redundant to iterate over anchored cells, so delete them.
                if (!c.anchored)
                    realMCV.Add(c);

            }
            mcv_fwdList = realMCV;
            mcv_fwdList.Sort((a, b) => a.domain.Count - b.domain.Count);

            MCVForwardChecking(CloneArray(cells), mcv_fwdList);
        }

        bool MCVForwardChecking(Cell[,] partSol, List<Cell> local_mcv)
        {
            
            // Sort the mcv list, to have a dynamic heuristic.
            // Sort by their domain count in partSol, since mcvList contains static domains
            local_mcv.Sort((a, b) => partSol[a.x,a.y].domain.Count - partSol[b.x, b.y].domain.Count);
            // Get the current cell from the mcv list. So when "cell" is increased you iterate over the list.
            Cell currentCell = local_mcv[0];
            // Get the current collumn, so the code is the same as fwd
            int c = currentCell.x;
            // Get the current row, so the code is the same as fwd
            int r = currentCell.y;

            Cell[,] temp = CloneArray(partSol);

            // Use CBT to fill in every empty cell.
            try
            {
                // If a cell is not anchored, which by definition should be true:
                if (!temp[c, r].anchored)
                {
                    // Domain only shrinks, so every item will be tried.
                    // No index error due to domain always containing the solution.
                    for (int i = 0; i < temp[c, r].domain.Count; i++)
                    {
                        // Deep-copy the current state, for reversal after trying a domain value.
                        temp = CloneArray(partSol);
                        // Attempt a value before assigning c, returning bool solved?
                        // With either the updated, or the old state of the board.
                        Tuple<bool, Cell[,]> tupleResult = DomainViolated(temp, temp[c, r].domain[i], c, r);

                        if (tupleResult.Item1 == false)
                        {
                            temp = tupleResult.Item2;
                            temp[c, r].value = temp[c, r].domain[i];
                            local_mcv.Remove(currentCell);
                            solved = MCVForwardChecking(temp, new List<Cell>(local_mcv));

                            if (solved)
                                return true;
                        }
                        // Restore old state (along with the deep-copy)
                        if (!solved)
                            temp[c, r].value = 0;
                    }
                }
                else
                {
                    // FW with MCV does not cover fixed numbers.
                }
                return false;
            }
            // Definition of end-of-game: Out of board's range. 
            // Every cell has passed on the recursion and does not violate constraints.
            catch (ArgumentOutOfRangeException)
            {
                solved = true;
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempcell"></param>
        /// <param name="tempval"></param>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        bool ConstraintsViolated(Cell[,] tempcell, int tempval, int c, int r)
        {
            // Temporarily swap with try-out value, but save old value in case of violation.
            Cell cell = tempcell[c, r];
            int oldvalue = cell.value;
            cell.value = tempval;

            // Check whether row constraints have been violated...
            for (int i = 0; i < 9; i++)
            {
                Cell rowCell = tempcell[i, cell.y];
                // ... defined by same value within row, but not same location.
                if ((rowCell.value == cell.value) && (i != cell.x))
                {
                    // Constraint violated, swap back to old value.
                    cell.value = oldvalue;
                    return true;
                }
            }

            // Check whether column constraints have been violated...
            for (int j = 0; j < 9; j++)
            {
                Cell columnCell = tempcell[cell.x, j];
                // ... defined by same value within column, but not same location.
                if ((columnCell.value == cell.value) && (j != cell.y))
                {
                    // Constraint violated, swap back to old value.
                    cell.value = oldvalue;
                    return true;
                }
            }

            // // Check whether square constraints have been violated...
            int sqX = cell.x / 3;
            int sqY = cell.y / 3;
            for (int i = sqX * 3; i < sqX * 3 + 3; i++)
            {
                for (int j = sqY * 3; j < sqY * 3 + 3; j++)
                {
                    Cell squareCell = tempcell[i, j];
                    // ... defined by same value within square, but not same location.
                    if ((squareCell.value == cell.value) && !((i == cell.x) && (j == cell.y)))
                    {
                        // Constraint violated, swap back to old value.
                        cell.value = oldvalue;
                        return true;
                    }
                }
            }

            // No violation! Valid value assignment for now.
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempcell"></param>
        /// <param name="tempval"></param>
        /// <param name="c"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        Tuple<bool, Cell[,]> DomainViolated(Cell[,] partSol, int tempval, int c, int r)
        {
            // Temporarily swap with try-out value, but save old value in case of violation.
            Cell cell = cells[c, r];
            cell.value = tempval;

            // Check whether row constraints have been violated...
            for (int i = 0; i < 9; i++)
            {
                if (i != cell.x )
                {
                    //Cell rowCell = tempcell[i, cell.y];
                    //rowCell.domain.Remove(cell.value);
                    partSol[i, cell.y].domain.Remove(cell.value);
                    if (partSol[i, cell.y].domain.Count == 0) // Domain is violated..
                    {
                        return new Tuple<bool, Cell[,]>(true, partSol);
                    }
                }
            }

            // Check whether column constraints have been violated...
            for (int j = 0; j < 9; j++)
            {
                if (j != cell.y)
                {
                    partSol[cell.x, j].domain.Remove(cell.value);
                    if (partSol[cell.x, j].domain.Count == 0) // Domain is violated..
                    {

                        return new Tuple<bool, Cell[,]>(true, partSol);
                    }
                }
            }

            // // Check whether square constraints have been violated...
            int sqX = cell.x / 3;
            int sqY = cell.y / 3;
            for (int i = sqX * 3; i < sqX * 3 + 3; i++)
            {
                for (int j = sqY * 3; j < sqY * 3 + 3; j++)
                {
                    if (!((i == cell.x) && (j == cell.y)))
                    {
                        partSol[i, j].domain.Remove(cell.value);
                        if (partSol[i, j].domain.Count == 0) // Domain is violated..
                        {
                            return new Tuple<bool, Cell[,]>(true, partSol);
                        }
                    }
                }
            }

            // No violation! Valid value assignment for now.
            return new Tuple<bool, Cell[,]>(false, partSol);
        }

        #region Helperfunctions

        /// <summary>
        ///  Lists empty cells and their domains to inductively find errors.
        /// </summary>
        public void CheckEmpty()
        {
            foreach (Cell c in cells)
            {
                if (c.value == 0)
                {
                    Console.WriteLine("(" + c.x + "," + c.y + ")" + " with Domain size: " + c.domain.Count);
                    Console.WriteLine("dom: ");
                    foreach (int i in c.domain)
                    {
                        Console.WriteLine(i);
                    }
                }
            }
        }

        /// <summary>
        /// Make a deep copy of the array
        /// </summary>
        /// <param name="c"></param>
        /// <returns>The array you want copied</returns>
        Cell[,] CloneArray(Cell[,] c)
        {
            Cell[,] clonedCells = new Cell[9, 9];
            
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Cell tempC = c[x, y];
                    List<int> tempList = new List<int>(tempC.domain);
                    clonedCells[x, y] = new Cell(tempC.value, tempC.x, tempC.y, tempC.anchored, tempList);
                }
            }
            return clonedCells;
        }

        /// <summary>
        /// Helperfunctions to compare the domains of two arrays
        /// </summary>
        /// <param name="c1"> First Array</param>
        /// <param name="c2"> Second Array</param>
        void CompareArray(Cell[,] c1, Cell[,] c2)
        {
            Console.WriteLine("Compare: ");
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!c1[x, y].domain.All(c2[x, y].domain.Contains))
                    {
                        Console.WriteLine("Diff @: " + x + " : " + y + " cells: ");
                        Console.WriteLine(string.Format("Cells: ({0}).", string.Join(", ", c1[x, y].domain)));
                        Console.WriteLine(string.Format("OldCe: ({0}).", string.Join(", ", c2[x, y].domain)));
                    }
                }
            }
        }

        /// <summary>
        /// Lets you print the board, a Domain or close the console.
        /// </summary>
        public void Help()
        {
            Console.WriteLine("Welcome to CSP SudokuSolver by Pieter Barkema and Bram Kreuger. Press the following keys:");
            Console.WriteLine("Press P to print the board.");
            Console.WriteLine("Press D to show a domain.");
            Console.WriteLine("Press Esc to leave.");

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.P)
                {
                    PrintBoard(null);
                }
                if (key.Key == ConsoleKey.D)
                {
                    PrintDomain();
                }
            } while (key.Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// Helper method for printing the whole board. Choose bool true to print original board (DEBUG ONLY)   
        /// </summary>
        public void PrintBoard(Cell[,] c)
        {
            // Waar is de else? Niet zo stoer doen Barkema!
            if (c == null)
            {
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        Console.Write(cells[x, y].value + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("- - - - - - - - -");
            }
            else
            {
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        Console.Write(c[x, y].value + " ");
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("- - - - - - - - -");
            }

        }

        public void PrintDomain()
        {
            int x = -1;
            int y = -1;

            Console.WriteLine("Enter X: ");
            string strX = Console.ReadLine();
            bool resX = int.TryParse(strX, out int numX);
            if (resX)
            {
                x = numX;
            }
            Console.WriteLine("Enter Y: ");
            string strY = Console.ReadLine();
            bool resY = int.TryParse(strY, out int numY);
            if (resY)
            {
                y = numY;
            }
            if (x > -1 && x < 9 && y > -1 && y < 9)
            {
                Console.WriteLine(string.Format("Domain: ({0}).", string.Join(", ", cells[x, y].domain)));
                Console.WriteLine(cells[x, y].anchored);
            }
            else
            {
                Console.WriteLine("Wrong input, try again.");
            }
        }
        /// <summary>
        /// Loads the file containing the sudokus and take the one matching the sudokuNumber. Make sure the text file is in:
        /// \Computationele Intelligentie Pi\Computationele Intelligentie Pi\bin\Debug\netcoreapp2.0
        /// Or somthing similair.
        /// </summary>        
        public void Initialize(int sudokuNumber)
        {
            string textFile = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "sudoku_puzzels.txt"));

            //Each "Seperator" is the number of the line where there is text, not numbers, these lines seperate the sudoku's.
            List<int> seperators = new List<int>();

            using (StringReader reader = new StringReader(textFile))
            {
                string line = string.Empty;
                int counter = 0;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (Regex.IsMatch(line, @"[a-zA-z]"))
                        {
                            seperators.Add(counter);
                        }
                    }

                    counter++;

                } while (line != null);
                seperators.Add(counter - 1); //Add last line for processing 
            }
            // Only enable LoadSudoku for analytics mode, only enable AskInput for testing one sudoku
            LoadSudoku(sudokuNumber, seperators, textFile);
            //AskInput(seperators, textFile);

        }

        /// <summary>
        /// Ask for input from the user. This is an self-contained method so it can be called recursively.
        /// </summary>
        void AskInput(List<int> seperators, string textFile)
        {
            Console.WriteLine("Choose a sudoku-puzzel from: 1 to: " + (seperators.Count - 1) + " . By typing the corrosponding number");
            string sudokuInput = Console.ReadLine();
            if (int.TryParse(sudokuInput, out int res))
            {
                int sudokuNumber = int.Parse(sudokuInput);

                if (sudokuNumber < seperators.Count && sudokuNumber > 0)
                {
                    LoadSudoku(sudokuNumber - 1, seperators, textFile);
                }
                else
                {
                    Console.WriteLine("Given sudoku-number is out of range.");
                    AskInput(seperators, textFile);
                }
            }
            else
            {
                Console.WriteLine("Please insert a number, not a string...");
                AskInput(seperators, textFile);
            }
        }

        /// <summary>
        /// Converts textfile to 2D array.
        /// </summary>
        void LoadSudoku(int sudokuNumber, List<int> seperators, string textFile)
        {
            List<int[]> charlist = new List<int[]>();
            int[,] sudoku = null;

            List<string> allLines = (from l in textFile.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                     select l).ToList(); // Splits the lines on the newlines

            int size = seperators[sudokuNumber + 1] - (seperators[sudokuNumber] + 1);
            sudoku = new int[size, size];
            for (int i = seperators[sudokuNumber] + 1; i < seperators[sudokuNumber + 1]; i++)
            {
                charlist.Add(Array.ConvertAll(allLines[i].ToCharArray(), c => (int)Char.GetNumericValue(c)));
            }
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    sudoku[x, y] = (charlist[x])[y];
                }
            }
            originalboard = sudoku;
        }

        #endregion
    }
}
