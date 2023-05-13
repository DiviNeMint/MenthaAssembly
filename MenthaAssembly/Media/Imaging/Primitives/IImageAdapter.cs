using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    /// <summary>
    /// Represents the image adapter.
    /// </summary>
    public interface IImageAdapter : ICloneable
    {
        /// <summary>
        /// Gets the x-coordinate of this adapter.
        /// </summary>
        public int X { set; get; }

        /// <summary>
        /// Gets the y-coordinate of this adapter.
        /// </summary>
        public int Y { set; get; }

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

        /// <summary>
        /// Moves to the specified coordinate without checking.
        /// </summary>
        /// <param name="X">The specified x-coordinate.</param>
        /// <param name="Y">The specified y-coordinate.</param>
        public void DangerousMove(int X, int Y);

        /// <summary>
        /// Offsets the specified delta on the y-axis without checking.
        /// </summary>
        /// <param name="Delta">The specified delta.</param>
        public void DangerousOffsetX(int Delta);

        /// <summary>
        /// Offsets the specified delta on the y-axis without checking.
        /// </summary>
        /// <param name="Delta">The specified delta.</param>
        public void DangerousOffsetY(int Delta);

        /// <summary>
        /// Move to next on the x-axis. without checking.
        /// </summary>
        public void DangerousMoveNextX();

        /// <summary>
        /// Move to next on the y-axis. without checking.
        /// </summary>
        public void DangerousMoveNextY();

        /// <summary>
        /// Move to previous on the x-axis. without checking.
        /// </summary>
        public void DangerousMovePreviousX();

        /// <summary>
        /// Move to previous on the y-axis. without checking.
        /// </summary>
        public void DangerousMovePreviousY();

        /// <summary>
        /// Creates a new <see cref="IImageAdapter"/> that is a copy of the current instance.
        /// </summary>
        public new IImageAdapter Clone();

    }
}