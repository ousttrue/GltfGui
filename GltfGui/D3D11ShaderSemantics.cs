using System;
using System.Text;

namespace GltfGui
{
    public enum Semantics
    {
        POSITION,
        COLOR,
    }

    public static class SemanticsExtensions
    {
        static Byte[] position = Encoding.UTF8.GetBytes("POSITION");
        static Byte[] color = Encoding.UTF8.GetBytes("COLOR");

        static PinPtr<byte> position_pin;
        static PinPtr<byte> color_pin;

        static SemanticsExtensions()
        {
            position_pin = PinPtr.Create(position);
            color_pin = PinPtr.Create(color);
        }

        public static IntPtr Pin(this Semantics semantic)
        {
            switch (semantic)
            {
                case Semantics.POSITION: return position_pin.Ptr;
                case Semantics.COLOR: return color_pin.Ptr;
            }

            throw new NotImplementedException();
        }
    }
}
