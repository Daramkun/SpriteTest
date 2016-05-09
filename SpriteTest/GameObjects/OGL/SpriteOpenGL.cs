using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace SpriteTest
{
	public class SpriteOpenGL : StandardDispose, ISprite
	{
		int vertexArrayObject;
		int vertexBufferObject;

		int vertexShader, fragmentShader;
		int program;

		public SpriteOpenGL ()
		{
			GL.CreateVertexArrays ( 1, out vertexArrayObject );
			GL.BindVertexArray ( vertexArrayObject );

			vertexBufferObject = GL.GenBuffer ();
			GL.BindBuffer ( BufferTarget.ArrayBuffer, vertexBufferObject );

			GL.BufferData<Vertex> ( BufferTarget.ArrayBuffer, Marshal.SizeOf<Vertex> () * 4, new Vertex []
			{
				new Vertex { Position = new Vector3 ( 0, 0, 0 ), TexCoord = new Vector2 ( 0, 1 ) },
				new Vertex { Position = new Vector3 ( 0, 1, 0 ), TexCoord = new Vector2 ( 0, 0 ) },
				new Vertex { Position = new Vector3 ( 1, 0, 0 ), TexCoord = new Vector2 ( 1, 1 ) },
				new Vertex { Position = new Vector3 ( 1, 1, 0 ), TexCoord = new Vector2 ( 1, 0 ) },
			}, BufferUsageHint.StaticDraw );

			GL.EnableVertexAttribArray ( 0 );
			GL.VertexAttribPointer ( 0, 3, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex> (), 0 );
			GL.EnableVertexAttribArray ( 1 );
			GL.VertexAttribPointer ( 1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex> (), 4 * 3 );

			GL.BindVertexArray ( 0 );

			vertexShader = GL.CreateShader ( ShaderType.VertexShader );
			fragmentShader = GL.CreateShader ( ShaderType.FragmentShader );

			GL.ShaderSource ( vertexShader, @"#version 330
layout ( location = 0 ) in vec3 inpos;
layout ( location = 1 ) in vec2 intex;

out vec2 outtex;
out vec4 outcol;

layout ( std140 ) uniform transform
{
	mat4 world;
	mat4 proj;
	vec4 ov;
};

void main ()
{
    vec4 pos = vec4(inpos, 1);
    pos = world * pos;
    pos = proj * pos;

    gl_Position = pos;
	
	outtex = intex;
	outcol = ov;
}" );
			GL.ShaderSource ( fragmentShader, @"#version 330
in vec2 outtex;
in vec4 outcol;

uniform sampler2D tex;

out vec4 fragColor;

void main ()
{
	fragColor = texture ( tex, outtex ) * outcol;
}
" );
			GL.CompileShader ( vertexShader );
			GL.CompileShader ( fragmentShader );

			Debug.WriteLine ( GL.GetShaderInfoLog ( vertexShader ) );
			Debug.WriteLine ( GL.GetShaderInfoLog ( fragmentShader ) );

			program = GL.CreateProgram ();
			GL.AttachShader ( program, vertexShader );
			GL.AttachShader ( program, fragmentShader );

			GL.BindFragDataLocation ( program, 0, "fragColor" );

			GL.LinkProgram ( program );

			Debug.WriteLine ( GL.GetProgramInfoLog ( program ) );
		}

		protected override void Dispose ( bool disposing )
		{
			GL.DeleteShader ( vertexShader );
			GL.DeleteShader ( fragmentShader );
			GL.DeleteProgram ( program );

			GL.DeleteBuffer ( vertexBufferObject );
			GL.DeleteVertexArray ( vertexArrayObject );

			base.Dispose ( disposing );
		}

		public void Draw ( IBitmap bitmap, World2 world, IDrawer drawer, object context )
		{
			GL.UseProgram ( program );

			GL.Enable ( EnableCap.Texture2D );
			GL.Enable ( EnableCap.DepthTest );
			GL.Disable ( EnableCap.CullFace );

			GL.UniformBlockBinding ( program, GL.GetUniformBlockIndex ( program, "transform" ), 0 );
			drawer.SetConstant ( bitmap, world, context );

			GL.BindTexture ( TextureTarget.Texture2D, ( bitmap as BitmapOpenGL ).texture );
			GL.Uniform1 ( GL.GetUniformLocation ( program, "tex" ), 0 );

			GL.BindVertexArray ( vertexArrayObject );
			GL.DrawArrays ( PrimitiveType.TriangleStrip, 0, 4 );
			GL.BindVertexArray ( 0 );
		}
	}
}
