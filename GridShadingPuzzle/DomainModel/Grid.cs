using System;
using System.Collections.Generic;
using System.Linq;
using GridShadingPuzzle.DomainModel.Results;

namespace GridShadingPuzzle.DomainModel
{
    /// <summary>
    /// Represents an entire game grid.
    /// </summary>
    public class Grid
    {
        /// <summary>
        /// The permutation cap for expensive analysis.
        /// </summary>
        private const int PermutationCap = 5000;

        /// <summary>
        /// Creates a grid with a designated side length.
        /// </summary>
        /// <remarks>
        /// It is assumed to be a square.
        /// </remarks>
        /// <param name="sideLength">The length of the sides</param>
        public Grid(int sideLength)
        {
            this.SideLength = sideLength;

            this.Rows = new List<Sequence>();
            this.Columns = new List<Sequence>();

            for (var i = 0; i < this.SideLength; i++)
            {
                this.Rows.Add(new Sequence(this.SideLength));
                this.Columns.Add(new Sequence(this.SideLength));
            }
        }

        /// <summary>
        /// The fixed length of the sides.
        /// </summary>
        public int SideLength { get; private set; }

        /// <summary>
        /// The collection of rows.
        /// </summary>
        public List<Sequence> Rows { get; private set; }

        /// <summary>
        /// The collection of columns.
        /// </summary>
        public List<Sequence> Columns { get; private set; }
        
        /// <summary>
        /// Sets the cell at the specified index to be black.
        /// </summary>
        /// <param name="reference">The cell reference.</param>
        public void SetCellBlack(GridReference reference)
        {
            this.ValidateIndices(reference.RowIndex, reference.ColumnIndex);
            this.Rows[reference.RowIndex].SetCellBlack(reference);
            this.UpdateReferences(reference);
        }

        /// <summary>
        /// Sets the row clues.
        /// </summary>
        /// <param name="clues">The row clues.</param>
        public void SetRowClues(List<List<int>> clues)
        {
            for (var i = 0; i < clues.Count; i++)
            {
                this.Rows[i].Clue = clues[i];
            }
        }

        /// <summary>
        /// Sets the column clues.
        /// </summary>
        /// <param name="clues">The column clues.</param>
        public void SetColumnClues(List<List<int>> clues)
        {
            for (var i = 0; i < clues.Count; i++)
            {
                this.Columns[i].Clue = clues[i];
            }
        }

        /// <summary>
        /// Attempts an instant solve.
        /// </summary>
        /// <returns>Tuple containing the number of rows and columns instantly solved.</returns>
        public Tuple<int, int> AttemptInstantSolve()
        {
            var rowCount = 0;
            var colCount = 0;

            foreach (var row in this.Rows)
            {
                if (row.AttemptInstantSolve())
                {
                    rowCount++;
                }
            }

            foreach (var col in this.Columns)
            {
                if (col.AttemptInstantSolve())
                {
                    colCount++;
                }
            }

            this.UpdateReferences();

            return new Tuple<int, int>(rowCount, colCount);
        }

        /// <summary>
        /// Estimates the permutations available to each row and column, and the total permutations.
        /// </summary>
        /// <returns>Permuntation estimate for each row and column.</returns>
        public PermutationEstimationResult EstimatePermutations()
        {
            var rowList = new List<double>();

            foreach (var row in this.Rows)
            {
                rowList.Add(row.EstimatePermutations());
            }

            var colList = new List<double>();

            foreach (var col in this.Columns)
            {
                colList.Add(col.EstimatePermutations());
            }

            return new PermutationEstimationResult(rowList, colList);
        }

        /// <summary>
        /// Looks at each permutation for each row/column and attempt to find common cells which are always black.
        /// </summary>
        /// <returns>The found number of common cells.</returns>
        public int EvaluateCommonCells()
        {
            var foundCommon = 0;
            
            foreach (var row in this.Rows)
            {
                if (row.EstimatePermutations() < PermutationCap)
                {
                    foundCommon += row.EvaluateCommonCells();
                }
            }

            foreach (var col in this.Columns)
            {
                if (col.EstimatePermutations() < PermutationCap)
                {
                    foundCommon += col.EvaluateCommonCells();
                }
            }
            
            return foundCommon;
        }

        /// <summary>
        /// Reduce the permutations available to those that are still possible.
        /// </summary>
        /// <returns>How many further rows or columns were solved.</returns>
        public int ReducePermutations()
        {
            var solvedCount = 0;

            foreach (var row in this.Rows)
            {
                if (row.Solved || row.EstimatePermutations() > PermutationCap)
                {
                    continue;
                }

                var solved = row.ReducePermutations();
                if (solved)
                {
                    solvedCount++;
                    this.UpdateReferences();
                }
            }

            foreach (var col in this.Columns)
            {
                if (col.Solved || col.EstimatePermutations() > PermutationCap)
                {
                    continue;
                }

                var solved = col.ReducePermutations();
                if (solved)
                {
                    solvedCount++;
                    this.UpdateReferences();
                }
            }

            return solvedCount;
        }

