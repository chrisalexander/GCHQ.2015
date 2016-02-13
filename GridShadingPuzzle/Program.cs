using System;
using System.Collections.Generic;
using System.Linq;
using GridShadingPuzzle.DomainModel;
using GridShadingPuzzle.Solver;

namespace GridShadingPuzzle
{
    /// <summary>
    /// Program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Application arguments.</param>
        public static void Main(string[] args)
        {
            var size = Properties.Settings.Default.GridSize;
            var initialBlackSquares = GetInitialBlackSquares();
            var rowClues = GetClues(Properties.Settings.Default.Rows);
            var colClues = GetClues(Properties.Settings.Default.Columns);

            var solver = new GridSolver();

            solver.Initialize(size, initialBlackSquares, rowClues, colClues);

            solver.PrintGrid();

            Console.WriteLine("Performing initial analysis");

            foreach (var analysis in solver.InitialAnalysis())
            {
                Console.WriteLine(analysis);
            }

            solver.PrintGrid();

            var foundCells = 0;
            var lastFoundCells = 0;

            do
            {
                lastFoundCells = foundCells;
                foundCells = ReduceAndResolve(solver);
            } while (foundCells != lastFoundCells);

            var permutations = solver.EvaluateRemainingPermutations();

            Console.WriteLine($"Permutations remaining: {permutations}");

            var reductionSolved = solver.ReducePermutations();
            var validateSolved = solver.ValidatePermutations();
            var foundCommonCells = solver.EvaluateCommonCells();

            Console.WriteLine($"Sequences solved by reduction: {reductionSolved}");
            Console.WriteLine($"Sequences solved by validation: {validateSolved}");
            Console.WriteLine($"Common cells found: {foundCommonCells}");

            solver.PrintGrid();

            Console.ReadLine();
        }

        /// <summary>
        /// Runs the reduction steps and returns how many common cells were found.
        /// </summary>
        /// <param name="solver">The solver to act on.</param>
        /// <returns>The number of common cells found.</returns>
        private static int ReduceAndResolve(GridSolver solver)
        {
            var foundCommonCells = solver.EvaluateCommonCells();

            Console.WriteLine($"Common cells found: {foundCommonCells}");

            solver.PrintGrid();

            var reductionSolved = solver.ReducePermutations();

            Console.WriteLine($"Rows solved by reduction: {reductionSolved}");

            var estimate = solver.BestGuessPermutations();

            Console.WriteLine($"Estimated permutations remaining: {estimate.Item1}");
            Console.WriteLine($"Rows and columns that are still guesses: {estimate.Item2}");

            solver.PrintGrid();

            return foundCommonCells;
        }

        /// <summary>
        /// Helper which retrieves the initial black squares from configuration.
        /// </summary>
        /// <returns>The initial black squares.</returns>
        private static List<GridReference> GetInitialBlackSquares()
        {
            var initialBlackSquaresString = Properties.Settings.Default.InitialBlackCells;

            var initialBlackSquares = new List<GridReference>();

            foreach (var referenceString in initialBlackSquaresString.Split(';'))
            {
                var reference = referenceString.Split(',').ToList();

                if (reference.Count != 2)
                {
                    throw new Exception($"Reference string {referenceString} contains the wrong number of elements");
                }

                var rowReference = int.Parse(reference[0]);
                var colReference = int.Parse(reference[1]);

                initialBlackSquares.Add(new GridReference(rowReference, colReference));
            }

            return initialBlackSquares;
        }

        /// <summary>
        /// Converts a clue string in to a clue collection.
        /// </summary>
        /// <param name="clues">The clue string.</param>
        /// <returns>Clue collection.</returns>
        private static List<List<int>> GetClues(string clues)
        {
            var clueCollection = new List<List<int>>();

            foreach (var clue in clues.Split(';'))
            {
                var clueNumbers = clue.Split(',');

                var separateClue = new List<int>();

                foreach (var clueNumber in clueNumbers)
                {
                    separateClue.Add(int.Parse(clueNumber));
                }

                clueCollection.Add(separateClue);
            }

            return clueCollection;
        }
    }
}
