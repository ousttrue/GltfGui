using System;
using System.Numerics;
using NWindowsKits;

namespace GltfGui
{
    class D3DApp : IDisposable
    {
        D3D11Device m_d3d11;
        DXGISwapChainForHWND m_swapchain;
        D3D11Shader m_shader;
        // D3D11Model m_model;
        D3D11Mesh m_mesh;

        public void Dispose()
        {
            m_mesh.Dispose();
            m_mesh = null;
            m_swapchain.Dispose();
            m_swapchain = null;
            m_d3d11.Dispose();
            m_d3d11 = null;
        }

        public D3DApp()
        {
            m_d3d11 = new D3D11Device();
            Console.WriteLine(m_d3d11);
            m_swapchain = new DXGISwapChainForHWND();
            m_shader = D3D11Shader.CreateSampleShader();
            // m_model = D3D11Model.CreateTriangle();
            m_mesh = D3D11Mesh.CreateTriangle();
        }

        public void Resize(HWND _, int w, int h)
        {
            m_swapchain.Resize(w, h);
        }

        ID3D11RenderTargetView Begin(HWND hWnd, out float width, out float height)
        {
            using (var texture = m_swapchain.GetBackbuffer(m_d3d11.Device, hWnd))
            {
                D3D11_TEXTURE2D_DESC desc = default;
                texture.GetDesc(ref desc);
                width = (float)desc.Width;
                height = (float)desc.Height;

                var rtv_desc = new D3D11_RENDER_TARGET_VIEW_DESC
                {
                    Format = desc.Format,
                    ViewDimension = D3D11_RTV_DIMENSION._TEXTURE2D
                };
                var rtv = new ID3D11RenderTargetView();
                m_d3d11.Device.CreateRenderTargetView(texture.Ptr, ref rtv_desc, ref rtv.NewPtr).ThrowIfFailed();
                return rtv;
            }
        }

        void End()
        {
            m_d3d11.Context.Flush();
            m_swapchain.Present();
        }

        public void Draw(HWND hWnd)
        {
            if (m_d3d11 == null)
            {
                return;
            }

            using var rtv = Begin(hWnd, out float width, out float height);
            {
                // clear
                var clearColor = new Vector4(0.0f, 0.125f, 0.3f, 1.0f);
                m_d3d11.Context.ClearRenderTargetView(rtv.Ptr, ref clearColor.X);

                // draw pipeline
                m_d3d11.Context.OMSetRenderTargets(1, ref rtv.Ptr, default);
                var vp = new D3D11_VIEWPORT
                {
                    Width = width,
                    Height = height,
                    MinDepth = 0.0f,
                    MaxDepth = 1.0f,
                };
                m_d3d11.Context.RSSetViewports(1, ref vp);

                m_shader.Setup(m_d3d11.Device, m_d3d11.Context);

                // m_model.Draw(m_d3d11.Device, m_d3d11.Context, m_shader.Layout.AsSpan());
                m_mesh.Draw(m_d3d11.Device, m_d3d11.Context, m_shader.VertexAttributes.AsSpan());

                // flush
                End();
            }
        }
    }
}
