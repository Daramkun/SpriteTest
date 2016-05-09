using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace SpriteTest
{
	public class BitmapOpenGL : StandardDispose, IBitmap
	{
		internal int texture;

		public Vector2 Size { get; private set; }

		void GetImageRawData ( int textureId, Stream stream )
		{
			Bitmap image = new Bitmap ( stream );
			Size = new Vector2 ( image.Width, image.Height );
			var data = image.LockBits ( new Rectangle ( new Point (), image.Size ), ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ( int ) TextureMinFilter.Nearest );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ( int ) TextureMagFilter.Nearest );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ( int ) TextureWrapMode.Repeat );
			GL.TexParameter ( TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ( int ) TextureWrapMode.Repeat );

			IntPtr ptr = data.Scan0;

			GL.TexImage2D ( TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero );
			for ( int i = 0; i < image.Height; ++i )
				GL.TexSubImage2D ( TextureTarget.Texture2D, 0, 0, image.Height - i - 1, image.Width, 1, 
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0 + ( i * image.Width * 4 ) );

			image.UnlockBits ( data );
			image.Dispose ();
		}

		public BitmapOpenGL ( Stream stream )
		{
			texture = GL.GenTexture ();
			GL.BindTexture ( TextureTarget.Texture2D, texture );
			GetImageRawData ( texture, stream );
		}

		protected override void Dispose ( bool disposing )
		{
			GL.DeleteTexture ( texture );
			base.Dispose ( disposing );
		}
	}
}
