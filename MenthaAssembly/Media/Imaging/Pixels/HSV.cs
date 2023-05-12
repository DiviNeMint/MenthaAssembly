using MenthaAssembly.Media.Imaging.Primitives;
using System;

namespace MenthaAssembly.Media.Imaging
{
    [NonAlpha]
    [Serializable]
    public struct HSV : IPixel
    {
        private double _H;
        public double H
        {
            get => _H;
            set => _H = NormalizationH(value);
        }

        private double _S;
        public double S
        {
            get => _S;
            set => _S = NormalizationS(value);
        }

        private double _V;
        public double V
        {
            get => _V;
            set => _V = NormalizationV(value);
        }

        byte IReadOnlyPixel.A
            => byte.MaxValue;

        byte IReadOnlyPixel.R
        {
            get
            {
                double V = _V * 255d;
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
                double V = _V * 255d;
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
                double V = _V * 255d;
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

        public HSV(byte R, byte G, byte B)
        {
            PixelHelper.ToHSV(R, G, B, out _H, out _S, out _V);
        }
        public HSV(double H, double S, double V)
        {
            _H = NormalizationH(H);
            _S = NormalizationS(S);
            _V = NormalizationV(V);
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
            hashCode = hashCode * -1521134295 + _V.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
            => obj is HSV v && _H == v._H && _S == v._S && _V == v._V;

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
        private static double NormalizationV(double V)
            => MathHelper.Clamp(V, 0d, 1d);

        public static implicit operator HSV(RGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(BGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(RGBA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(ARGB Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(BGRA Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(ABGR Target)
            => new(Target.R, Target.G, Target.B);
        public static implicit operator HSV(Gray8 Target)
            => new(0d, 0d, Target.Gray / 255d);

        public static bool operator ==(HSV This, HSV Target)
            => This.Equals(Target);
        public static bool operator !=(HSV This, HSV Target)
            => !This.Equals(Target);

    }
}