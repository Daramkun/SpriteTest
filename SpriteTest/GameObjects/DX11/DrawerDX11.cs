using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class DrawerDX11 : StandardDispose, IDrawer
	{
		SharpDX.Direct3D11.Buffer constantBuffer;

		public DrawerDX11 ()
		{
			constantBuffer = new SharpDX.Direct3D11.Buffer ( Program.d3dDevice11, new SharpDX.Direct3D11.BufferDescription ()
			{
				Usage = SharpDX.Direct3D11.ResourceUsage.Dynamic,
				BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Write,
				SizeInBytes = sizeof ( float ) * 16 * 2 + sizeof ( float ) * 4,
			} );
		}

		protected override void Dispose ( bool disposing )
		{
			constantBuffer.Dispose ();
			base.Dispose ( disposing );
		}

		public void SetConstant ( IBitmap bitmap, World2 world, object ctx )
		{
			SharpDX.Direct3D11.DeviceContext context = ctx as SharpDX.Direct3D11.DeviceContext;

			UniformBuffer data = new UniformBuffer
			{
				Projection = Matrix4x4.CreateOrthographicOffCenter ( 0, 800, 600, 0, -100000.0f, 100000.0f ),
				OverlayColor = new Vector4 ( 1, 1, 1, 0.5f )
			};
			world.GetMatrix ( out data.World, bitmap.Size );
			//context.UpdateSubresource<UniformBuffer> ( ref data, constantBuffer );
			SharpDX.DataStream stream;
			context.MapSubresource ( constantBuffer, SharpDX.Direct3D11.MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream );
			stream.Write<UniformBuffer> ( data );
			context.UnmapSubresource ( constantBuffer, 0 );
			context.VertexShader.SetConstantBuffer ( 0, constantBuffer );
		}
	}
}
