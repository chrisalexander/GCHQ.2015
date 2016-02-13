using System;

namespace GridShadingPuzzle.DomainModel
{
    /// <summary>
    /// Represents a cell in the grid.
    /// </summary>
    public class Cell
    {
        /// <summary>
        /// The value of the cell, null if unknown.
        /// </summary>
        private bool? black = null;

        /// <summary>
        /// Whether the cell is black.
        /// </summary>
        public bool Black
        {
            get
            {
                return this.black.HasValue && this.black.Value == true;
            }

            set
            {
                if (this.black.HasValue)
                {
                    throw new Exception("Cell already set, cannot be reset");
                }

                this.black = value;
            }
        }

        /// <summary>
        /// Whether the cell is white.
        /// </summary>
        public bool White
        {
            get
            {
                return !this.Black;
            }

            set
            {
                this.Black = !value;
            }
        }

        /// <summary>
        /// Returns whether the cell has a known value or not.
        /// </summary>
        public bool Known
        {
            get
            {
                return this.black.HasValue;
            }
        }

        /// <summary>
        /// Try to set the value to black.
        /// </summary>
        public void TrySetBlack()
        {
            if (!this.black.HasValue)
            {
                this.black = true;
                return;
            }

            if (this.black.Value != true)
            {
                throw new Exception("Unable to set cell to black; is already white");
            }
        }

        /// <summary>
        /// Try to set the value to white.
        /// </summary>
        public void TrySetWhite()
        {
            if (!this.black.HasValue)
            {
                this.black = false;
                return;
            }

            if (this.black.Value != false)
            {
                throw new Exception("Unable to set cell to white; is already black");
            }
        }
    }
}
