using System;
using System.Collections.Generic;
using System.Linq;
using GridShadingPuzzle.Graph;

namespace GridShadingPuzzle.DomainModel
{
    /// <summary>
    /// Represents a row or a column of values.
    /// </summary>
    public class Sequence
    {
        /// <summary>
        /// Cache of all permutations available to this sequence.
        /// </summary>
        private List<List<bool>> permutationCache;

        /// <summary>
        /// Creates a new sequence with the specified length.
        /// </summary>
        /// <param name="length">The length of the sequence.</param>
        public Sequence(int length)
        {
            this.Cells = new List<Cell>();

            for (var i = 0; i < length; i++)
            {
                this.Cells.Add(new Cell());
            }
        }
        
        /// <summary>
        /// The cells within the sequence.
        /// </summary>
        public List<Cell> Cells { get; private set; }

        /// <summary>
        /// The clue.
        /// </summary>
        public List<int> Clue { get; set; }

        /// <summary>
        /// Whether the sequence is solved.
        /// </summary>
        public bool Solved { get; private set; }

        /// <summary>
        /// Returns the current number of permutations.
        /// </summary>
        public int AvailablePermutations
        {
            get
            {
                return this.permutationCache == null ? 0 : this.permutationCache.Count;
            }
        }

        /// <summary>
        /// Sets the cell at the specified index to be black.
        /// </summary>
        /// <param name="reference">The cell reference.</param>
        public void SetCellBlack(GridReference reference)
        {
            this.Cells[reference.ColumnIndex].Black = true;
        }

        /// <summary>
        /// Attempts an instance solve.
        /// </summary>
        /// <returns>Whether it was solved.</returns>
        public bool AttemptInstantSolve()
        {
            var solved = this.GetOccupiedSquareCount() == this.Cells.Count;

            if (!solved)
            {
                return false;
            }

            var currentIndex = 0;

            foreach (var clue in this.Clue)
            {
                for (var i = 0; i < clue; i++)
                {
                    this.Cells[currentIndex].TrySetBlack();
                    currentIndex++;
                }

                // Leave a white space at the end.
                if (currentIndex < this.Cells.Count)
                {
                    this.Cells[currentIndex].TrySetWhite();
                }
                currentIndex++;
            }

            this.UpdateSolved();
            this.permutationCache = null;
            return true;
        }
        
        /// <summary>
        /// Estimates the number of permutations in the sequence clue.
        /// </summary>
        /// <returns>The estimated number of permutations.</returns>
        public double EstimatePermutations()
        {
            var availableSquares = this.Cells.Count - this.GetOccupiedSquareCount();

            // No permutations if we are full already.
            if (availableSquares == 0)
            {
                return 1;
            }

            // The available locations are one after each clue phase, plus one at the start.
            var squareAllocatableLocationCount = this.Clue.Count + 1;

            // See http://math.stackexchange.com/questions/192670/n-unlabelled-balls-in-m-labeled-buckets
            return this.Factorial(availableSquares + squareAllocatableLocationCount - 1) /
                    (this.Factorial(availableSquares) * this.Factorial(squareAllocatableLocationCount - 1));
        }

        /// <summary>
        /// Returns the best case number of permutations remaining.
        /// </summary>
        /// <returns>Guess, and a flag indicating whether it is a guess or not.</returns>
        public Tuple<double, bool> BestGuessPermutations()
        {
            if (this.permutationCache != null)
            {
                return new Tuple<double, bool>(this.permutationCache.Count, false);
            }

            return new Tuple<double, bool>(this.EstimatePermutations(), true);
        }

        /// <summary>
        /// Looks at each permutation for the sequence and attempt to find common cells which are always black.
        /// </summary>
        /// <returns>The found number of common cells.</returns>
        public int EvaluateCommonCells()
        {
            this.PopulatePermutations();

            if (this.permutationCache.Count == 0)
            {
                return 0;
            }

            var blackCombination = new List<bool>();
            var whiteCombination = new List<bool>();

            foreach (var cell in this.Cells)
            {
                blackCombination.Add(true);
                whiteCombination.Add(true);
            }

            foreach (var permutation in this.permutationCache)
            {
                for (var i = 0; i < this.Cells.Count; i++)
                {
                    blackCombination[i] = blackCombination[i] && permutation[i];
                    whiteCombination[i] = whiteCombination[i] && !permutation[i];
                }
            }
            
            var commonCells = 0;
            
            for (var i = 0; i < this.Cells.Count; i++)
            {
                if (blackCombination[i])
                {
                    this.Cells[i].TrySetBlack();
                    commonCells++;
                }
                else if (whiteCombination[i])
                {
                    this.Cells[i].TrySetWhite();
                    commonCells++;
                }
            }

            return commonCells;
        }

