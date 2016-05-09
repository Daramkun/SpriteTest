using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class DrawerDX9 : StandardDispose, IDrawer
	{
		static SharpDX.Direct3D9.ConstantTable cachedCt;
		private static SharpDX.Direct3D9.ConstantTable GetConstantTable ()
		{
			if ( cachedCt == null)
				cachedCt = Program.d3dDevice9.VertexShader.Function.ConstantTable;
			return cachedCt;
        }

		static Dictionary<string, SharpDX.Direct3D9.EffectHandle> handleCache = new Dictionary<string, SharpDX.Direct3D9.EffectHandle> ();

		private static SharpDX.Direct3D9.EffectHandle GetHandle ( string name )
		{
			SharpDX.Direct3D9.EffectHandle handle;
			if ( !handleCache.TryGetValue ( name, out handle ) )
			{
				handle = GetConstantTable ().GetConstantByName ( null, name );
				handleCache.Add ( name, handle );
			}
			return handle;
		}

		protected override void Dispose ( bool disposing )
		{
			//foreach ( var pair in handleCache )
			//	pair.Value.Dispose ();
			base.Dispose ( disposing );
		}

		public void SetConstant ( IBitmap bitmap, World2 world, object ctx )
		{
			UniformBuffer data = new UniformBuffer
			{
				Projection = Matrix4x4.CreateOrthographicOffCenter ( 0, 800, 600, 0, -100000.0f, 100000.0f ),
				OverlayColor = new Vector4 ( 1, 1, 1, 1 )
			};
			world.GetMatrix ( out data.World, bitmap.Size );

			var constantTable = GetConstantTable ();
			constantTable.SetValue<Matrix4x4> ( Program.d3dDevice9, GetHandle ( "world" ), data.World );
			constantTable.SetValue<Matrix4x4> ( Program.d3dDevice9, GetHandle ( "proj" ), data.Projection );
			constantTable.SetValue<Vector4> ( Program.d3dDevice9, GetHandle ( "ov" ), data.OverlayColor );
		}
	}
}
