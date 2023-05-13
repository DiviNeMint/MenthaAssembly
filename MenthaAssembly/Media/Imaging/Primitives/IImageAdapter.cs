namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the image adapter.
    /// </summary>
    public interface IImageAdapter
    {
        /// <summary>
        /// Gets the x-coordinate of this adapter.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the y-coordinate of this adapter.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Gets the length of this adapter on x-axis.
        /// </summary>
        public int XLength { get; }

        /// <summary>
        /// Gets the length of this adapter on y-axis.
        /// </summary>
        public int YLength { get; }

        /// <summary>
        /// Moves to the specified coordinate.
        /// </summary>
        /// <param name="X">The specified x-coordinate.</param>
        /// <param name="Y">The specified y-coordinate.</param>
        public void Move(int X, int Y);

        /// <summary>
        /// Offsets the specified delta on the y-axis.
        /// </summary>
        /// <param name="Delta">The specified delta.</param>
        public void OffsetX(int Delta);

        /// <summary>
        /// Offsets the specified delta on the y-axis.
        /// </summary>
        /// <param name="Delta">The specified delta.</param>
        public void OffsetY(int Delta);

        /// <summary>
        /// Move to next on the x-axis.
        /// </summary>
        public void MoveNextX();

        /// <summary>
        /// Move to next on the y-axis.
        /// </summary>
        public void MoveNextY();

        /// <summary>
        /// Move to previous on the x-axis.
        /// </summary>
        public void MovePreviousX();

        /// <summary>
        /// Move to previous on the y-axis.
        /// </summary>
        public void MovePreviousY();

    }

}