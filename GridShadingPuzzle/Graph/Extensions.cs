using System;
using System.Collections.Generic;
using System.Linq;

namespace GridShadingPuzzle.Graph
{
    /// <summary>
    /// Graph extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Prunes the node and all child nodes to remove those
        /// that meet the specified criteria.
        /// </summary>
        /// <param name="this">The node to start pruning from.</param>
        /// <param name="maxValue">The max value to prune off.</param>
        public static void Prune<T>(this Node<T> @this, Func<T, bool> prune)
        {
            // Prune the child nodes.
            for (var i = @this.Children.Count - 1; i >= 0; i--)
            {
                if (prune(@this.Children[i].Value))
                {
                    @this.Children.RemoveAt(i);
                }
            }

            // Prune the remaining children
            foreach (var child in @this.Children)
            {
                child.Prune(prune);
            }
        }

        /// <summary>
        /// Returns all of the leaves relative to the current node.
        /// </summary>
        /// <typeparam name="T">The type of the node value.</typeparam>
        /// <returns>All leaves from the current node.</returns>
        public static IEnumerable<Node<T>> Leaves<T>(this Node<T> @this)
        {
            if (@this.Children.Count == 0)
            {
                // If there are no children, then this is the leaf node.
                yield return @this;
                yield break;
            }

            foreach (var child in @this.Children)
            {
                foreach (var leaf in child.Leaves())
                {
                    yield return leaf;
                }
            }
        }

        /// <summary>
        /// Get all of the permutations from the root node.
        /// </summary>
        /// <typeparam name="T">The type of the node value.</typeparam>
        /// <param name="this">The current node.</param>
        /// <returns>All of the permutations from the node down.</returns>
        public static IEnumerable<List<T>> Permutations<T>(this Node<T> @this, List<T> ancestors = null)
        {
            if (ancestors == null)
            {
                ancestors = new List<T>();
            }

            var result = ancestors.ToList();
            result.Add(@this.Value);

            if (@this.Children.Count == 0)
            {
                // Make sure this node has its own copy fo its ancestors.
                yield return result;
                yield break;
            }
            
            foreach (var child in @this.Children)
            {
                foreach (var permutation in child.Permutations(result))
                {
                    yield return permutation;
                }
            }
        }
    }
}
