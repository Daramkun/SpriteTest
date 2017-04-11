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

namespace SpriteTest.GameObjects.DX12
{
	public class BitmapDX12 : StandardDispose, IBitmap
	{
		SharpDX.Direct3D12.Resource texture;

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

			texture = Program.d3dDevice12.CreateCommittedResource ( new SharpDX.Direct3D12.HeapProperties ( SharpDX.Direct3D12.HeapType.Default ),
				SharpDX.Direct3D12.HeapFlags.None,
				SharpDX.Direct3D12.ResourceDescription.Texture2D ( SharpDX.DXGI.Format.R8G8B8A8_UNorm, image.Width, image.Height ),
				SharpDX.Direct3D12.ResourceStates.CopyDestination );

			dataStream2.Dispose ();
			dataStream.Dispose ();

			image.UnlockBits ( data );
			image.Dispose ();
		}

		public BitmapDX12 ( Stream stream )
		{
			GetImageRawData ( stream );
			
		}

		protected override void Dispose ( bool disposing )
		{
			texture.Dispose ();
			base.Dispose ( disposing );
		}
	}
}
