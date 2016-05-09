using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SpriteTest
{
	class TestSceneDX11 : Scene, ITestScene
	{
		ISprite sprite;
		IBitmap bitmap1, bitmap2;

		Queue<SharpDX.Direct3D11.CommandList> commandLists = new Queue<SharpDX.Direct3D11.CommandList> ();

		public ISprite Sprite { get { return sprite; } }

		public TestSceneDX11 ()
		{
			IsParallelUpdate = true;
			IsParallelDraw = true;
		}

		ConcurrentDictionary<Thread, SharpDX.Direct3D11.DeviceContext> contexts = new ConcurrentDictionary<Thread, SharpDX.Direct3D11.DeviceContext> ();

		public SharpDX.Direct3D11.DeviceContext GetDeviceContext ()
		{
			SharpDX.Direct3D11.DeviceContext context;
			if ( Thread.CurrentThread == Program.mainThread )
				return Program.d3dDevice.ImmediateContext;
			if ( !contexts.TryGetValue ( Thread.CurrentThread, out context ) )
			{
				context = new SharpDX.Direct3D11.DeviceContext ( Program.d3dDevice );
				context.OutputMerger.SetRenderTargets ( Program.d3dRenderTargetView );
				context.Rasterizer.SetViewport ( 0, 0, 800, 600, 0, 1 );
				contexts.TryAdd ( Thread.CurrentThread, context );
			}
			return context;
		}

		private void ChangeTitle () { Program.openTK.Title = $"SpriteTest DX11: {Children.Count}"; }

		private void KeyboardEvent ( object sender, KeyboardKeyEventArgs e )
		{
			switch ( e.Key )
			{
				case Key.Q: Children.Add ( new SpriteObject ( bitmap1 ) ); break;
				case Key.W: Children.Add ( new SpriteObject ( bitmap2 ) ); break;
				case Key.Number1: for ( int i = 0; i < 100; ++i ) Children.Add ( new SpriteObject ( bitmap1 ) ); break;
				case Key.Number2: for ( int i = 0; i < 100; ++i ) Children.Add ( new SpriteObject ( bitmap2 ) ); break;

				case Key.A: if ( Children.Count > 0 ) Children.Remove ( Children [ 0 ] ); break;
				case Key.S: for ( int i = 0; i < 100; ++i ) if ( Children.Count > 0 ) Children.Remove ( Children [ i ] ); break;
			}
		}

		public override void OnInitialize ()
		{
			sprite = new SpriteDX11 ();
			var assembly = Assembly.GetEntryAssembly ();
			bitmap1 = new BitmapDX11 ( assembly.GetManifestResourceStream ( "SpriteTest.Resources.Test1.jpg" ) );
			bitmap2 = new BitmapDX11 ( assembly.GetManifestResourceStream ( "SpriteTest.Resources.Test2.jpg" ) );

			Program.openTK.Keyboard.KeyUp += KeyboardEvent;

			base.OnInitialize ();
		}

		public override void OnUninitialize ()
		{
			Program.openTK.Keyboard.KeyUp -= KeyboardEvent;
			bitmap2.Dispose ();
			bitmap1.Dispose ();
			sprite.Dispose ();
			base.OnUninitialize ();
		}

		public override void OnUpdate ( GameTime gameTime )
		{
			base.OnUpdate ( gameTime );
		}

		public override void OnPostProcess ()
		{
			if ( IsPostProcessingMode )
			{
				base.OnPostProcess ();
				ChangeTitle ();
			}
		}

		public override void OnDraw ( GameTime gameTime )
		{
			Program.d3dDevice.ImmediateContext.ClearRenderTargetView ( Program.d3dRenderTargetView, new SharpDX.Mathematics.Interop.RawColor4 ( 0, 0, 0, 1 ) );
			
			base.OnDraw ( gameTime );
		}

		public override void OnPostDraw ()
		{
			foreach ( var context in contexts )
			{
				commandLists.Enqueue ( context.Value.FinishCommandList ( false ) );
				context.Value.Dispose ();
			}
			contexts.Clear ();

			foreach ( var commandList in commandLists )
			{
				Program.d3dDevice.ImmediateContext.ExecuteCommandList ( commandList, false );
				commandList.Dispose ();
			}
			commandLists.Clear ();

			Program.dxgiSwapChain.Present ( 1, SharpDX.DXGI.PresentFlags.None );
			Thread.Sleep ( 0 );
		}
	}
}