        /// <summary>
        /// Generates permutations from all of the available combinations remaining.
        /// </summary>
        /// <returns>The number of permutations.</returns>
        public int GenerateRemainingPermutations()
        {
            this.permutationCache = new List<List<bool>>();

            var rootNode = new Node<bool>();

            foreach (var cell in this.Cells)
            {
                if (!cell.Known)
                {
                    foreach (var leaf in rootNode.Leaves().ToList())
                    {
                        leaf.Children.Add(new Node<bool>(true));
                        leaf.Children.Add(new Node<bool>(false));
                    }
                }
            }
            
            foreach (var permutation in rootNode.Permutations())
            {
                var unknownIndex = 1;

                var perm = new List<bool>();

                for (var i = 0; i < this.Cells.Count; i++)
                {
                    if (!this.Cells[i].Known)
                    {
                        perm.Add(permutation[unknownIndex]);
                        unknownIndex++;
                    } else
                    {
                        perm.Add(this.Cells[i].Black);
                    }
                }

                this.permutationCache.Add(perm);
            }

            return this.AvailablePermutations;
        }

        /// <summary>
        /// Reduce the permutations available to those that are still possible.
        /// </summary>
        /// <returns>Whether the sequence was solved.</returns>
        public bool ReducePermutations()
        {
            this.PopulatePermutations();

            var initialCount = this.permutationCache.Count;

            for (var p = this.permutationCache.Count - 1; p >= 0; p--)
            {
                var stillValid = true;

                for (var i = 0; i < this.Cells.Count; i++)
                {
                    if (this.Cells[i].Known)
                    {
                        // If the cell is black and the permutation is not at that point, it is not valid
                        if (this.Cells[i].Black && !this.permutationCache[p][i])
                        {
                            stillValid = false;
                            break;
                        }

                        // Similarly if the cell is white and the permutation is not, it is also not valid
                        if (this.Cells[i].White && this.permutationCache[p][i])
                        {
                            stillValid = false;
                            break;
                        }
                    }
                }

                if (!stillValid)
                {
                    this.permutationCache.RemoveAt(p);
                }
            }

            return EvaluatePermutations(initialCount);
        }

        /// <summary>
        /// Validates the permutations to check they meet the clue criteria.
        /// </summary>
        /// <returns>Whether the sequence is solved.</returns>
        public bool ValidatePermutations()
        {
            this.DeduplicatePermutations();

            var initialCount = this.permutationCache.Count;

            for (var p = this.permutationCache.Count - 1; p >= 0; p--)
            {
                var permutation = this.permutationCache[p];

                var permutationClue = new List<int>();
                var currentClue = 0;

                foreach (var perm in permutation)
                {
                    if (perm)
                    {
                        currentClue++;
                    }
                    else
                    {
                        if (currentClue > 0)
                        {
                            permutationClue.Add(currentClue);
                        }
                        currentClue = 0;
                    }
                }

                if (currentClue > 0)
                {
                    permutationClue.Add(currentClue);
                }

                if (permutationClue.Count != this.Clue.Count)
                {
                    this.permutationCache.RemoveAt(p);
                    continue;
                }

                var stillValid = true;

                for (var i = 0; i < this.Clue.Count; i++)
                {
                    if (this.Clue[i] != permutationClue[i])
                    {
                        stillValid = false;
                        break;
                    }
                }

                if (!stillValid)
                {
                    this.permutationCache.RemoveAt(p);
                }
            }

            return this.EvaluatePermutations(initialCount);
        }

        /// <summary>
        /// Deduplicates the permutation cache.
        /// </summary>
        private void DeduplicatePermutations()
        {
            for (var p = this.permutationCache.Count - 1; p >= 0; p--)
            {
                var permutation = this.permutationCache[p];

                var allSame = true;

                for (var q = p - 1; q >= 0; q--)
                {
                    for (var i = 0; i < permutation.Count; i++)
                    {
                        if (permutation[i] != this.permutationCache[q][i])
                        {
                            allSame = false;
                            break;
                        }
                    }

                    if (allSame)
                    {
                        break;
                    }
                }

                if (allSame)
                {
                    this.permutationCache.RemoveAt(p);
                }
            }
        }

