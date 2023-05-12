using MenthaAssembly.Media.Imaging.Primitives;
using System;

namespace MenthaAssembly.Media.Imaging
{
    /// <summary>
    /// Represents a pixel with hue value, saturation, brightness.
    /// </summary>
    [NonAlpha]
    [Serializable]
    public struct HSB : IPixel
    {
        private double _H, _S, _B;

        /// <summary>
        /// Gets the hue value for this pixel.
        /// </summary>
        public double H
        {
            get => _H;
            set => _H = NormalizationH(value);
        }

        /// <summary>
        /// Gets the saturation value for this pixel.
        /// </summary>
        public double S
        {
            get => _S;
            set => _S = NormalizationS(value);
        }

        /// <summary>
        /// Gets the brightness value for this pixel.
        /// </summary>
        public double B
        {
            get => _B;
            set => _B = NormalizationB(value);
        }

        byte IReadOnlyPixel.A
            => byte.MaxValue;

        byte IReadOnlyPixel.R
        {
            get
            {
                double V = _B * 255d;
                if (_S <= 0d)
                    return (byte)Math.Round(V);

                double hh = H / 60d;
                return Math.Floor(hh) switch
                {
                    1d => (byte)Math.Round(V * (1d - S * (hh - 1d))),
                    2d or 3d => (byte)Math.Round(V * (1d - S)),
                    4d => (byte)Math.Round(V * (1d - S * (5d - hh))),
                    _ => (byte)Math.Round(V),
                };
            }
        }

        byte IReadOnlyPixel.G
        {
            get
            {
                double V = _B * 255d;
                if (_S <= 0d)
                    return (byte)Math.Round(V);

                double hh = H / 60d;
                return Math.Floor(hh) switch
                {
                    1d or 2d => (byte)Math.Round(V),
                    3d => (byte)Math.Round(V * (1d - S * (hh - 3d))),
                    4d or 5d => (byte)Math.Round(V * (1d - S)),
                    _ => (byte)Math.Round(V * (1d - S * (1d - hh))),
                };
            }
        }

        byte IReadOnlyPixel.B
        {
            get
            {
                double V = _B * 255d;
                if (_S <= 0d)
                    return (byte)Math.Round(V);

                double hh = H / 60d;
                return Math.Floor(hh) switch
                {
                    2d => (byte)Math.Round(V * (1d - S * (3d - hh))),
                    3d or 4d => (byte)Math.Round(V),
                    5d => (byte)Math.Round(V * (1d - S * (hh - 5d))),
                    _ => (byte)Math.Round(V * (1d - S)),
                };
            }
        }

        int IPixelBase.BitsPerPixel => 24;

        public HSB(byte R, byte G, byte B)
        {
            PixelHelper.GetHSV(R, G, B, out _H, out _S, out _B);
        }
        public HSB(double H, double S, double B)
        {
            _H = NormalizationH(H);
            _S = NormalizationS(S);
            _B = NormalizationB(B);
        }

        void IPixel.Overlay(byte A, byte R, byte G, byte B)
            => throw new NotImplementedException();

        void IPixel.Override(byte A, byte R, byte G, byte B)
            => throw new NotImplementedException();

        public override int GetHashCode()
        {
            int hashCode = -1944459463;
            hashCode = hashCode * -1521134295 + _H.GetHashCode();
            hashCode = hashCode * -1521134295 + _S.GetHashCode();
            hashCode = hashCode * -1521134295 + _B.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is HSB v && _H == v._H && _S == v._S && _B == v._B;

        private static double NormalizationH(double H)
        {
            // Normalization
            while (H < 0d)
                H += 360d;

            while (360d <= H)
                H -= 360d;

            return H;
        }
        private static double NormalizationS(double S)
            => MathHelper.Clamp(S, 0d, 1d);
        private static double NormalizationB(double V)
            => MathHelper.Clamp(V, 0d, 1d);

        public static implicit operator HSB(RGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(BGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(RGBA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(ARGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(BGRA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(ABGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSB(Gray8 Target)
            => new(0d, 0d, Target.Gray / 255d);

        public static bool operator ==(HSB This, HSB Target)
            => This.Equals(Target);
        public static bool operator !=(HSB This, HSB Target)
            => !This.Equals(Target);

    }
}