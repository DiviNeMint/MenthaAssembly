using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Media.Imaging.Utils
{
    public static class TemplateMatchAlgorithm
    {
        /// <summary>
        /// Finds all matches of a template within an image.
        /// </summary>
        /// <param name="Image">The source image to search in.</param>
        /// <param name="Template">The template image to match.</param>
        /// <param name="Channel">
        /// The channel to use for matching. <para/>
        /// If set to <see cref="ImageChannel.All"/>, the image is converted to grayscale.
        /// </param>
        /// <param name="Options">Options controlling matching behavior (e.g., threshold, search region, method).</param>
        /// <returns>
        /// A collection of <see cref="TemplateMatchResult"/> containing the positions and scores of all matches.
        /// </returns>
        public static IEnumerable<TemplateMatchResult> MatchTemplate(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, TemplateMatchOptions Options)
        {
            MatchCache tplCache = new(),
                       imgCache = new();

            int Tw = Template.XLength;
            int Th = Template.YLength;

            bool EnabledPreFilter = Options.PreFilter.Enabled;
            TemplateMatchMode Mode = Options.Mode;
            if (Mode == TemplateMatchMode.Auto)
                DecideModeAndFilter(Image.XLength, Image.YLength, Tw, Th, ref EnabledPreFilter, ref Mode);

            bool IsAlphaTemplate = !PixelHelper.IsNonAlphaPixel(Template.PixelType);
            Func<IPixelAdapter, IPixelAdapter, ImageChannel, double, MatchCache, MatchCache, IEnumerable<(int, int, double)>> Matcher = Mode switch
            {
                TemplateMatchMode.SlidingWindow => IsAlphaTemplate ? MatchTemplateSpatialWithMask : MatchTemplateSpatial,
                TemplateMatchMode.Fourier => IsAlphaTemplate ? MatchTemplateFFTWithMask : MatchTemplateFFT,
                _ => throw new NotSupportedException($"Not supported template match mode: {Mode}."),
            };

            if (EnabledPreFilter)
            {
                double Threshold = Options.Threshold;
                (int X, int Y)[] Candidates = [.. FilterCandidates(Image, Template, Channel, Options.PreFilter, imgCache, tplCache)];
                foreach ((int Lx, int Ty, int Rx, int By) in MergeCandidates(Candidates, 2))
                {
                    double BScore = double.NegativeInfinity;
                    int BMx = -1,
                        BMy = -1;

                    CropPixelAdapter CroppedImage = new(Image, Lx, Ty, Tw + Rx - Lx + 2, Th + By - Ty + 2);
                    foreach ((int Mx, int My, double Score) in Matcher(CroppedImage, Template, Channel, Options.Threshold, new MatchCache(), tplCache))
                    {
                        if (BScore < Score)
                        {
                            BScore = Score;
                            BMx = Mx;
                            BMy = My;
                        }
                    }

                    if (!double.IsNegativeInfinity(BScore))
                        yield return new TemplateMatchResult(Lx + BMx, Ty + BMy, BScore);
                }
            }
            else
            {
                foreach ((int X, int Y, double Score) in Matcher(Image, Template, Channel, Options.Threshold, imgCache, tplCache))
                    yield return new TemplateMatchResult(X, Y, Score);
            }
        }
        public static IEnumerable<TemplateMatchResult> MatchTemplate(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, TemplateMatchOptions Options, ParallelOptions ParallelOptions)
        {
            MatchCache tplCache = new(),
                       imgCache = new();

            int Tw = Template.XLength;
            int Th = Template.YLength;

            bool EnabledPreFilter = Options.PreFilter.Enabled;
            TemplateMatchMode Mode = Options.Mode;
            if (Mode == TemplateMatchMode.Auto)
                DecideModeAndFilter(Image.XLength, Image.YLength, Tw, Th, ref EnabledPreFilter, ref Mode);

            bool IsAlphaTemplate = !PixelHelper.IsNonAlphaPixel(Template.PixelType);
            Func<IPixelAdapter, IPixelAdapter, ImageChannel, double, MatchCache, MatchCache, ParallelOptions, IEnumerable<(int, int, double)>> Matcher = Mode switch
            {
                TemplateMatchMode.SlidingWindow => IsAlphaTemplate ? MatchTemplateSpatialWithMask : MatchTemplateSpatial,
                TemplateMatchMode.Fourier => IsAlphaTemplate ? MatchTemplateFFTWithMask : MatchTemplateFFT,
                _ => throw new NotSupportedException($"Not supported template match mode: {Mode}."),
            };

            if (EnabledPreFilter)
            {
                double Threshold = Options.Threshold;
                (int X, int Y)[] Candidates = [.. FilterCandidates(Image, Template, Channel, Options.PreFilter, imgCache, tplCache)];
                foreach ((int Lx, int Ty, int Rx, int By) in MergeCandidates(Candidates, 2))
                {
                    double BScore = double.NegativeInfinity;
                    int BMx = -1,
                        BMy = -1;

                    CropPixelAdapter CroppedImage = new(Image, Lx, Ty, Tw + Rx - Lx + 2, Th + By - Ty + 2);
                    foreach ((int Mx, int My, double Score) in Matcher(CroppedImage, Template, Channel, Options.Threshold, new MatchCache(), tplCache, ParallelOptions))
                    {
                        if (BScore < Score)
                        {
                            BScore = Score;
                            BMx = Mx;
                            BMy = My;
                        }
                    }

                    if (!double.IsNegativeInfinity(BScore))
                        yield return new TemplateMatchResult(Lx + BMx, Ty + BMy, BScore);
                }
            }
            else
            {
                foreach ((int X, int Y, double Score) in Matcher(Image, Template, Channel, Options.Threshold, imgCache, tplCache, ParallelOptions))
                    yield return new TemplateMatchResult(X, Y, Score);
            }
        }
        private static void DecideModeAndFilter(int Iw, int Ih, int Tw, int Th, ref bool EnabledPreFilter, ref TemplateMatchMode Mode)
        {
            int FFTw = NextPowerOfTwo(Iw + Tw);
            int FFTh = NextPowerOfTwo(Ih + Th);
            long FFTSize = (long)FFTw * FFTh;

            long CandidateCount = (long)(Iw - Tw + 1) * (Ih - Th + 1);
            long tplSize = (long)Tw * Th;

            // 規則 1: 候選數量非常小 or 模板很小 → SlidingWindow
            if (CandidateCount <= 1e5 || tplSize <= 1024)
            {
                Mode = TemplateMatchMode.SlidingWindow;
            }
            // 規則 2: 模板很大 + 影像也大 → Fourier
            else if (tplSize >= 16384 && CandidateCount >= 1e6)
            {
                Mode = TemplateMatchMode.Fourier;
            }
            else
            {
                // 規則 3: 用 ratio 決定
#if NET6_0_OR_GREATER
                double ratio = CandidateCount * tplSize / (FFTSize * Math.Log2(FFTSize));
#else
                double ratio = CandidateCount * tplSize / (FFTSize * Math.Log(FFTSize, 2));
#endif
                if (ratio < 0.8)
                    Mode = TemplateMatchMode.SlidingWindow;
                else if (ratio > 2.0)
                    Mode = TemplateMatchMode.Fourier;

                // 規則 4: 模糊區域 → 根據候選數決定
                else
                    Mode = (CandidateCount > 5e5) ? TemplateMatchMode.Fourier : TemplateMatchMode.SlidingWindow;
            }

            EnabledPreFilter = CandidateCount >= 1e6 || !(CandidateCount <= 4 || Mode == TemplateMatchMode.Fourier || tplSize < 1024 || Iw * Ih < 16384);
        }

        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateFFT(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;
            int Tw = Template.XLength;
            int Th = Template.YLength;
            int FFTw = NextPowerOfTwo(Iw + Tw);
            int FFTh = NextPowerOfTwo(Ih + Th);

            // ===============================
            //             模板資訊
            // ===============================
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            double[,] tplData = tplCache.Data;

            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        tplMean += Value;
                        tplVar += Value * Value;
                        tplPixelCount++;
                    }
                }

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Data = tplData;
            }
            else if (tplData is null)
            {
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                        tplData[i, j] = TExtractValue(TAdt);
                }

                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            double[,] tplReal = new double[FFTh, FFTw];
            double[,] tplImag = new double[FFTh, FFTw];
            for (int j = 0; j < Th; j++)
                for (int i = 0; i < Tw; i++)
                    tplReal[Th - 1 - j, Tw - 1 - i] = tplData[i, j] - tplMean;

            // ===============================
            //             影像資訊
            // ===============================
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            double[,] imgReal = new double[FFTh, FFTw];
            double[,] imgImag = new double[FFTh, FFTw];

            if (Integral is null || SquaredIntegral is null)
            {
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        SquaredrowSum += Value * Value;

                        imgReal[j, i - 1] = Value;
                        Integral[j + 1, i] = Integral[j, i] + RowSum;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }
            else if (imgCache.Data is double[,] imgData)
            {
                for (int j = 0; j < Ih; j++)
                    for (int i = 0; i < Iw; i++)
                        imgReal[j, i] = imgData[i, j];
            }
            else
            {
                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                        imgReal[j, i] = IExtractValue(IAdt);
                }
            }

            // ==============================
            //             FFT
            // ==============================
            FFT2D(imgReal, imgImag, false);
            FFT2D(tplReal, tplImag, false);

            // 頻域點乘
            for (int y = 0; y < FFTh; y++)
                for (int x = 0; x < FFTw; x++)
                    Dot(imgReal[y, x], imgImag[y, x], tplReal[y, x], tplImag[y, x], ref imgReal[y, x], ref imgImag[y, x]);

            // 逆 FFT
            FFT2D(imgReal, imgImag, true);

            // ==============================
            //           計算相似度
            // ==============================
            for (int y = 0; y <= Ih - Th; y++)
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    long sum = GetRegionSum(Integral, x, y, Tw, Th);
                    long sumSq = GetRegionSum(SquaredIntegral, x, y, Tw, Th);
                    double imgMean = sum / (double)tplPixelCount;
                    double imgVar = sumSq - 2 * imgMean * sum + imgMean * imgMean * tplPixelCount;

                    double numerator = imgReal[y + Th - 1, x + Tw - 1];
                    double score = (imgVar == 0) ? 0 : numerator / Math.Sqrt(tplVar * imgVar);

                    if (Threshold < score)
                        yield return (x, y, score);
                }
            }
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateFFT(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache, ParallelOptions ParallelOptions)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;
            int Tw = Template.XLength;
            int Th = Template.YLength;
            int FFTw = NextPowerOfTwo(Iw + Tw);
            int FFTh = NextPowerOfTwo(Ih + Th);

            // ===============================
            //             模板資訊
            // ===============================
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            double[,] tplData = tplCache.Data;

            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions,
                () => (0.0, 0.0, 0),
                (j, loopState, local) =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        local.Item1 += Value;
                        local.Item2 += Value * Value;
                        local.Item3++;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplMean, tplMean + local.Item1);
                    Interlocked.Exchange(ref tplVar, tplVar + local.Item2);
                    Interlocked.Add(ref tplPixelCount, local.Item3);
                });

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Data = tplData;
            }
            else if (tplData is null)
            {
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions, j =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                        tplData[i, j] = TExtractValue(TAdt);
                });

                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            double[,] tplReal = new double[FFTh, FFTw];
            double[,] tplImag = new double[FFTh, FFTw];
            Parallel.For(0, Th, ParallelOptions, j =>
            {
                for (int i = 0; i < Tw; i++)
                    tplReal[Th - 1 - j, Tw - 1 - i] = tplData[i, j] - tplMean;
            });

            // ===============================
            //             影像資訊
            // ===============================
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            double[,] imgReal = new double[FFTh, FFTw];
            double[,] imgImag = new double[FFTh, FFTw];

            if (Integral is null || SquaredIntegral is null)
            {
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        SquaredrowSum += Value * Value;

                        imgReal[j, i - 1] = Value;
                        Integral[j + 1, i] = Integral[j, i] + RowSum;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }
            else if (imgCache.Data is double[,] imgData)
            {
                Parallel.For(0, Ih, ParallelOptions, j =>
                {
                    for (int i = 0; i < Iw; i++)
                        imgReal[j, i] = imgData[i, j];
                });
            }
            else
            {
                Parallel.For(0, Ih, ParallelOptions, j =>
                {
                    IPixelAdapter IAdt = ICreateAdapter();
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                        imgReal[j, i] = IExtractValue(IAdt);
                });
            }

            // ==============================
            //             FFT
            // ==============================
            FFT2D(imgReal, imgImag, false, ParallelOptions);
            FFT2D(tplReal, tplImag, false, ParallelOptions);

            // 頻域點乘
            Parallel.For(0, FFTh, ParallelOptions, y =>
            {
                for (int x = 0; x < FFTw; x++)
                    Dot(imgReal[y, x], imgImag[y, x], tplReal[y, x], tplImag[y, x], ref imgReal[y, x], ref imgImag[y, x]);
            });

            // 逆 FFT
            FFT2D(imgReal, imgImag, true, ParallelOptions);

            // ==============================
            //           計算相似度
            // ==============================
            ConcurrentBag<(int X, int Y, double Score)> Results = [];
            Parallel.For(0, Ih - Th + 1, ParallelOptions, y =>
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    long sum = GetRegionSum(Integral, x, y, Tw, Th);
                    long sumSq = GetRegionSum(SquaredIntegral, x, y, Tw, Th);
                    double imgMean = sum / (double)tplPixelCount;
                    double imgVar = sumSq - 2 * imgMean * sum + imgMean * imgMean * tplPixelCount;

                    double numerator = imgReal[y + Th - 1, x + Tw - 1];
                    double score = (imgVar == 0) ? 0 : numerator / Math.Sqrt(tplVar * imgVar);

                    if (Threshold < score)
                        Results.Add((x, y, score));
                }
            });

            return Results;
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateFFTWithMask(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;
            int Tw = Template.XLength;
            int Th = Template.YLength;
            int FFTw = NextPowerOfTwo(Iw + Tw);
            int FFTh = NextPowerOfTwo(Ih + Th);

            // ===============================
            //             模板資訊
            // ===============================
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            bool[,] tplMask = tplCache.Mask;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;

                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        tplMean += Value;
                        tplVar += Value * Value;
                        tplPixelCount++;
                    }
                }

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }
            else if (tplMask is null || tplData is null)
            {
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;
                        tplData[i, j] = TExtractValue(TAdt);
                    }
                }

                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            double[,] tplReal = new double[FFTh, FFTw];
            double[,] tplImag = new double[FFTh, FFTw];
            double[,] tplMaskReal = new double[FFTh, FFTw];
            double[,] tplMaskImag = new double[FFTh, FFTw];
            for (int j = 0; j < Th; j++)
            {
                for (int i = 0; i < Tw; i++)
                {
                    if (tplMask[i, j])
                    {
                        tplReal[Th - 1 - j, Tw - 1 - i] = tplData[i, j] - tplMean;
                        tplMaskReal[Th - 1 - j, Tw - 1 - i] = 1.0;
                    }
                }
            }

            // ===============================
            //             影像資訊
            // ===============================
            double[,] imgReal = new double[FFTh, FFTw];
            double[,] imgImag = new double[FFTh, FFTw];
            double[,] imgSquaredReal = new double[FFTh, FFTw];
            double[,] imgSquaredImag = new double[FFTh, FFTw];
            if (imgCache.Data is double[,] imgData)
            {
                for (int j = 0; j < Ih; j++)
                {
                    for (int i = 0; i < Iw; i++)
                    {
                        double Value = imgData[i, j];
                        imgReal[j, i] = Value;
                        imgSquaredReal[j, i] = Value * Value;
                    }
                }
            }
            else
            {
                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        double Value = IExtractValue(IAdt);
                        imgReal[j, i] = Value;
                        imgSquaredReal[j, i] = Value * Value;
                    }
                }
            }

            // ==============================
            //             FFT
            // ==============================
            FFT2D(imgReal, imgImag, false);
            FFT2D(imgSquaredReal, imgSquaredImag, false);
            FFT2D(tplReal, tplImag, false);
            FFT2D(tplMaskReal, tplMaskImag, false);

            // 頻域點乘
            //double[,] convNumerFreqReal = new double[FFTh, FFTw];
            //double[,] convNumerFreqImag = new double[FFTh, FFTw];
            //double[,] convSumFreqReal = new double[FFTh, FFTw];
            //double[,] convSumFreqImag = new double[FFTh, FFTw];
            for (int y = 0; y < FFTh; y++)
            {
                for (int x = 0; x < FFTw; x++)
                {
                    Dot(imgReal[y, x], imgImag[y, x], tplReal[y, x], tplImag[y, x], ref tplReal[y, x], ref tplImag[y, x]);
                    Dot(imgReal[y, x], imgImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref imgReal[y, x], ref imgImag[y, x]);
                    Dot(imgSquaredReal[y, x], imgSquaredImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref imgSquaredReal[y, x], ref imgSquaredImag[y, x]);
                }
            }

            // 逆 FFT
            FFT2D(tplReal, tplImag, true);
            FFT2D(imgReal, imgImag, true);
            FFT2D(imgSquaredReal, imgSquaredImag, true);

            // 節省不必要的陣列宣告
            //double[,] convNumerFreqReal = new double[FFTh, FFTw];
            //double[,] convNumerFreqImag = new double[FFTh, FFTw];
            //double[,] convSumFreqReal = new double[FFTh, FFTw];
            //double[,] convSumFreqImag = new double[FFTh, FFTw];
            //double[,] convSumSqFreqReal = new double[FFTh, FFTw];
            //double[,] convSumSqFreqImag = new double[FFTh, FFTw];
            //for (int y = 0; y < FFTh; y++)
            //{
            //    for (int x = 0; x < FFTw; x++)
            //    {
            //        Dot(imgReal[y, x], imgImag[y, x], tplReal[y, x], tplImag[y, x], ref convNumerFreqReal[y, x], ref convNumerFreqImag[y, x]);
            //        Dot(imgReal[y, x], imgImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref convSumFreqReal[y, x], ref convSumFreqImag[y, x]);
            //        Dot(imgSquaredReal[y, x], imgSquaredImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref convSumSqFreqReal[y, x], ref convSumSqFreqImag[y, x]);
            //    }
            //}
            //FFT2D(convNumerFreqReal, convNumerFreqImag, true);
            //FFT2D(convSumFreqReal, convSumFreqImag, true);
            //FFT2D(convSumSqFreqReal, convSumSqFreqImag, true);

            // ==============================
            //           計算相似度
            // ==============================
            for (int y = 0; y <= Ih - Th; y++)
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    int ry = y + Th - 1;
                    int rx = x + Tw - 1;

                    double numerator = tplReal[ry, rx];      // Σ (image * (g - mean) * mask)
                    double sumWeighted = imgReal[ry, rx];      // Σ (image * mask)
                    double sumSqWeighted = imgSquaredReal[ry, rx];  // Σ (image^2 * mask)

                    // 節省不必要的陣列宣告
                    //double numerator = convNumerFreqReal[ry, rx];    
                    //double sumWeighted = convSumFreqReal[ry, rx];    
                    //double sumSqWeighted = convSumSqFreqReal[ry, rx];

                    double imgMean = sumWeighted / tplPixelCount;
                    double imgVar = sumSqWeighted - 2.0 * imgMean * sumWeighted + imgMean * imgMean * tplPixelCount;

                    double score = (imgVar <= 1e-12) ? 0.0 : numerator / Math.Sqrt(tplVar * imgVar);
                    if (Threshold < score)
                        yield return (x, y, score);
                }
            }
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateFFTWithMask(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache, ParallelOptions ParallelOptions)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;
            int Tw = Template.XLength;
            int Th = Template.YLength;
            int FFTw = NextPowerOfTwo(Iw + Tw);
            int FFTh = NextPowerOfTwo(Ih + Th);

            // ===============================
            //             模板資訊
            // ===============================
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            bool[,] tplMask = tplCache.Mask;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions,
                () => (0.0, 0.0, 0),
                (j, loopState, local) =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;

                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        local.Item1 += Value;
                        local.Item2 += Value * Value;
                        local.Item3++;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplMean, tplMean + local.Item1);
                    Interlocked.Exchange(ref tplVar, tplVar + local.Item2);
                    Interlocked.Add(ref tplPixelCount, local.Item3);
                });

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }
            else if (tplMask is null || tplData is null)
            {
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions, j =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;
                        tplData[i, j] = TExtractValue(TAdt);
                    }
                });

                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            double[,] tplReal = new double[FFTh, FFTw];
            double[,] tplImag = new double[FFTh, FFTw];
            double[,] tplMaskReal = new double[FFTh, FFTw];
            double[,] tplMaskImag = new double[FFTh, FFTw];
            Parallel.For(0, Th, ParallelOptions, j =>
            {
                for (int i = 0; i < Tw; i++)
                {
                    if (tplMask[i, j])
                    {
                        tplReal[Th - 1 - j, Tw - 1 - i] = tplData[i, j] - tplMean;
                        tplMaskReal[Th - 1 - j, Tw - 1 - i] = 1.0;
                    }
                }
            });

            // ===============================
            //             影像資訊
            // ===============================
            double[,] imgReal = new double[FFTh, FFTw];
            double[,] imgImag = new double[FFTh, FFTw];
            double[,] imgSquaredReal = new double[FFTh, FFTw];
            double[,] imgSquaredImag = new double[FFTh, FFTw];
            if (imgCache.Data is double[,] imgData)
            {
                Parallel.For(0, Ih, ParallelOptions, j =>
                {
                    for (int i = 0; i < Iw; i++)
                    {
                        double Value = imgData[i, j];
                        imgReal[j, i] = Value;
                        imgSquaredReal[j, i] = Value * Value;
                    }
                });
            }
            else
            {
                Parallel.For(0, Ih, ParallelOptions, j =>
                {
                    IPixelAdapter IAdt = ICreateAdapter();
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        double Value = IExtractValue(IAdt);
                        imgReal[j, i] = Value;
                        imgSquaredReal[j, i] = Value * Value;
                    }
                });
            }

            // ==============================
            //             FFT
            // ==============================
            FFT2D(imgReal, imgImag, false, ParallelOptions);
            FFT2D(imgSquaredReal, imgSquaredImag, false, ParallelOptions);
            FFT2D(tplReal, tplImag, false, ParallelOptions);
            FFT2D(tplMaskReal, tplMaskImag, false, ParallelOptions);

            Parallel.For(0, FFTh, ParallelOptions, y =>
            {
                for (int x = 0; x < FFTw; x++)
                {
                    Dot(imgReal[y, x], imgImag[y, x], tplReal[y, x], tplImag[y, x], ref tplReal[y, x], ref tplImag[y, x]);
                    Dot(imgReal[y, x], imgImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref imgReal[y, x], ref imgImag[y, x]);
                    Dot(imgSquaredReal[y, x], imgSquaredImag[y, x], tplMaskReal[y, x], tplMaskImag[y, x], ref imgSquaredReal[y, x], ref imgSquaredImag[y, x]);
                }
            });

            // 逆 FFT
            FFT2D(tplReal, tplImag, true, ParallelOptions);
            FFT2D(imgReal, imgImag, true, ParallelOptions);
            FFT2D(imgSquaredReal, imgSquaredImag, true, ParallelOptions);

            // ==============================
            //           計算相似度
            // ==============================
            ConcurrentBag<(int X, int Y, double Score)> Results = [];
            Parallel.For(0, Ih - Th + 1, ParallelOptions, y =>
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    int ry = y + Th - 1;
                    int rx = x + Tw - 1;

                    double numerator = tplReal[ry, rx];      // Σ (image * (g - mean) * mask)
                    double sumWeighted = imgReal[ry, rx];      // Σ (image * mask)
                    double sumSqWeighted = imgSquaredReal[ry, rx];  // Σ (image^2 * mask)

                    double imgMean = sumWeighted / tplPixelCount;
                    double imgVar = sumSqWeighted - 2.0 * imgMean * sumWeighted + imgMean * imgMean * tplPixelCount;

                    double score = (imgVar <= 1e-12) ? 0.0 : numerator / Math.Sqrt(tplVar * imgVar);
                    if (Threshold < score)
                        Results.Add((x, y, score));
                }
            });

            return Results;
        }

        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateSpatial(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache)
        {
            CreateAccessors(Image, ImageChannel.All, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, ImageChannel.All, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        tplMean += Value;
                        tplVar += Value * Value;
                        tplPixelCount++;
                    }
                }

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Data = tplData;
            }
            else if (tplData is null)
            {
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                        tplData[i, j] = TExtractValue(TAdt);
                }

                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // ===============================
            //             影像資訊
            // ===============================
            int Iw = Image.XLength;
            int Ih = Image.YLength;
            double[,] imgData = imgCache.Data;
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            if (imgData is null || Integral is null || SquaredIntegral is null)
            {
                imgData = new double[Iw, Ih];
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        SquaredrowSum += Value * Value;

                        imgData[i - 1, j] = Value;
                        Integral[j + 1, i] = Integral[j, i] + RowSum;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Data = imgData;
                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }

            // ==============================
            //           計算相似度
            // ==============================
            for (int y = 0; y <= Ih - Th; y++)
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    double sumI = GetRegionSum(Integral, x, y, Tw, Th);
                    double sumI2 = GetRegionSum(SquaredIntegral, x, y, Tw, Th);

                    double cross = 0;
                    for (int j = 0; j < Th; j++)
                        for (int i = 0; i < Tw; i++)
                            cross += imgData[x + i, y + j] * tplData[i, j];

                    double imgMean = sumI / tplPixelCount;
                    double imgVar = sumI2 - imgMean * imgMean * tplPixelCount;

                    double numerator = cross - sumI * tplMean;
                    double denominator = Math.Sqrt(tplVar * imgVar);

                    double score = (denominator <= 1e-8) ? 0 : numerator / denominator;
                    if (Threshold < score)
                        yield return (x, y, score);
                }
            }
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateSpatial(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache, ParallelOptions ParallelOptions)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差、遮罩
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions,
                () => (0.0, 0.0, 0),
                (j, loopState, local) =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        local.Item1 += Value;
                        local.Item2 += Value * Value;
                        local.Item3++;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplMean, tplMean + local.Item1);
                    Interlocked.Exchange(ref tplVar, tplVar + local.Item2);
                    Interlocked.Add(ref tplPixelCount, local.Item3);
                });

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Data = tplData;
            }
            else if (tplData is null)
            {
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions, j =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                        tplData[i, j] = TExtractValue(TAdt);
                });

                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // ===============================
            //             影像資訊
            // ===============================
            int Iw = Image.XLength;
            int Ih = Image.YLength;
            double[,] imgData = imgCache.Data;
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            if (imgData is null || Integral is null || SquaredIntegral is null)
            {
                imgData = new double[Iw, Ih];
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        SquaredrowSum += Value * Value;

                        imgData[i - 1, j] = Value;
                        Integral[j + 1, i] = Integral[j, i] + RowSum;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Data = imgData;
                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }

            // ==============================
            //           計算相似度
            // ==============================
            ConcurrentBag<(int X, int Y, double Score)> Results = [];
            Parallel.For(0, Ih - Th + 1, ParallelOptions, y =>
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    double sumI = GetRegionSum(Integral, x, y, Tw, Th);
                    double sumI2 = GetRegionSum(SquaredIntegral, x, y, Tw, Th);

                    double cross = 0;
                    for (int j = 0; j < Th; j++)
                        for (int i = 0; i < Tw; i++)
                            cross += imgData[x + i, y + j] * tplData[i, j];

                    double imgMean = sumI / tplPixelCount;
                    double imgVar = sumI2 - imgMean * imgMean * tplPixelCount;

                    double numerator = cross - sumI * tplMean;
                    double denominator = Math.Sqrt(tplVar * imgVar);

                    double score = (denominator <= 1e-8) ? 0 : numerator / denominator;
                    if (Threshold < score)
                        Results.Add((x, y, score));
                }
            });

            return Results;
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateSpatialWithMask(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差、遮罩
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            bool[,] tplMask = tplCache.Mask;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;

                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        tplMean += Value;
                        tplVar += Value * Value;
                        tplPixelCount++;
                    }
                }

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }
            else if (tplMask is null || tplData is null)
            {
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;
                        tplData[i, j] = TExtractValue(TAdt);
                    }
                }

                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // ===============================
            //             影像資訊
            // ===============================
            int Iw = Image.XLength;
            int Ih = Image.YLength;
            double[,] imgData = imgCache.Data;
            if (imgData is null)
            {
                imgData = new double[Iw, Ih];
                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        double Value = IExtractValue(IAdt);
                        imgData[i, j] = Value;
                    }
                }
                imgCache.Data = imgData;
            }

            // ==============================
            //           計算相似度
            // ==============================
            for (int y = 0; y <= Ih - Th; y++)
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    // Σ I, Σ I²（僅 Mask 範圍）
                    double sumI = 0;
                    double sumI2 = 0;
                    double cross = 0;

                    for (int j = 0; j < Th; j++)
                    {
                        for (int i = 0; i < Tw; i++)
                        {
                            if (!tplMask[i, j])
                                continue;

                            double Iv = imgData[x + i, y + j];
                            sumI += Iv;
                            sumI2 += Iv * Iv;
                            cross += Iv * tplData[i, j];
                        }
                    }

                    double imgMean = sumI / tplPixelCount;
                    double imgVar = sumI2 - imgMean * imgMean * tplPixelCount;

                    double numerator = cross - sumI * tplMean;
                    double denominator = Math.Sqrt(tplVar * imgVar);

                    double score = (denominator <= 1e-8) ? 0 : numerator / denominator;
                    if (Threshold < score)
                        yield return (x, y, score);
                }

            }
        }
        internal static IEnumerable<(int X, int Y, double Score)> MatchTemplateSpatialWithMask(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, double Threshold, MatchCache imgCache, MatchCache tplCache, ParallelOptions ParallelOptions)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差、遮罩
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount = tplCache.ValidPixelCount;
            bool[,] tplMask = tplCache.Mask;
            double[,] tplData = tplCache.Data;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar) || tplPixelCount < 0)
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions,
                () => (0.0, 0.0, 0),
                (j, loopState, local) =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;

                        double Value = TExtractValue(TAdt);
                        tplData[i, j] = Value;

                        local.Item1 += Value;
                        local.Item2 += Value * Value;
                        local.Item3++;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplMean, tplMean + local.Item1);
                    Interlocked.Exchange(ref tplVar, tplVar + local.Item2);
                    Interlocked.Add(ref tplPixelCount, local.Item3);
                });

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }
            else if (tplMask is null || tplData is null)
            {
                tplMask = new bool[Tw, Th];
                tplData = new double[Tw, Th];

                Parallel.For(0, Th, ParallelOptions, j =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        tplMask[i, j] = true;
                        tplData[i, j] = TExtractValue(TAdt);
                    }
                });

                tplCache.Mask = tplMask;
                tplCache.Data = tplData;
            }

            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // ===============================
            //             影像資訊
            // ===============================
            int Iw = Image.XLength;
            int Ih = Image.YLength;
            double[,] imgData = imgCache.Data;
            if (imgData is null)
            {
                imgData = new double[Iw, Ih];
                Parallel.For(0, Ih, ParallelOptions, j =>
                {
                    IPixelAdapter IAdt = ICreateAdapter();
                    IAdt.DangerousMove(0, j);
                    for (int i = 0; i < Iw; i++, IAdt.DangerousMoveNextX())
                        imgData[i, j] = IExtractValue(IAdt);
                });

                imgCache.Data = imgData;
            }

            // ==============================
            //           計算相似度
            // ==============================
            ConcurrentBag<(int, int, double)> Results = [];
            Parallel.For(0, Ih - Th + 1, ParallelOptions, y =>
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    // Σ I, Σ I²（僅 Mask 範圍）
                    double sumI = 0;
                    double sumI2 = 0;
                    double cross = 0;

                    for (int j = 0; j < Th; j++)
                    {
                        for (int i = 0; i < Tw; i++)
                        {
                            if (!tplMask[i, j])
                                continue;

                            double Iv = imgData[x + i, y + j];
                            sumI += Iv;
                            sumI2 += Iv * Iv;
                            cross += Iv * tplData[i, j];
                        }
                    }

                    double imgMean = sumI / tplPixelCount;
                    double imgVar = sumI2 - imgMean * imgMean * tplPixelCount;

                    double numerator = cross - sumI * tplMean;
                    double denominator = Math.Sqrt(tplVar * imgVar);

                    double score = (denominator <= 1e-8) ? 0 : numerator / denominator;
                    if (Threshold < score)
                        Results.Add((x, y, score));
                }
            });

            return Results;
        }

        internal static IEnumerable<(int X, int Y)> FilterCandidates(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, TemplateMatchPreFilterOptions Options, MatchCache imgCache, MatchCache tplCache)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;
            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar))
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;

                IPixelAdapter TAdt = TCreateAdapter();
                for (int j = 0; j < Th; j++)
                {
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        double Value = TExtractValue(TAdt);
                        tplMean += Value;
                        tplVar += Value * Value;
                        tplPixelCount++;
                    }
                }

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
            }

            tplPixelCount = (long)Tw * Th;  // Unmask
            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // 計算模板邊緣強度（Sobel）
            double tplEdgeSum = tplCache.EdgeSum;
            if (double.IsNegativeInfinity(tplEdgeSum))
            {
                tplEdgeSum = 0.0;
                ImagePatch TPatch = new(TCreateAdapter(), 3, 3);
                for (int y = 1; y < Th - 1; y++)
                {
                    TPatch.DangerousMove(1, y);
                    for (int x = 1; x < Tw - 1; x++, TPatch.DangerousMoveNextX())
                    {
                        double gx = TExtractValue((IPixelAdapter)TPatch[2, 1]) - TExtractValue((IPixelAdapter)TPatch[0, 1]);
                        double gy = TExtractValue((IPixelAdapter)TPatch[1, 2]) - TExtractValue((IPixelAdapter)TPatch[1, 0]);
                        tplEdgeSum += gx * gx + gy * gy;
                    }
                }

                tplCache.EdgeSum = tplEdgeSum;
            }

            // ===============================
            //             影像資訊
            // ===============================
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            if (Integral is null || SquaredIntegral is null)
            {
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        Integral[j + 1, i] = Integral[j, i] + RowSum;

                        SquaredrowSum += Value * Value;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }

            double MeanThreshold = Options.MeanThreshold;
            double VarianceThreshold = Options.VarianceThreshold;
            double EdgeThreshold = Options.EdgeThreshold * tplEdgeSum;

            ImagePatch Patch = new(ICreateAdapter(), 3, 3);
            for (int y = 0; y <= Ih - Th; y++)
            {
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    long sum = GetRegionSum(Integral, x, y, Tw, Th);
                    long sumSq = GetRegionSum(SquaredIntegral, x, y, Tw, Th);

                    double imgMean = (double)sum / tplPixelCount;
                    double imgVar = sumSq - 2 * imgMean * sum + imgMean * imgMean * tplPixelCount;

                    // 均值篩選
                    if (Math.Abs(imgMean - tplMean) > MeanThreshold)
                        continue;

                    // 方差篩選
                    double varRatio = imgVar / tplVar;
                    if (varRatio < VarianceThreshold || varRatio > 1.0 / VarianceThreshold)
                        continue;

                    // 邊緣篩選（Sobel）
                    double edgeSum = 0;
                    for (int j = 1; j < Th - 1; j++)
                    {
                        Patch.DangerousMove(x + 1, y + j);
                        for (int i = 1; i < Tw - 1; i++, Patch.DangerousMoveNextX())
                        {
                            double gx = IExtractValue((IPixelAdapter)Patch[2, 1]) - IExtractValue((IPixelAdapter)Patch[0, 1]);  //image[x + i + 1, y + j].Gray - image[x + i - 1, y + j].Gray;
                            double gy = IExtractValue((IPixelAdapter)Patch[1, 2]) - IExtractValue((IPixelAdapter)Patch[1, 0]);  //image[x + i, y + j + 1].Gray - image[x + i, y + j - 1].Gray;
                            edgeSum += gx * gx + gy * gy;
                        }
                    }

                    if (edgeSum < EdgeThreshold)
                        continue;

                    yield return (x, y);
                }
            }
        }
        internal static IEnumerable<(int X, int Y)> FilterCandidates(IPixelAdapter Image, IPixelAdapter Template, ImageChannel Channel, TemplateMatchPreFilterOptions Options, MatchCache imgCache, MatchCache tplCache, ParallelOptions ParallelOptions)
        {
            CreateAccessors(Image, Channel, out Func<IPixelAdapter> ICreateAdapter, out Func<IPixelAdapter, byte> IExtractValue);
            CreateAccessors(Template, Channel, out Func<IPixelAdapter> TCreateAdapter, out Func<IPixelAdapter, byte> TExtractValue);

            int Iw = Image.XLength;
            int Ih = Image.YLength;

            // ===============================
            //             模板資訊
            // ===============================
            int Tw = Template.XLength;
            int Th = Template.YLength;

            // 計算均值、方差
            double tplMean = tplCache.SumOfValue;
            double tplVar = tplCache.SumOfSquares;
            long tplPixelCount;
            if (double.IsNegativeInfinity(tplMean) || double.IsNegativeInfinity(tplVar))
            {
                tplMean = 0.0;
                tplVar = 0.0;
                tplPixelCount = 0;
                Parallel.For(0, Th, ParallelOptions,
                () => (0.0, 0.0, 0),
                (j, loopState, local) =>
                {
                    IPixelAdapter TAdt = TCreateAdapter();
                    TAdt.DangerousMove(0, j);
                    for (int i = 0; i < Tw; i++, TAdt.DangerousMoveNextX())
                    {
                        if (TAdt.A == byte.MinValue)
                            continue;

                        double Value = TExtractValue(TAdt);
                        local.Item1 += Value;
                        local.Item2 += Value * Value;
                        local.Item3++;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplMean, tplMean + local.Item1);
                    Interlocked.Exchange(ref tplVar, tplVar + local.Item2);
                    Interlocked.Add(ref tplPixelCount, local.Item3);
                });

                tplCache.SumOfValue = tplMean;
                tplCache.SumOfSquares = tplVar;
                tplCache.ValidPixelCount = tplPixelCount;
            }

            tplPixelCount = (long)Tw * Th;  // Unmask
            tplMean /= tplPixelCount;
            tplVar -= tplMean * tplMean * tplPixelCount;

            // 計算模板邊緣強度（Sobel）
            double tplEdgeSum = tplCache.EdgeSum;
            if (double.IsNegativeInfinity(tplEdgeSum))
            {
                tplEdgeSum = 0.0;
                Parallel.For(1, Th - 1, ParallelOptions,
                () => 0.0,
                (j, loopState, local) =>
                {
                    ImagePatch Patch = new(TCreateAdapter(), 3, 3);
                    Patch.DangerousMove(1, j);

                    for (int x = 1; x < Tw - 1; x++, Patch.DangerousMoveNextX())
                    {
                        double gx = TExtractValue((IPixelAdapter)Patch[2, 1]) - TExtractValue((IPixelAdapter)Patch[0, 1]);
                        double gy = TExtractValue((IPixelAdapter)Patch[1, 2]) - TExtractValue((IPixelAdapter)Patch[1, 0]);
                        local += gx * gx + gy * gy;
                    }

                    return local;
                },
                local =>
                {
                    Interlocked.Exchange(ref tplEdgeSum, tplEdgeSum + local);
                });

                tplCache.EdgeSum = tplEdgeSum;
            }

            // ===============================
            //             影像資訊
            // ===============================
            long[,] Integral = imgCache.Integral;
            long[,] SquaredIntegral = imgCache.SquaredIntegral;
            if (Integral is null || SquaredIntegral is null)
            {
                Integral = new long[Ih + 1, Iw + 1];
                SquaredIntegral = new long[Ih + 1, Iw + 1];

                IPixelAdapter IAdt = ICreateAdapter();
                for (int j = 0; j < Ih; j++)
                {
                    long RowSum = 0;
                    long SquaredrowSum = 0;
                    IAdt.DangerousMove(0, j);
                    for (int i = 1; i <= Iw; i++, IAdt.DangerousMoveNextX())
                    {
                        long Value = IExtractValue(IAdt);
                        RowSum += Value;
                        SquaredrowSum += Value * Value;

                        Integral[j + 1, i] = Integral[j, i] + RowSum;
                        SquaredIntegral[j + 1, i] = SquaredIntegral[j, i] + SquaredrowSum;
                    }
                }

                imgCache.Integral = Integral;
                imgCache.SquaredIntegral = SquaredIntegral;
            }

            double MeanThreshold = Options.MeanThreshold;
            double VarianceThreshold = Options.VarianceThreshold;
            double EdgeThreshold = Options.EdgeThreshold * tplEdgeSum;

            ConcurrentBag<(int X, int Y)> Results = [];
            Parallel.For(0, Ih - Th + 1, ParallelOptions, y =>
            {
                ImagePatch Patch = new(ICreateAdapter(), 3, 3);
                for (int x = 0; x <= Iw - Tw; x++)
                {
                    long sum = GetRegionSum(Integral, x, y, Tw, Th);
                    long sumSq = GetRegionSum(SquaredIntegral, x, y, Tw, Th);

                    double imgMean = (double)sum / tplPixelCount;
                    double imgVar = sumSq - 2 * imgMean * sum + imgMean * imgMean * tplPixelCount;

                    // 均值篩選
                    if (Math.Abs(imgMean - tplMean) > MeanThreshold)
                        continue;

                    // 方差篩選
                    double varRatio = imgVar / tplVar;
                    if (varRatio < VarianceThreshold || varRatio > 1.0 / VarianceThreshold)
                        continue;

                    // 邊緣篩選（Sobel）
                    double edgeSum = 0;
                    for (int j = 1; j < Th - 1; j++)
                    {
                        Patch.DangerousMove(x + 1, y + j);
                        for (int i = 1; i < Tw - 1; i++, Patch.DangerousMoveNextX())
                        {
                            double gx = IExtractValue((IPixelAdapter)Patch[2, 1]) - IExtractValue((IPixelAdapter)Patch[0, 1]);  //image[x + i + 1, y + j].Gray - image[x + i - 1, y + j].Gray;
                            double gy = IExtractValue((IPixelAdapter)Patch[1, 2]) - IExtractValue((IPixelAdapter)Patch[1, 0]);  //image[x + i, y + j + 1].Gray - image[x + i, y + j - 1].Gray;
                            edgeSum += gx * gx + gy * gy;
                        }
                    }

                    if (edgeSum < EdgeThreshold)
                        continue;

                    Results.Add((x, y));
                }
            });

            return Results;
        }
        internal static List<(int Lx, int Ty, int Rx, int By)> MergeCandidates(IEnumerable<(int X, int Y)> Candidates, int Radius)
        {
            List<(int Lx, int Ty, int Rx, int By)> Result = [];

            HashSet<(int X, int Y)> visited = [];
            foreach ((int X, int Y) point in Candidates)
            {
                if (visited.Contains(point))
                    continue;

                // BFS 找到這個點所屬的群
                Queue<(int X, int Y)> queue = new();
                List<(int X, int Y)> cluster = [];
                queue.Enqueue(point);
                visited.Add(point);

                while (queue.Count > 0)
                {
                    (int X, int Y) cur = queue.Dequeue();
                    cluster.Add(cur);

                    foreach ((int X, int Y) other in Candidates)
                    {
                        if (visited.Contains(other))
                            continue;

                        // 在 X,Y 範圍 ±radius 內就算同群
                        if (Math.Abs(cur.X - other.X) <= Radius && Math.Abs(cur.Y - other.Y) <= Radius)
                        {
                            queue.Enqueue(other);
                            visited.Add(other);
                        }
                    }
                }

                (int Px, int Py) = cluster[0];
                int Lx = Px, Ty = Py;
                int Rx = Px, By = Py;
                for (int i = 1; i < cluster.Count; i++)
                {
                    (Px, Py) = cluster[i];

                    if (Px < Lx)
                        Lx = Px;
                    else if (Rx < Px)
                        Rx = Px;

                    if (Py < Ty)
                        Ty = Py;
                    else if (By < Py)
                        By = Py;
                }

                Result.Add((Lx, Ty, Rx, By));
            }

            return Result;
        }

        private static void CreateAccessors(IPixelAdapter Source, ImageChannel Channel, out Func<IPixelAdapter> CreateAdapter, out Func<IPixelAdapter, byte> ExtractValue)
        {
            switch (Channel)
            {
                case ImageChannel.R:
                    {
                        CreateAdapter = Source.Clone;
                        ExtractValue = a => a.R;
                        break;
                    }
                case ImageChannel.G:
                    {
                        CreateAdapter = Source.Clone;
                        ExtractValue = a => a.G;
                        break;
                    }
                case ImageChannel.B:
                    {
                        CreateAdapter = Source.Clone;
                        ExtractValue = a => a.B;

                        break;
                    }
                case ImageChannel.All:
                    {
                        CreateAdapter = PixelHelper.IsNonAlphaPixel(Source.PixelType) ?
                                        Source.PixelType == typeof(Gray8) ? Source.Clone : () => new CastPixelAdapter<Gray8>(Source.Clone()) :
                                        () => new CastPixelAdapter<AlphaGray8>(Source.Clone());

                        ExtractValue = a => a.R;
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private static void FFT1D(double[] real, double[] imag, bool inverse)
        {
            static int ReverseBits(int i, int bits)
            {
                int j = 0;
                for (int x = 0; x < bits; x++)
                {
                    j = (j << 1) | (i & 1);
                    i >>= 1;
                }
                return j;
            }

            int n = real.Length;
#if NET6_0_OR_GREATER
            int m = (int)Math.Log2(n);
#else
            int m = (int)Math.Log(n, 2);
#endif
            if ((1 << m) != n)
                throw new ArgumentException("FFT length must be power of 2");

            // 位元反轉置換
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, m);
                if (j > i)
                {
                    (real[i], real[j]) = (real[j], real[i]);
                    (imag[i], imag[j]) = (imag[j], imag[i]);
                }
            }

            // FFT 演算法
            for (int s = 1; s <= m; s++)
            {
                int m2 = 1 << s;
                int m1 = m2 >> 1;
                double theta = (inverse ? 2 : -2) * Math.PI / m2;
                double wpr = Math.Cos(theta);
                double wpi = Math.Sin(theta);

                for (int k = 0; k < n; k += m2)
                {
                    double wr = 1.0;
                    double wi = 0.0;

                    for (int j = 0; j < m1; j++)
                    {
                        int i1 = k + j;
                        int i2 = i1 + m1;

                        double tr = wr * real[i2] - wi * imag[i2];
                        double ti = wr * imag[i2] + wi * real[i2];

                        real[i2] = real[i1] - tr;
                        imag[i2] = imag[i1] - ti;
                        real[i1] += tr;
                        imag[i1] += ti;

                        // 更新旋轉因子
                        double tmp = wr;
                        wr = tmp * wpr - wi * wpi;
                        wi = tmp * wpi + wi * wpr;
                    }
                }
            }

            // 逆 FFT 時除以 n
            if (inverse)
            {
                for (int i = 0; i < n; i++)
                {
                    real[i] /= n;
                    imag[i] /= n;
                }
            }
        }
        private static void FFT2D(double[,] real, double[,] imag, bool inverse)
        {
            int h = real.GetLength(0);
            int w = real.GetLength(1);

            // 逐列 FFT
            double[] rbuf = new double[w];
            double[] ibuf = new double[w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    rbuf[x] = real[y, x];
                    ibuf[x] = imag[y, x];
                }

                FFT1D(rbuf, ibuf, inverse);

                for (int x = 0; x < w; x++)
                {
                    real[y, x] = rbuf[x];
                    imag[y, x] = ibuf[x];
                }
            }

            // 逐行 FFT
            rbuf = new double[h];
            ibuf = new double[h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    rbuf[y] = real[y, x];
                    ibuf[y] = imag[y, x];
                }

                FFT1D(rbuf, ibuf, inverse);

                for (int y = 0; y < h; y++)
                {
                    real[y, x] = rbuf[y];
                    imag[y, x] = ibuf[y];
                }
            }
        }
        private static void FFT2D(double[,] real, double[,] imag, bool inverse, ParallelOptions ParallelOptions)
        {
            int h = real.GetLength(0);
            int w = real.GetLength(1);

            // 逐列 FFT
            Parallel.For(0, h, ParallelOptions, y =>
            {
                double[] rbuf = new double[w];
                double[] ibuf = new double[w];
                for (int x = 0; x < w; x++)
                {
                    rbuf[x] = real[y, x];
                    ibuf[x] = imag[y, x];
                }

                FFT1D(rbuf, ibuf, inverse);

                for (int x = 0; x < w; x++)
                {
                    real[y, x] = rbuf[x];
                    imag[y, x] = ibuf[x];
                }
            });

            // 逐行 FFT
            Parallel.For(0, w, ParallelOptions, x =>
            {
                double[] rbuf = new double[h];
                double[] ibuf = new double[h];
                for (int y = 0; y < h; y++)
                {
                    rbuf[y] = real[y, x];
                    ibuf[y] = imag[y, x];
                }

                FFT1D(rbuf, ibuf, inverse);

                for (int y = 0; y < h; y++)
                {
                    real[y, x] = rbuf[y];
                    imag[y, x] = ibuf[y];
                }
            });
        }

        private static void Dot(double a, double b, double c, double d, ref double x, ref double y)
        {
            x = a * c - b * d;
            y = a * d + b * c;
        }

        private static int NextPowerOfTwo(int x)
        {
            int p = 1;
            while (p < x)
                p <<= 1;

            return p;
        }

        /// <summary>
        /// 取得矩形區域的總和
        /// </summary>
        private static long GetRegionSum(long[,] integral, int x, int y, int w, int h)
        {
            int x2 = x + w;
            int y2 = y + h;
            return integral[y2, x2] - integral[y, x2] - integral[y2, x] + integral[y, x];
        }

        internal class MatchCache
        {
            public double SumOfValue = double.NegativeInfinity;

            public double SumOfSquares = double.NegativeInfinity;

            public double EdgeSum = double.NegativeInfinity;

            public long ValidPixelCount = -1;

            public bool[,] Mask;

            public double[,] Data;

            public long[,] Integral;

            public long[,] SquaredIntegral;

        }

    }
}