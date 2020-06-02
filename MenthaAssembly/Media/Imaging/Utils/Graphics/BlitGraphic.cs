using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Primitives;

namespace MenthaAssembly
{
    public abstract partial class ImageContextBase<Pixel, Struct> : IImageContext
        where Pixel : unmanaged, IPixel
        where Struct : unmanaged, IPixelBase
    {
        protected void Blit(ImageContextBase<Pixel, Struct> Source, int X, int Y)
        {
            int x,
                y = 0;

            double ii,
                   jj = Y,
                   lastii = -1,
                   lastjj = -1,
                   sdx = Source.Width * 1d / this.Width,
                   sdy = Source.Height * 1d / this.Height;

            byte sa = 0,
                 da;

            Pixel SourcePixel;

            for (int j = 0; j < Height; j++)
            {
                if (y >= 0 && y < this.Height)
                {
                    ii = X;
                    x = 0;
                    SourcePixel = Source[0, 0];

                    // Pixel by pixel copying
                    for (int i = 0; i < Width; i++)
                    {
                        if (x >= 0 && x < this.Width)
                        {
                            if ((int)ii != lastii || (int)jj != lastjj)
                            {
                                if (0 <= ii && ii < Source.Width &&
                                    0 <= jj && jj < Source.Height)
                                {
                                    SourcePixel = Source[(int)ii, (int)jj];
                                    sa = SourcePixel.A;
                                }
                                else
                                {
                                    sa = 0;
                                }
                                lastii = ii;
                                lastjj = jj;
                            }

                            if (sa > 0)
                            {
                                Pixel DestPixel = this[x, y];
                                da = DestPixel.A;

                                if (sa == 255 || da == 0)
                                {
                                    this[x, y] = SourcePixel;
                                }
                                else
                                {
                                    int isa = 255 - sa;

                                    this[x, y] = ToPixel(da,
                                                               (byte)(((SourcePixel.R << 8) + isa * DestPixel.R) >> 8),
                                                               (byte)(((SourcePixel.G << 8) + isa * DestPixel.G) >> 8),
                                                               (byte)(((SourcePixel.B << 8) + isa * DestPixel.B) >> 8));
                                }
                            }
                        }
                        x++;
                        ii += sdx;
                    }
                }
                jj += sdy;
                y++;
            }

        }

        protected void Blit(Int32Bound destRect, ImageContextBase<Pixel, Struct> Source, Int32Bound sourceRect)
        {
            int dw = destRect.Width;
            int dh = destRect.Height;

            int px = destRect.Left;
            int py = destRect.Top;
            int x;
            int y;
            int idx;
            double ii;
            double jj;
            Pixel SourcePixel;
            int sa = 0;
            int da;

            double sdx = sourceRect.Width * 1d / destRect.Width;
            double sdy = sourceRect.Height * 1d / destRect.Height;
            int sourceStartX = sourceRect.Left;
            int sourceStartY = sourceRect.Top;
            int lastii, lastjj;
            lastii = -1;
            lastjj = -1;
            jj = sourceStartY;
            y = py;
            for (int j = 0; j < dh; j++)
            {
                if (y >= 0 && y < Height)
                {
                    ii = sourceStartX;
                    idx = px + y * Width;
                    x = px;
                    SourcePixel = Source.GetPixel(0, 0);

                    // Pixel by pixel copying
                    for (int i = 0; i < dw; i++)
                    {
                        if (x >= 0 && x < Width)
                        {
                            if ((int)ii != lastii || (int)jj != lastjj)
                            {
                                if (0 <= ii && ii < Source.Width &&
                                    0 <= jj && jj < Source.Height)
                                {
                                    SourcePixel = Source.GetPixel((int)ii, (int)jj);
                                    sa = SourcePixel.A;
                                }
                                else
                                {
                                    sa = 0;
                                }
                            }

                            if (sa > 0)
                            {
                                Pixel DestPixel = GetPixel(x, y);
                                da = DestPixel.A;
                                if (sa == 255 || da == 0)
                                {
                                    SetPixel(x, y, SourcePixel);
                                }
                                else
                                {
                                    int isa = 255 - sa;
                                    SetPixel(x, y, ToPixel((byte)da,
                                                                 (byte)(((SourcePixel.R << 8) + isa * DestPixel.R) >> 8),
                                                                 (byte)(((SourcePixel.G << 8) + isa * DestPixel.G) >> 8),
                                                                 (byte)(((SourcePixel.B << 8) + isa * DestPixel.B) >> 8)));
                                }
                            }
                        }
                        x++;
                        idx++;
                        ii += sdx;
                    }
                }
                jj += sdy;
                y++;
            }

        }


    }

}
