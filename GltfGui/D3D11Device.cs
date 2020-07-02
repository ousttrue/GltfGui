using System;
using System.Runtime.InteropServices;
using NWindowsKits;

namespace GltfGui
{
    /// <summary>
    ///  ID3DDevice
    /// </summary>
    public class D3D11Device : IDisposable
    {
        public ID3D11Device Device { get; private set; }
        public ID3D11DeviceContext Context { get; private set; }
        public D3D_FEATURE_LEVEL FeatureLevel { get; private set; }

        public override string ToString()
        {
            if (Device != null)
            {
                return $"[D3D11Device: {FeatureLevel}]";
            }
            else
            {
                return "[D3D11Device: null]";
            }
        }

        public void Dispose()
        {
            Device.Dispose();
            Device = null;
            Context.Dispose();
            Context = null;
        }

        public D3D11Device()
        {
            Span<D3D_FEATURE_LEVEL> levels = stackalloc D3D_FEATURE_LEVEL[]
            {
                D3D_FEATURE_LEVEL._12_1,
                D3D_FEATURE_LEVEL._12_0,
                D3D_FEATURE_LEVEL._11_1,
                D3D_FEATURE_LEVEL._11_0,
                D3D_FEATURE_LEVEL._10_1,
                D3D_FEATURE_LEVEL._10_0,
                D3D_FEATURE_LEVEL._9_3,
                D3D_FEATURE_LEVEL._9_2,
                D3D_FEATURE_LEVEL._9_1
            };

            var flags = D3D11_CREATE_DEVICE_FLAG._BGRA_SUPPORT;
#if DEBUG
            flags |= D3D11_CREATE_DEVICE_FLAG._DEBUG;
#endif

            Device = new ID3D11Device();
            Context = new ID3D11DeviceContext();
            D3D_FEATURE_LEVEL level = default;
            d3d11.D3D11CreateDevice(
                default,
                D3D_DRIVER_TYPE._HARDWARE,
                default,
                (uint)flags,
                ref MemoryMarshal.GetReference(levels),
                (uint)levels.Length,
                C.D3D11_SDK_VERSION,
                ref Device.NewPtr,
                ref level,
                ref Context.NewPtr).ThrowIfFailed();

            FeatureLevel = level;
        }
    }
}
