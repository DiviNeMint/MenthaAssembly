﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MenthaAssembly
{
    /// <summary>
    /// Represents a histogram of the specified type.
    /// </summary>
    [Serializable]
    public struct Histogram<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        private readonly T[] Datas;

        /// <summary>
        /// Gets the data's count.
        /// </summary>
        public int Count
            => Datas.Length;

        public T this[int Index]
            => Datas[Index];

        /// <summary>
        /// Gets the crests.
        /// </summary>
        public IEnumerable<HistogramCrest> Crests
            => EnumCrest();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="Datas">The datas of histogram</param>
        public Histogram(IEnumerable<T> Datas)
        {
            this.Datas = Datas is T[] DatasArray ? DatasArray : Datas.ToArray();
        }

        /// <summary>
        /// Smooths histogram.
        /// </summary>
        /// <param name="SmoothFunc">The method of smoothing.</param>
        public void Smooth(Func<T, T, T, T> SmoothFunc)
        {
            if (Datas.Length < 3)
                return;

            T[] Histo = Datas.ToArray();
            T Prev = Histo[0],
              Current = Prev,
              Next;

            int Length = Histo.Length - 1;
            for (int i = 1; i <= Length; i++)
            {
                Next = Histo[i];
                Datas[i - 1] = SmoothFunc(Prev, Current, Next);
                Prev = Current;
                Current = Next;
            }

            Datas[Length] = SmoothFunc(Prev, Current, Current);
        }

        private IEnumerable<HistogramCrest> EnumCrest()
        {
            int Length = Datas.Length;
            if (Length < 3)
                yield break;

            int Start = 0,
                Peak = 0,
                End;

            T Current = Datas[0],
              Next;

            int i = 1,
                Compare;
            while (i < Length)
            {
                // Peak
                for (; i < Length;)
                {
                    Next = Datas[i++];
                    Compare = Next.CompareTo(Current);
                    Current = Next;

                    if (Compare < 0)
                    {
                        Peak = (Peak + i - 1) >> 1;
                        break;
                    }
                    else if (Compare > 0)
                    {
                        Peak = i - 2;
                    }
                }

                if (i == Length)
                {
                    End = Length - 1;
                    yield return new HistogramCrest
                    {
                        Start = Start,
                        End = End,
                        Peak = End,
                    };
                    yield break;
                }

                End = i - 2;

                // End
                for (; i < Length;)
                {
                    Next = Datas[i++];
                    Compare = Current.CompareTo(Next);
                    Current = Next;

                    if (Compare < 0)
                    {
                        End = (End + i - 1) >> 1;
                        break;
                    }
                    else if (Compare > 0)
                    {
                        End = i - 2;
                    }
                }

                if (i == Length)
                {
                    yield return new HistogramCrest
                    {
                        Start = Start,
                        End = Length - 1,
                        Peak = Peak,
                    };
                    yield break;
                }
                else if (Start != End)
                {
                    yield return new HistogramCrest
                    {
                        Start = Start,
                        End = End,
                        Peak = Peak,
                    };

                    Start = End;
                    Peak = Start;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
            => ((IEnumerable<T>)Datas).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
            => Datas.GetEnumerator();

    }
}