        /// <summary>
        /// Validate the permutations available to those that are still possible.
        /// </summary>
        /// <returns>How many further rows or columns were solved.</returns>
        public int ValidatePermutations()
        {
            var solvedCount = 0;

            foreach (var row in this.Rows)
            {
                if (row.Solved)
                {
                    continue;
                }

                var solved = row.ValidatePermutations();
                if (solved)
                {
                    solvedCount++;
                    this.UpdateReferences();
                }
            }

            foreach (var col in this.Columns)
            {
                if (col.Solved)
                {
                    continue;
                }

                var solved = col.ValidatePermutations();
                if (solved)
                {
                    solvedCount++;
                    this.UpdateReferences();
                }
            }

            return solvedCount;
        }

        /// <summary>
        /// Returns the number of best guess permutations, along with how many rows and columns are still guesses.
        /// </summary>
        /// <returns>The number of permutations, and how many rows and columns were guesses.</returns>
        public Tuple<double, int> BestGuessPermutations()
        {
            double permutations = 1;
            var guesses = 0;

            foreach (var sequence in this.Rows.Concat(this.Columns))
            {
                var result = sequence.BestGuessPermutations();
                permutations *= result.Item1;
                if (result.Item2)
                {
                    guesses++;
                }
            }

            return new Tuple<double, int>(permutations, guesses);
        }

        /// <summary>
        /// Gets the percentage of cells that are known to be solved.
        /// </summary>
        /// <returns>The number of solved cells.</returns>
        public int KnownCellPercentage()
        {
            this.UpdateReferences();

            double known = 0;
            double total = 0;

            foreach (var row in this.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    if (cell.Known)
                    {
                        known++;
                    }

                    total++;
                }
            }

            return (int)((known / total) * 100);
        }

        /// <summary>
        /// Returns the number of solved rows and cols.
        /// </summary>
        /// <returns>The solved count for rows and columns.</returns>
        public Tuple<int, int> SolvedSequenceCount()
        {
            var rowsSolved = 0;
            var colSolved = 0;

            foreach (var row in this.Rows)
            {
                if (row.Solved)
                {
                    rowsSolved++;
                }
            }

            foreach (var col in this.Columns)
            {
                if (col.Solved)
                {
                    colSolved++;
                }
            }

            return new Tuple<int, int>(rowsSolved, colSolved);
        }

        /// <summary>
        /// Goes through unsolved rows and columns and generates all remaining permutations.
        /// </summary>
        /// <returns>The number of permutations remaining.</returns>
        public double EvaluateRemainingPermutations()
        {
            double permutations = 1;

            foreach (var row in this.Rows)
            {
                if (row.Solved)
                {
                    continue;
                }

                permutations *= row.GenerateRemainingPermutations();
            }

            foreach (var col in this.Columns)
            {
                if (col.Solved)
                {
                    continue;
                }

                permutations *= col.GenerateRemainingPermutations();
            }

            return permutations;
        }

        /// <summary>
        /// Ensure the columns collection is up to date with the row collection.
        /// </summary>
        private void UpdateReferences()
        {
            for (var rowIndex = 0; rowIndex < this.SideLength; rowIndex++)
            {
                for (var colIndex = 0; colIndex < this.SideLength; colIndex++)
                {
                    this.UpdateReferences(new GridReference(rowIndex, colIndex));
                }
            }
        }

        /// <summary>
        /// Update all solved flags.
        /// </summary>
        private void UpdateAllSolutions()
        {
            foreach (var row in this.Rows)
            {
                row.UpdateSolved();
            }

            foreach (var col in this.Columns)
            {
                col.UpdateSolved();
            }
        }

        /// <summary>
        /// Updates the column collection to match the row collection in the specific cell.
        /// </summary>
        /// <param name="reference">The cell reference.</param>
        private void UpdateReferences(GridReference reference)
        {
            var col = this.Columns[reference.ColumnIndex].Cells[reference.RowIndex];
            var row = this.Rows[reference.RowIndex].Cells[reference.ColumnIndex];

            if (row.Known && col.Known)
            {
                // If they are both known, then blow up if they are different
                if (row.Black != col.Black)
                {
                    throw new Exception($"Two known cells in rows and cols have different values: ({reference.RowIndex},{reference.ColumnIndex})");
                }

                return;
            }
            else if (!row.Known && !col.Known)
            {
                // If neither are known then no changes to make
                return;
            }

            if (col.Known)
            {
                // Column is known, so set the row value
                row.Black = col.Black;
            }
            else
            {
                col.Black = row.Black;
            }
        }

        /// <summary>
        /// Validates that all provided indices are within the specified range of the grid.
        /// </summary>
        /// <param name="indices">The indices to check.</param>
        private void ValidateIndices(params int[] indices)
        {
            foreach (var index in indices)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException($"indices({index})", "The value is too small; expected minimum size 0");
                }

                if (index >= this.SideLength)
                {
                    throw new ArgumentOutOfRangeException($"indices({index})", $"The value is too large; expected maximum size {this.SideLength}-1");
                }
            }
        }
    }
}
