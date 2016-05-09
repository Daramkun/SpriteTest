using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D9;

namespace SpriteTest
{
	public class SpriteDX9 : StandardDispose, ISprite
	{
		VertexBuffer vertexBuffer;
		VertexShader vertexShader;
		PixelShader pixelShader;
		ConstantTable pixelShaderConstantTable;
		EffectHandle texHandle;
		VertexDeclaration vertexDeclaration;

		int vertexSize = Marshal.SizeOf<Vertex> ();

		public SpriteDX9 ()
		{
			var vertices = new Vertex []
			{
				new Vertex { Position = new Vector3 ( 0, 0, 0 ), TexCoord = new Vector2 ( 0, 0 ) },
				new Vertex { Position = new Vector3 ( 0, 1, 0 ), TexCoord = new Vector2 ( 0, 1 ) },
				new Vertex { Position = new Vector3 ( 1, 0, 0 ), TexCoord = new Vector2 ( 1, 0 ) },
				new Vertex { Position = new Vector3 ( 1, 1, 0 ), TexCoord = new Vector2 ( 1, 1 ) },
			};

			vertexBuffer = new VertexBuffer ( Program.d3dDevice9, Marshal.SizeOf<Vertex> () * 4, Usage.None,
				VertexFormat.Position | VertexFormat.Texture1, Pool.Managed );
			var vbStream = vertexBuffer.Lock ( 0, Marshal.SizeOf<Vertex> () * 4, LockFlags.None );
			vbStream.WriteRange<Vertex> ( vertices );
			vbStream.Dispose ();
			vertexBuffer.Unlock ();

			var compiledVertexShader = SharpDX.Direct3D9.ShaderBytecode.Compile ( @"
float4x4 world : WORLD;
float4x4 proj : PROJECTION;
float4 ov;

struct VERTEX_IN
{
	float3 position : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct VERTEX_OUT
{
	float4 position : POSITION;
	float4 diffuse : COLOR;
	float2 texcoord : TEXCOORD0;
};

VERTEX_OUT main( VERTEX_IN pos )
{
	VERTEX_OUT outValue;
	outValue.position = float4 ( pos.position, 1 );
	outValue.position = mul ( outValue.position, world );
	outValue.position = mul ( outValue.position, proj );
	outValue.diffuse = ov;
	outValue.texcoord = pos.texcoord;
	return outValue;
}
", "main", "vs_2_0", ShaderFlags.None );
			vertexShader = new VertexShader ( Program.d3dDevice9, compiledVertexShader );
			var compiledPixelShader = SharpDX.Direct3D9.ShaderBytecode.Compile ( @"
sampler2D tex;

struct PIXEL_IN
{
	float4 position : SV_POSITION;
	float4 diffuse : COLOR;
	float2 texcoord : TEXCOORD0;
};

float4 main( PIXEL_IN inVal ) : COLOR
{
	return tex2D ( tex, inVal.texcoord ) * inVal.diffuse;
}
", "main", "ps_2_0", ShaderFlags.None );
			pixelShaderConstantTable = compiledPixelShader.Bytecode.ConstantTable;
			texHandle = pixelShaderConstantTable.GetConstantByName ( null, "tex" );
            pixelShader = new PixelShader ( Program.d3dDevice9, compiledPixelShader );

			vertexDeclaration = new VertexDeclaration ( Program.d3dDevice9, new VertexElement []
			{
				new VertexElement ( 0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0 ),
				new VertexElement ( 0, sizeof ( float ) * 3, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0 ),
				VertexElement.VertexDeclarationEnd,
			} );
        }

		protected override void Dispose ( bool disposing )
		{
			vertexDeclaration.Dispose ();
			pixelShader.Dispose ();
			vertexShader.Dispose ();
			vertexBuffer.Dispose ();
			base.Dispose ( disposing );
		}

		public void Draw ( IBitmap bitmap, World2 world, IDrawer drawer, object context )
		{
			Program.d3dDevice9.SetRenderState ( RenderState.CullMode, Cull.None );
			//Program.d3dDevice9.SetRenderState ( RenderState.Lighting, false );
			//Program.d3dDevice9.VertexFormat = VertexFormat.Position | VertexFormat.Texture1;

			Program.d3dDevice9.VertexDeclaration = vertexDeclaration;
			Program.d3dDevice9.SetStreamSource ( 0, vertexBuffer, 0, vertexSize );
			Program.d3dDevice9.SetTexture ( pixelShaderConstantTable.GetSamplerIndex( texHandle ), ( bitmap as BitmapDX9 ).texture );

			Program.d3dDevice9.VertexShader = vertexShader;
			Program.d3dDevice9.PixelShader = pixelShader;

			drawer.SetConstant ( bitmap, world, context );

			Program.d3dDevice9.DrawPrimitives ( PrimitiveType.TriangleStrip, 0, 2 );
        }
	}
}
