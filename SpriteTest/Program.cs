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
        public static SceneContainer sceneContainer;

		public static SharpDX.Direct3D11.Device d3dDevice11;
		public static SharpDX.DXGI.SwapChain dxgiSwapChain11;
		public static SharpDX.Direct3D11.RenderTargetView d3dRenderTargetView11;
		public static SharpDX.Direct3D11.Texture2D d3dDepthStencilBuffer11;
		public static SharpDX.Direct3D11.DepthStencilView d3dDepthStencilView11;

		public static SharpDX.Direct3D9.Direct3D d3d9;
		public static SharpDX.Direct3D9.Device d3dDevice9;

		public static SharpDX.DXGI.Factory2 dxgiFactory12;
		public static SharpDX.DXGI.Adapter1 dxgiAdapter12;
		public static SharpDX.Direct3D12.Device d3dDevice12;
		public static SharpDX.Direct3D12.CommandQueue d3dCommandQueue12;
		public static SharpDX.DXGI.SwapChain3 dxgiSwapChain12;
		public static SharpDX.Direct3D12.DescriptorHeap d3dDescriptorHeap12;
		public static SharpDX.Direct3D12.Resource [] d3dRenderTargets12;
		public static SharpDX.Direct3D12.CommandAllocator d3dCommandAllocator12;
		public static SharpDX.Direct3D12.RootSignature d3dRootSignature12;

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
				}, out d3dDevice11, out dxgiSwapChain11 );
			var backBuffer = dxgiSwapChain11.GetBackBuffer<SharpDX.Direct3D11.Resource> ( 0 );
			d3dRenderTargetView11 = new SharpDX.Direct3D11.RenderTargetView ( d3dDevice11, backBuffer );
			backBuffer.Dispose ();

			d3dDepthStencilBuffer11 = new SharpDX.Direct3D11.Texture2D ( d3dDevice11, new SharpDX.Direct3D11.Texture2DDescription ()
			{
				ArraySize = 1,
				BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
				Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
				Width = 800,
				Height = 600,
				MipLevels = 1,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				SampleDescription = new SharpDX.DXGI.SampleDescription ( 1, 0 )
			} );
			d3dDepthStencilView11 = new SharpDX.Direct3D11.DepthStencilView ( d3dDevice11, d3dDepthStencilBuffer11 );

			d3dDevice11.ImmediateContext.OutputMerger.SetRenderTargets ( d3dDepthStencilView11, d3dRenderTargetView11 );
			d3dDevice11.ImmediateContext.Rasterizer.SetViewport ( 0, 0, 800, 600, 0, 1 );
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

		static void InitializeDirect3D12 ()
		{
			dxgiFactory12 = new SharpDX.DXGI.Factory2 ();
			dxgiAdapter12 = dxgiFactory12.Adapters1 [ 0 ];
			d3dDevice12 = new SharpDX.Direct3D12.Device ( dxgiAdapter12 );
			d3dCommandQueue12 = d3dDevice12.CreateCommandQueue ( new SharpDX.Direct3D12.CommandQueueDescription ()
			{
				Type = SharpDX.Direct3D12.CommandListType.Direct,
				Priority = 0,
				Flags = SharpDX.Direct3D12.CommandQueueFlags.None,
				NodeMask = 0,
			} );
			IntPtr hwnd = openTK.WindowInfo.Handle;
			dxgiSwapChain12 = new SharpDX.DXGI.SwapChain ( dxgiFactory12, d3dCommandQueue12, new SharpDX.DXGI.SwapChainDescription ()
			{
				BufferCount = Environment.ProcessorCount,
				Flags = SharpDX.DXGI.SwapChainFlags.None,
				IsWindowed = true,
				ModeDescription = new SharpDX.DXGI.ModeDescription ( 800, 600, new SharpDX.DXGI.Rational ( 60, 1 ), SharpDX.DXGI.Format.R8G8B8A8_UNorm ),
				OutputHandle = hwnd,
				SampleDescription = new SharpDX.DXGI.SampleDescription ( 1, 0 ),
				SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
				Usage = SharpDX.DXGI.Usage.RenderTargetOutput
			} ).QueryInterface<SharpDX.DXGI.SwapChain3> ();
			d3dDescriptorHeap12 = d3dDevice12.CreateDescriptorHeap ( new SharpDX.Direct3D12.DescriptorHeapDescription ()
			{
				DescriptorCount = Environment.ProcessorCount,
				Type = SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView,
				NodeMask = 0,
				Flags = SharpDX.Direct3D12.DescriptorHeapFlags.None
			} );
			var size = d3dDevice12.GetDescriptorHandleIncrementSize ( SharpDX.Direct3D12.DescriptorHeapType.RenderTargetView );
			var handle = d3dDescriptorHeap12.CPUDescriptorHandleForHeapStart;
			d3dRenderTargets12 = new SharpDX.Direct3D12.Resource [ Environment.ProcessorCount ];
			for ( int i = 0; i < Environment.ProcessorCount; ++i )
			{
				d3dRenderTargets12 [ i ] = dxgiSwapChain12.GetBackBuffer<SharpDX.Direct3D12.Resource> ( i );
				d3dDevice12.CreateRenderTargetView ( d3dRenderTargets12 [ i ], null, handle );
				handle.Ptr += size;
			}
			d3dCommandAllocator12 = d3dDevice12.CreateCommandAllocator ( SharpDX.Direct3D12.CommandListType.Direct );

			var sigdesc = new SharpDX.Direct3D12.RootSignatureDescription ( SharpDX.Direct3D12.RootSignatureFlags.AllowInputAssemblerInputLayout,
				new SharpDX.Direct3D12.RootParameter [] 
				{
					new SharpDX.Direct3D12.RootParameter(SharpDX.Direct3D12.ShaderVisibility.Pixel, 
						new SharpDX.Direct3D12.DescriptorRange ( SharpDX.Direct3D12.DescriptorRangeType.ShaderResourceView, 1, 0, 0, int.MaxValue )
					)
				}, new SharpDX.Direct3D12.StaticSamplerDescription []
				{
					new SharpDX.Direct3D12.StaticSamplerDescription(SharpDX.Direct3D12.ShaderVisibility.Pixel, 0, 0)
					{
						Filter = SharpDX.Direct3D12.Filter.MinMagMipPoint,
						AddressUVW = SharpDX.Direct3D12.TextureAddressMode.Wrap
					}
				} );
			d3dRootSignature12 = d3dDevice12.CreateRootSignature ( 0, sigdesc.Serialize () );
		}

		[STAThread]
		static void Main ()
		{
			mainThread = Thread.CurrentThread;

			GameWindow window = new GameWindow ( 800, 600 );
			openTK = window;
			sceneContainer = new SceneContainer (
				/*new TestSceneOpenGL ()/**/
				/**/new TestSceneDX11 ()/**/
				/*new TestSceneDX9 ()/**/
			);
			GameTime updateGameTime = new GameTime (), renderGameTime = new GameTime ();
			window.Load += ( sender, e ) =>
			{
				InitializeDirect3D11 ();
				InitializeDirect3D9 ();
				InitializeDirect3D12 ();
				sceneContainer.OnInitialize ();
				window.VSync = VSyncMode.Off;
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

				d3dRootSignature12.Dispose ();
				d3dCommandAllocator12.Dispose ();
				d3dDescriptorHeap12.Dispose ();
				dxgiSwapChain12.Dispose ();
				d3dCommandQueue12.Dispose ();
				d3dDevice12.Dispose ();
				dxgiAdapter12.Dispose ();
				dxgiFactory12.Dispose ();

				d3dDevice9.Dispose ();
				d3d9.Dispose ();

				d3dDepthStencilView11.Dispose ();
				d3dDepthStencilBuffer11.Dispose ();
				d3dRenderTargetView11.Dispose ();
				dxgiSwapChain11.Dispose ();
				d3dDevice11.Dispose ();
			};
			
			window.Run ( 200, 200 );
		}
	}
}
