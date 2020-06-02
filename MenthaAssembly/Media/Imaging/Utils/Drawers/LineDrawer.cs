using MenthaAssembly.Media.Imaging.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal static class LineDrawer
    {

        public static IEnumerable<Int32Vector> LoopNextWidthDelta(int AbsDeltaX, int DeltaY, int AbsDeltaY, int Step)
        {
            int X = 0,
                Y = 0,
                StepX = Step,
                StepY = DeltaY > 0 ? Step : -Step; ;

            if (AbsDeltaX == 0)
            {
                do
                {
                    Y += StepY;
                    yield return new Int32Vector(X, Y);
                } while (true);
            }
            else if (AbsDeltaY == 0)
            {
                do
                {
                    X += StepX;
                    yield return new Int32Vector(X, Y);
                } while (true);
            }
            else if (AbsDeltaX < AbsDeltaY)
            {
                int Remainer = AbsDeltaY >> 1;
                do
                {
                    Remainer -= AbsDeltaX;
                    if (Remainer < 0)
                    {
                        Remainer += AbsDeltaY;
                        X += StepX;
                    }
                    Y += StepY;

                    yield return new Int32Vector(X, Y);
                } while (true);
            }
            else
            {
                int Remainer = AbsDeltaX >> 1;
                do
                {
                    Remainer -= AbsDeltaY;
                    if (Remainer < 0)
                    {
                        Remainer += AbsDeltaX;
                        Y += StepY;
                    }
                    X += StepX;

                    yield return new Int32Vector(X, Y);
                } while (true);
            }
        }

        public static IEnumerable<Int32Point> CalculateBoundKeyPoint(int X0, int Y0, int X1, int Y1, int DeltaX, int AbsDeltaX, int AbsDeltaY, int Step)
        {
            int StepX = DeltaX > 0 ? Step : -Step,
                StepY = Step;

            if (AbsDeltaX > AbsDeltaY)
            {
                int Remainer = AbsDeltaX >> 1,
                    LastX = X0, LastY = Y0;

                if (Remainer >= AbsDeltaY)
                    yield return new Int32Point(X0, Y0);

                if (X0 > X1)
                {
                    do
                    {
                        Remainer -= AbsDeltaY;
                        if (Remainer < 0)
                        {
                            LastX = X0;
                            LastY = Y0;
                            yield return new Int32Point(X0, Y0);
                            Remainer += AbsDeltaX;
                            Y0 += StepY;
                        }
                        X0 += StepX;

                    } while (X0 >= X1);
                }
                else
                {
                    do
                    {
                        Remainer -= AbsDeltaY;
                        if (Remainer < 0)
                        {
                            LastX = X0;
                            LastY = Y0;
                            yield return new Int32Point(X0, Y0);
                            Remainer += AbsDeltaX;
                            Y0 += StepY;
                        }
                        X0 += StepX;
                    } while (X0 <= X1);
                }

                if (LastX != X1 && LastY != Y1)
                    yield return new Int32Point(X1, Y1);
            }
            else
            {
                int Remainer = AbsDeltaY >> 1;
                do
                {
                    yield return new Int32Point(X0, Y0);
                    Remainer -= AbsDeltaX;
                    if (Remainer < 0)
                    {
                        Remainer += AbsDeltaY;
                        X0 += StepX;
                    }
                    Y0 += StepY;
                } while (Y0 <= Y1);
            }
        }

    }

}
