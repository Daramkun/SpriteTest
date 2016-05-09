using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SpriteTest
{
	static class Program
	{
		public static Random rand = new Random ();
		public static Thread mainThread;

		public static GameWindow openTK;

		public static SharpDX.Direct3D11.Device d3dDevice;
		public static SharpDX.DXGI.SwapChain dxgiSwapChain;
		public static SharpDX.Direct3D11.RenderTargetView d3dRenderTargetView;

		public static SharpDX.Direct3D9.Direct3D d3d9;
		public static SharpDX.Direct3D9.Device d3dDevice9;

		static void InitializeDirect3D11 ()
		{
			IntPtr hwnd = openTK.WindowInfo.Handle;

			SharpDX.Direct3D11.Device.CreateWithSwapChain ( SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.None,
				new SharpDX.DXGI.SwapChainDescription ()
				{
					BufferCount = 1,
					Flags = SharpDX.DXGI.SwapChainFlags.None,
					IsWindowed = true,
					ModeDescription = new SharpDX.DXGI.ModeDescription ( 800, 600, new SharpDX.DXGI.Rational ( 60, 1 ), SharpDX.DXGI.Format.R8G8B8A8_UNorm ),
					OutputHandle = hwnd,
					SampleDescription = new SharpDX.DXGI.SampleDescription ( 1, 0 ),
					SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
					Usage = SharpDX.DXGI.Usage.RenderTargetOutput
				}, out d3dDevice, out dxgiSwapChain );
			var backBuffer = dxgiSwapChain.GetBackBuffer<SharpDX.Direct3D11.Resource> ( 0 );
			d3dRenderTargetView = new SharpDX.Direct3D11.RenderTargetView ( d3dDevice, backBuffer );
			backBuffer.Dispose ();

			d3dDevice.ImmediateContext.OutputMerger.SetRenderTargets ( d3dRenderTargetView );
			d3dDevice.ImmediateContext.Rasterizer.SetViewport ( 0, 0, 800, 600, 0, 1 );
		}

		static void InitializeDirect3D9 ()
		{
			d3d9 = new SharpDX.Direct3D9.Direct3D ();
			d3dDevice9 = new SharpDX.Direct3D9.Device ( d3d9, 0, SharpDX.Direct3D9.DeviceType.Hardware, openTK.WindowInfo.Handle,
				SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing, new SharpDX.Direct3D9.PresentParameters ()
				{
					BackBufferWidth = 800,
					BackBufferHeight = 600,
					BackBufferFormat = SharpDX.Direct3D9.Format.A8R8G8B8,
					BackBufferCount = 1,
					AutoDepthStencilFormat = SharpDX.Direct3D9.Format.D24S8,
					EnableAutoDepthStencil = true,
					PresentationInterval = SharpDX.Direct3D9.PresentInterval.One,
					SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
					Windowed = true,
					DeviceWindowHandle = openTK.WindowInfo.Handle,
				} );
		}

		[STAThread]
		static void Main ()
		{
			mainThread = Thread.CurrentThread;

			GameWindow window = new GameWindow ( 800, 600 );
			openTK = window;
			SceneContainer sceneContainer = new SceneContainer (
				/*new TestSceneOpenGL ()/**/
				/*new TestSceneDX11 ()/**/
				/**/new TestSceneDX9 ()/**/
			);
			GameTime updateGameTime = new GameTime (), renderGameTime = new GameTime ();
			window.Load += ( sender, e ) =>
			{
				InitializeDirect3D11 ();
				InitializeDirect3D9 ();
				sceneContainer.OnInitialize ();
			};
			window.UpdateFrame += ( sender, e ) =>
			{
				updateGameTime.Update ();
				sceneContainer.OnUpdate ( updateGameTime );
				sceneContainer.OnPostProcess ();
			};
			window.RenderFrame += ( sender, e ) =>
			{
				renderGameTime.Update ();
				sceneContainer.OnDraw ( renderGameTime );
				sceneContainer.OnPostDraw ();
			};
			window.Unload += ( sender, e ) =>
			{
				sceneContainer.OnUninitialize ();
				d3dDevice9.Dispose ();
				d3d9.Dispose ();
				d3dRenderTargetView.Dispose ();
				dxgiSwapChain.Dispose ();
				d3dDevice.Dispose ();
			};
			window.Run ();
		}
	}
}
