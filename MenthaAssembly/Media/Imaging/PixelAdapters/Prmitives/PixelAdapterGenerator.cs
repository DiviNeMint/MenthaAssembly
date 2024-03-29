﻿using System;

namespace MenthaAssembly.Media.Imaging.Utils
{
    internal abstract class PixelAdapterGenerator
    {
        public static PixelAdapterGenerator Instance1 { get; } = new PixelAdapterGenerator1();

        public static PixelAdapterGenerator Instance3 { get; } = new PixelAdapterGenerator3();

        public static PixelAdapterGenerator Instance4 { get; } = new PixelAdapterGenerator4();

        public abstract PixelAdapter<U> GetAdapter<T, U>(IImageContext Context, int X, int Y)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixel;

        private sealed class PixelAdapterGenerator1 : PixelAdapterGenerator
        {
            public override PixelAdapter<U> GetAdapter<T, U>(IImageContext Context, int X, int Y)
            {
                Type PixelType = typeof(U);
                return Context.PixelType == PixelType ? new PixelAdapter1<U>(Context, X, Y) :
                                                        PixelHelper.IsCalculatedPixel(PixelType) ? new CastPixelAdapter<T, U>(new PixelAdapter1<T>(Context, X, Y)) :
                                                                                                   new PixelAdapter1<T, U>(Context, X, Y);
            }
        }

        private sealed class PixelAdapterGenerator3 : PixelAdapterGenerator
        {
            public override PixelAdapter<U> GetAdapter<T, U>(IImageContext Context, int X, int Y)
                => PixelHelper.IsCalculatedPixel(typeof(U)) ? new CastPixelAdapter<T, U>(new PixelAdapter3<T>(Context, X, Y)) :
                                                              new PixelAdapter3<U>(Context, X, Y);

        }

        private sealed class PixelAdapterGenerator4 : PixelAdapterGenerator
        {
            public override PixelAdapter<U> GetAdapter<T, U>(IImageContext Context, int X, int Y)
                => PixelHelper.IsCalculatedPixel(typeof(U)) ? new CastPixelAdapter<T, U>(new PixelAdapter4<T>(Context, X, Y)) :
                                                              new PixelAdapter4<U>(Context, X, Y);

        }
    }
}