using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class BitmapDX11 : StandardDispose, IBitmap
	{
		SharpDX.Direct3D11.Texture2D texture;
		internal SharpDX.Direct3D11.ShaderResourceView shaderResourceView;
		internal SharpDX.Direct3D11.SamplerState samplerState;

		public Vector2 Size { get; private set; }

		[StructLayout ( LayoutKind.Sequential )]
		struct Color { public byte b, g, r, a; }

		void GetImageRawData ( Stream stream )
		{
			Bitmap image = new Bitmap ( stream );

			Size = new Vector2 ( image.Width, image.Height );
			var data = image.LockBits ( new Rectangle ( new Point (), image.Size ), ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			IntPtr ptr = data.Scan0;
			SharpDX.DataStream dataStream = new SharpDX.DataStream ( image.Width * 4 * image.Height, true, true );
			dataStream.WriteRange ( ptr, image.Width * image.Height * 4 );
			dataStream.Position = 0;
			SharpDX.DataStream dataStream2 = new SharpDX.DataStream ( image.Width * 4 * image.Height, true, true );
			for ( int i = 0; i < image.Width * image.Height; ++i )
			{
				Color v = dataStream.Read<Color> ();
				Color v2 = new Color () { r = v.b, g = v.g, b = v.r, a = v.a };
				dataStream2.Write<Color> ( v2 );
			}

			texture = new SharpDX.Direct3D11.Texture2D ( Program.d3dDevice, new SharpDX.Direct3D11.Texture2DDescription ()
			{
				Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
				Width = image.Width,
				Height = image.Height,
				ArraySize = 1,
				MipLevels = 1,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				SampleDescription = new SharpDX.DXGI.SampleDescription ( 1, 0 ),
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read | SharpDX.Direct3D11.CpuAccessFlags.Write,
				BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.RenderTarget,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None
			}, new SharpDX.DataRectangle ( dataStream2.DataPointer, image.Width * 4 ) );

			dataStream2.Dispose ();
			dataStream.Dispose ();
			
			image.UnlockBits ( data );
			image.Dispose ();
		}

		public BitmapDX11 ( Stream stream )
		{
			GetImageRawData ( stream );
			shaderResourceView = new SharpDX.Direct3D11.ShaderResourceView ( Program.d3dDevice, texture, new SharpDX.Direct3D11.ShaderResourceViewDescription ()
			{
				Texture2D = new SharpDX.Direct3D11.ShaderResourceViewDescription.Texture2DResource () { MipLevels = 1 },
				Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
				Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
			} );
			samplerState = new SharpDX.Direct3D11.SamplerState ( Program.d3dDevice, new SharpDX.Direct3D11.SamplerStateDescription ()
			{
				AddressU = SharpDX.Direct3D11.TextureAddressMode.Wrap,
				AddressV = SharpDX.Direct3D11.TextureAddressMode.Wrap,
				AddressW = SharpDX.Direct3D11.TextureAddressMode.Wrap,
				Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint,
				ComparisonFunction = SharpDX.Direct3D11.Comparison.Never,
				MaximumAnisotropy = 1,
				MinimumLod = 0,
				MaximumLod = float.MaxValue,
				MipLodBias = 0,
			} );
		}

		protected override void Dispose ( bool disposing )
		{
			samplerState.Dispose ();
			shaderResourceView.Dispose ();
			texture.Dispose ();
			base.Dispose ( disposing );
		}
	}
}