        /// <summary>
        /// Handles the permutation cache to check whether the sequence can be solved.
        /// </summary>
        /// <param name="initialCount">The initial number of permutations in the cache.</param>
        /// <returns>Whether the sequence is solved.</returns>
        private bool EvaluatePermutations(int initialCount)
        {
            if (this.permutationCache.Count == 0)
            {
                return false;// throw new Exception("No permutations remaining, something has gone catastrophically wrong");
            }

            var permutationsRemoved = initialCount - this.permutationCache.Count;

            if (this.permutationCache.Count == 1)
            {
                var result = this.permutationCache[0];

                for (var i = 0; i < result.Count; i++)
                {
                    if (result[i])
                    {
                        this.Cells[i].TrySetBlack();
                    }
                    else
                    {
                        this.Cells[i].TrySetWhite();
                    }
                }

                this.UpdateSolved();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Update this sequence's solved flag.
        /// </summary>
        public void UpdateSolved()
        {
            foreach (var cell in this.Cells)
            {
                if (!cell.Known)
                {
                    return;
                }
            }

            this.Solved = true;
        }

        /// <summary>
        /// Populates the permutation cache so they are available for evaluation.
        /// </summary>
        private void PopulatePermutations()
        {
            if (this.permutationCache != null)
            {
                return;
            }

            this.permutationCache = new List<List<bool>>();

            var spaceCount = this.Clue.Count + 1;
            var availableSquares = this.Cells.Count - this.GetOccupiedSquareCount();

            foreach (var spacePermutation in this.GenerateSpacePermutations(spaceCount, availableSquares))
            {
                this.permutationCache.Add(this.GenerateSequence(spacePermutation));
            }
        }

        /// <summary>
        /// Generates all the valid permutations of spaces within the sequence.
        /// </summary>
        /// <param name="spaceCount">The number of slots available.</param>
        /// <param name="availableSquares">The number of squares that can go in the slots.</param>
        /// <returns>The space permutations.</returns>
        private IEnumerable<List<int>> GenerateSpacePermutations(int spaceCount, int availableSquares)
        {
            var rootNode = new Node<int>(0);
            
            for (var space = 0; space < spaceCount; space++)
            {
                var targetNodes = rootNode.Leaves().ToList();
                
                for (var count = 0; count <= availableSquares; count++)
                {
                    foreach (var node in targetNodes)
                    {
                        node.Children.Add(new Node<int>(node.Value + count));
                    }
                }
            }

            // Remove the nodes where they get too large.
            rootNode.Prune((value) => value > availableSquares);

            var permutations = rootNode.Permutations().Where(p => p[p.Count - 1] == availableSquares).ToList();

            var template = new List<int>();

            for (var i = 0; i < spaceCount; i++)
            {
                template.Add(i == 0 || i == spaceCount - 1 ? 0 : 1);
            }

            foreach (var permutation in permutations)
            {
                var result = template.ToList();

                for (var i = 1; i < permutation.Count; i++)
                {
                    result[i - 1] += permutation[i] - permutation[i - 1];
                }

                yield return result;
            }
        }

        /// <summary>
        /// Generates a sequence given the current clue and the list of spaces.
        /// </summary>
        /// <param name="spaces">The space sizes.</param>
        /// <returns>The overall sequence.</returns>
        private List<bool> GenerateSequence(List<int> spaces)
        {
            var sequence = new List<bool>();

            for (var i = 0; i < this.Clue.Count; i++)
            {
                // Add the set of spaces
                for (var s = 0; s < spaces[i]; s++)
                {
                    sequence.Add(false);
                }

                // Add the set of clues
                for (var c = 0; c < this.Clue[i]; c++)
                {
                    sequence.Add(true);
                }
            }

            // Add the final set of spaces
            for (var s = 0; s < spaces[spaces.Count - 1]; s++)
            {
                sequence.Add(false);
            }

            return sequence;
        }

        /// <summary>
        /// Gets the minimum occupied square count for the clue.
        /// </summary>
        /// <returns>The minimum occupied square count.</returns>
        private int GetOccupiedSquareCount()
        {
            var clueTotal = 0;

            foreach (var clueValue in this.Clue)
            {
                clueTotal += clueValue;
            }

            return clueTotal + this.Clue.Count - 1;
        }

        /// <summary>
        /// Compute the factorial of the value.
        /// </summary>
        /// <param name="value">The factorial value to compute.</param>
        /// <returns>The factorial.</returns>
        private double Factorial(int value)
        {
            double output = 1;

            for (var i = 1; i <= value; i++)
            {
                output *= i;
            }

            return output;
        }
    }
}
