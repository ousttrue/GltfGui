using System.Runtime.InteropServices;

namespace GltfGui
{
    static class HRExtensions
    {
        public static void ThrowIfFailed(this int hr, string msg = "")
        {
            if (hr != 0)
            {
                throw new COMException(msg, hr);
            }
        }
    }
}
