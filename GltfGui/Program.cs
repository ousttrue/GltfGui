using System;
using System.Text;

namespace GltfGui
{
    class Program
    {
        [STAThread]
        static void Main(string[] _)
        {
            // for SJIS
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var window = Win32.Window.Create();
            if (window == null)
            {
                throw new Exception("fail to create window");
            }

            using (var d3d = new D3DApp())
            {
                window.OnResize += (w, h) =>
                {
                    d3d.Resize(window.WindowHandle, w, h);
                };

                Win32.MessageLoop.Run(() =>
                {
                    d3d.Draw(window.WindowHandle);

                }, 30);
            }
        }
    }
}
