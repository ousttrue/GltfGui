using System;
using NWindowsKits;

namespace GltfGui
{
    /// <summary>
    /// Swapchain
    /// </summary>
    public class DXGISwapChainForHWND : IDisposable
    {
        IDXGISwapChain1 m_swapChain;
        bool m_disposed;

        public void Dispose()
        {
            if (m_swapChain)
            {
                m_swapChain.Dispose();
                m_swapChain = null;
            }
            m_disposed = true;
        }

        public void Resize(int w, int h)
        {
            if (m_swapChain)
            {
                DXGI_SWAP_CHAIN_DESC desc = default;
                m_swapChain.GetDesc(ref desc).ThrowIfFailed();
                m_swapChain.ResizeBuffers(desc.BufferCount,
                    (uint)w,
                    (uint)h,
                    desc.BufferDesc.Format, desc.Flags);
            }
        }

        public ID3D11Texture2D GetBackbuffer(ID3D11Device device, HWND hWnd)
        {
            if (m_disposed)
            {
                return null;
            }
            if (!m_swapChain)
            {
                m_swapChain = new IDXGISwapChain1();
                using var dxgiDevice = new IDXGIDevice2();
                device.QueryInterface(ref IDXGIDevice2.IID, ref dxgiDevice.NewPtr).ThrowIfFailed();

                using var dxgiAdapter = new IDXGIAdapter();
                dxgiDevice.GetAdapter(ref dxgiAdapter.NewPtr).ThrowIfFailed();

                using var dxgiFactory = new IDXGIFactory2();
                dxgiAdapter.GetParent(ref IDXGIFactory2.IID, ref dxgiFactory.NewPtr).ThrowIfFailed();

                var desc = new DXGI_SWAP_CHAIN_DESC1
                {
                    Format = DXGI_FORMAT._B8G8R8A8_UNORM,
                    AlphaMode = DXGI_ALPHA_MODE._UNSPECIFIED,
                    BufferUsage = C.DXGI_USAGE_RENDER_TARGET_OUTPUT,
                    Scaling = DXGI_SCALING._NONE,
                    BufferCount = 2,
                    SwapEffect = DXGI_SWAP_EFFECT._FLIP_SEQUENTIAL,
                    SampleDesc = new DXGI_SAMPLE_DESC
                    {
                        Count = 1,
                        Quality = 0,
                    },

                };
                var fs_desc = new DXGI_SWAP_CHAIN_FULLSCREEN_DESC
                {
                    Windowed = 1,
                };
                dxgiFactory.CreateSwapChainForHwnd(device.Ptr, hWnd, ref desc, ref fs_desc, default, ref m_swapChain.NewPtr).ThrowIfFailed();
            }

            var texture = new ID3D11Texture2D();
            m_swapChain.GetBuffer(0, ref texture.GetIID(), ref texture.NewPtr).ThrowIfFailed();
            return texture;
        }

        public void Present()
        {
            if (m_swapChain is null)
            {
                return;
            }
            m_swapChain.Present(0, 0);
        }
    }
}
