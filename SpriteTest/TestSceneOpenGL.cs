using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace SpriteTest
{
	class TestSceneOpenGL : Scene, ITestScene
	{
		ISprite sprite;
		IBitmap bitmap1, bitmap2;

		public ISprite Sprite { get { return sprite; } }

		public TestSceneOpenGL () { IsParallelUpdate = true; }

		private void ChangeTitle () { Program.openTK.Title = $"SpriteTest OpenGL: {Children.Count}"; }

		private void KeyboardEvent ( object sender, KeyboardKeyEventArgs e )
		{
			switch ( e.Key )
			{
				case Key.Q: Children.Add ( new SpriteObject ( bitmap1 ) ); break;
				case Key.W: Children.Add ( new SpriteObject ( bitmap2 ) ); break;
				case Key.Number1: for ( int i = 0; i < 100; ++i ) Children.Add ( new SpriteObject ( bitmap1 ) ); break;
				case Key.Number2: for ( int i = 0; i < 100; ++i ) Children.Add ( new SpriteObject ( bitmap2 ) ); break;

				case Key.A: Children.Remove ( Children [ 0 ] ); break;
				case Key.S: Children.Remove ( Children [ 0 ] ); break;
				case Key.Z: for ( int i = 0; i < 100; ++i ) Children.Remove ( Children [ i ] ); break;
				case Key.X: for ( int i = 0; i < 100; ++i ) Children.Remove ( Children [ i ] ); break;
			}
		}

		public override void OnInitialize ()
		{
			sprite = new SpriteOpenGL ();
			var assembly = Assembly.GetEntryAssembly ();
			bitmap1 = new BitmapOpenGL ( assembly.GetManifestResourceStream ( "SpriteTest.Resources.Test1.jpg" ) );
			bitmap2 = new BitmapOpenGL ( assembly.GetManifestResourceStream ( "SpriteTest.Resources.Test2.jpg" ) );

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
			GL.ClearColor ( 0, 0, 0, 1 );
			GL.ClearDepth ( 1 );
			GL.Clear ( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );

			base.OnDraw ( gameTime );

			Program.openTK.SwapBuffers ();
		}
	}
}
