using System.Collections.Generic;

namespace GridShadingPuzzle.DomainModel.Results
{
    /// <summary>
    /// The result of computing a permutation estimation for the entire grid.
    /// </summary>
    public class PermutationEstimationResult
    {
        /// <summary>
        /// Creates a new permutation estimate result.
        /// </summary>
        /// <param name="rowPermutations">The collection of row permutation estimates.</param>
        /// <param name="colPermutations">The collection of column permutation estimates.</param>
        public PermutationEstimationResult(List<double> rowPermutations, List<double> colPermutations)
        {
            this.RowPermutationEstimations = rowPermutations;
            this.ColumnPermutationEstimations = colPermutations;

            this.EstimateOverallPermutations();
        }

        /// <summary>
        /// The estimated number of permutations for each row.
        /// </summary>
        public List<double> RowPermutationEstimations { get; private set; }

        /// <summary>
        /// The estimated number of permutations for each column.
        /// </summary>
        public List<double> ColumnPermutationEstimations { get; private set; }

        /// <summary>
        /// The overall permutation estimation count.
        /// </summary>
        public double OverallPermutations { get; private set; }

        /// <summary>
        /// Uses the current data to compute the overall estimate.
        /// </summary>
        private void EstimateOverallPermutations()
        {
            double overallPermutations = 1;

            foreach (var row in this.RowPermutationEstimations)
            {
                overallPermutations *= row;
            }

            foreach (var col in this.ColumnPermutationEstimations)
            {
                overallPermutations *= col;
            }

            this.OverallPermutations = overallPermutations;
        }
    }
}
