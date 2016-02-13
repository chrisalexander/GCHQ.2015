using System.Collections.Generic;

namespace GridShadingPuzzle.Graph
{
    /// <summary>
    /// A graph node.
    /// </summary>
    /// <typeparam name="T">The type of the value of the node.</typeparam>
    public class Node<T>
    {
        /// <summary>
        /// Create a new node with the default value.
        /// </summary>
        public Node()
        {
            this.Children = new List<Node<T>>();
        }

        /// <summary>
        /// Create a new node with the specified initial value.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public Node(T initialValue)
            : this()
        {
            this.Value = initialValue;
        }

        /// <summary>
        /// The child nodes.
        /// </summary>
        public List<Node<T>> Children { get; private set; }

        /// <summary>
        /// This node's value.
        /// </summary>
        public T Value = default(T);
    }
}
