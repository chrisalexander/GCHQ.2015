namespace GridShadingPuzzle.DomainModel
{
    /// <summary>
    /// Represents a reference to a cell in the grid.
    /// </summary>
    public class GridReference
    {
        /// <summary>
        /// Creates a new grid reference.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        public GridReference(int rowIndex, int colIndex)
        {
            this.RowIndex = rowIndex;
            this.ColumnIndex = colIndex;
        }

        /// <summary>
        /// The row index.
        /// </summary>
        public int RowIndex { get; private set; }

        /// <summary>
        /// The column index.
        /// </summary>
        public int ColumnIndex { get; private set; }
    }
}
