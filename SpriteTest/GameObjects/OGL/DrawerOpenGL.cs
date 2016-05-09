using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace SpriteTest
{
	public class DrawerOpenGL : StandardDispose, IDrawer
	{
		internal int uniformBufferObject;

		public DrawerOpenGL()
		{
			uniformBufferObject = GL.GenBuffer ();
			GL.BindBuffer ( BufferTarget.UniformBuffer, uniformBufferObject );
			GL.BufferData ( BufferTarget.UniformBuffer, ( sizeof ( float ) * 16 ) * 2 + ( sizeof ( float ) * 4 ), IntPtr.Zero, BufferUsageHint.StaticDraw );
			GL.BindBuffer ( BufferTarget.UniformBuffer, 0 );
		}

		protected override void Dispose ( bool disposing )
		{
			//GL.DeleteBuffer ( uniformBufferObject );
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
			GL.BindBuffer ( BufferTarget.UniformBuffer, uniformBufferObject );
			GL.BufferData<UniformBuffer> ( BufferTarget.UniformBuffer, ( sizeof ( float ) * 16 ) * 2 + ( sizeof ( float ) * 4 ), ref data, BufferUsageHint.StaticDraw );
			GL.BindBuffer ( BufferTarget.UniformBuffer, 0 );

			GL.BindBufferRange ( BufferRangeTarget.UniformBuffer, 0, uniformBufferObject, IntPtr.Zero,
				( sizeof ( float ) * 16 ) * 2 + ( sizeof ( float ) * 4 ) );
		}
	}
}
