using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;

namespace SpriteTest
{
	public class BitmapDX9 : StandardDispose, IBitmap
	{
		internal Texture texture;

		public Vector2 Size { get; private set; }

		public BitmapDX9 ( Stream stream )
		{
			Bitmap image = new Bitmap ( stream );

			texture = new Texture ( Program.d3dDevice9, image.Width, image.Height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed );
			var rect = texture.LockRectangle ( 0, new SharpDX.Mathematics.Interop.RawRectangle ( 0, 0, image.Width, image.Height ), LockFlags.None );
			SharpDX.DataStream dataStream = new SharpDX.DataStream ( rect.DataPointer, image.Width * image.Height * 4, false, true );
			SharpDX.Mathematics.Interop.RawColorBGRA [] colours = new SharpDX.Mathematics.Interop.RawColorBGRA [ image.Width * image.Height ];
			int index = 0;
			for ( int i = 0; i < image.Height; i++ )
				for ( int j = 0; j < image.Width; j++ )
				{
					var argb = image.GetPixel ( j, i );
                    colours [ index++ ] = new SharpDX.Mathematics.Interop.RawColorBGRA ( argb.B, argb.G, argb.R, argb.A );
				}
			dataStream.WriteRange<SharpDX.Mathematics.Interop.RawColorBGRA> ( colours, 0, image.Width * image.Height );
			texture.UnlockRectangle ( 0 );

			Size = new Vector2 ( image.Width, image.Height );
		}

		protected override void Dispose ( bool disposing )
		{
			texture.Dispose ();
			base.Dispose ( disposing );
		}
	}
}
