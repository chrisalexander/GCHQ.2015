using System;
using System.Collections.Generic;
using GridShadingPuzzle.DomainModel;

namespace GridShadingPuzzle.Solver
{
    /// <summary>
    /// Solver for the grid.
    /// </summary>
    public class GridSolver
    {
        /// <summary>
        /// The grid we are solving.
        /// </summary>
        private Grid grid;
        
        /// <summary>
        /// Initializes the grid with the specified side length and initial condition.
        /// </summary>
        /// <param name="sideLength">The length of the sides of the grid.</param>
        /// <param name="initialBlackCells">The cells which are initially black.</param>
        /// <param name="rowClues">The row clues.</param>
        /// <param name="columnClues">The column clues.</param>
        public void Initialize(int sideLength, List<GridReference> initialBlackCells, List<List<int>> rowClues, List<List<int>> columnClues)
        {
            this.grid = new Grid(sideLength);

            foreach (var cell in initialBlackCells)
            {
                this.grid.SetCellBlack(cell);
            }

            this.grid.SetRowClues(rowClues);
            this.grid.SetColumnClues(columnClues);
        }

        /// <summary>
        /// Provides an initial analysis of the problem.
        /// </summary>
        /// <returns>Human-readable problem notes.</returns>
        public List<string> InitialAnalysis()
        {
            var analysis = new List<string>();

            var initialPermutations = Math.Pow(2, grid.SideLength * grid.SideLength);

            analysis.Add($"Total permutations: {initialPermutations}");

            var instantSolveResult = grid.AttemptInstantSolve();

            analysis.Add($"Rows solved instantly: {instantSolveResult.Item1}");
            analysis.Add($"Columns solved instantly: {instantSolveResult.Item2}");

            var instantSolvePermutations = Math.Pow(2, (grid.SideLength - instantSolveResult.Item1) * (grid.SideLength - instantSolveResult.Item2));

            analysis.Add($"Permutations remaining: {instantSolvePermutations}");

            var estimatedPermutations = grid.EstimatePermutations();
            
            for (var i = 0; i < estimatedPermutations.RowPermutationEstimations.Count; i++)
            {
                analysis.Add($"Row {i} estimated permutations: {estimatedPermutations.RowPermutationEstimations[i]}");
            }

            for (var i = 0; i < estimatedPermutations.ColumnPermutationEstimations.Count; i++)
            {
                analysis.Add($"Column {i} estimated permutations: {estimatedPermutations.ColumnPermutationEstimations[i]}");
            }

            analysis.Add($"Overall permutations: {estimatedPermutations.OverallPermutations}");

            return analysis;
        }

        /// <summary>
        /// Looks at each permutation for each row/column and attempt to find common cells which are always black.
        /// </summary>
        /// <returns>The found number of common cells.</returns>
        public int EvaluateCommonCells()
        {
            return this.grid.EvaluateCommonCells();
        }

        /// <summary>
        /// Reduce the permutations available to those that are still possible.
        /// </summary>
        /// <returns>How many further rows or columns were solved.</returns>
        public int ReducePermutations()
        {
            return this.grid.ReducePermutations();
        }

        /// <summary>
        /// Validate the permutations available to those that are still possible.
        /// </summary>
        /// <returns>How many further rows or columns were solved.</returns>
        public int ValidatePermutations()
        {
            return this.grid.ValidatePermutations();
        }

        /// <summary>
        /// Returns the number of best guess permutations, along with how many rows and columns are still guesses.
        /// </summary>
        /// <returns>The number of permutations, and how many rows and columns were guesses.</returns>
        public Tuple<double, int> BestGuessPermutations()
        {
            return this.grid.BestGuessPermutations();
        }

        /// <summary>
        /// Goes through unsolved rows and columns and generates all remaining permutations.
        /// </summary>
        /// <returns>The number of permutations remaining.</returns>
        public double EvaluateRemainingPermutations()
        {
            return this.grid.EvaluateRemainingPermutations();
        }

        /// <summary>
        /// Prints the current solved status of the grid.
        /// </summary>
        public void PrintGrid()
        {
            Console.WriteLine();
            Console.WriteLine("Current grid state:");
            Console.WriteLine();

            var colWidth = 3;

            var headerCollection = new List<string>();

            for (var i = 1; i <= this.grid.Columns.Count; i++)
            {
                headerCollection.Add(this.MakeWidth(i, colWidth));
            }

            var header = " " + "".PadLeft(colWidth) + string.Join(" ", headerCollection);

            Console.WriteLine(header);
            Console.WriteLine();

            var rowIndex = 1;

            foreach (var row in this.grid.Rows)
            {
                var rowCollection = new List<string>();

                foreach (var col in row.Cells)
                {
                    rowCollection.Add((col.Known ? col.Black ? "X" : "" : ".").PadLeft(colWidth));
                }

                var rowString = this.MakeWidth(rowIndex, colWidth) + " " + string.Join(" ", rowCollection);

                Console.WriteLine(rowString);
                Console.WriteLine();

                rowIndex++;
            }

            var solvedSequences = grid.SolvedSequenceCount();

            Console.WriteLine();
            Console.WriteLine($"Completion percentage: {grid.KnownCellPercentage()}%");
            Console.WriteLine($"Completed rows: {solvedSequences.Item1}");
            Console.WriteLine($"Completed columns: {solvedSequences.Item2}");
            Console.WriteLine();
        }

        /// <summary>
        /// Takes a number and formats it as a string with a specific width.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private string MakeWidth(int number, int width)
        {
            var numberString = number.ToString();
            return numberString.PadLeft(width);
        }
    }
}
