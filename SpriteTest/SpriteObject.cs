using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class SpriteObject : GameObject
	{
		IBitmap bitmap;
		World2 world;
		IDrawer drawer;

		float unit;

		public SpriteObject ( IBitmap bitmap )
		{
			this.bitmap = bitmap;
		}

		public override void OnInitialize ()
		{
			world = World2.Identity;
			world.Translate = new Vector2 ( Program.rand.Next () % 800, Program.rand.Next () % 600 );
			world.RotationCenter = bitmap.Size / 2;
			unit = ( float ) Program.rand.NextDouble ();
			drawer = ( Parent is TestSceneDX11 ) ? new DrawerDX11 () : new DrawerOpenGL () as IDrawer;
		}

		protected override void Dispose ( bool disposing )
		{
			drawer.Dispose ();
			base.Dispose ( disposing );
		}

		public override void OnUpdate ( GameTime gameTime )
		{
			world.Rotation += ( float ) gameTime.ElapsedGameTime.TotalSeconds * unit;
		}

		public override void OnDraw ( GameTime gameTime )
		{
			var context = ( ( Parent is TestSceneDX11 ) ? ( Parent as TestSceneDX11 ).GetDeviceContext () : null );
			( Parent as ITestScene ).Sprite.Draw ( bitmap, world, drawer, context );
		}
	}
}
