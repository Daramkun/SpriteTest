using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class SpriteDX11 : StandardDispose, ISprite
	{
		SharpDX.Direct3D11.Buffer vertexBuffer;
		SharpDX.Direct3D11.VertexBufferBinding vertexBufferBinding;
		SharpDX.Direct3D11.InputLayout inputLayout;
		SharpDX.Direct3D11.VertexShader vertexShader;
		SharpDX.Direct3D11.PixelShader pixelShader;
		SharpDX.Direct3D11.RasterizerState rasterizerState;
		SharpDX.Direct3D11.DepthStencilState depthStencilState;

		public SpriteDX11 ()
		{
			var vertices = new Vertex []
			{
				new Vertex { Position = new Vector3 ( 0, 0, 0 ), TexCoord = new Vector2 ( 0, 0 ) },
				new Vertex { Position = new Vector3 ( 0, 1, 0 ), TexCoord = new Vector2 ( 0, 1 ) },
				new Vertex { Position = new Vector3 ( 1, 0, 0 ), TexCoord = new Vector2 ( 1, 0 ) },
				new Vertex { Position = new Vector3 ( 1, 1, 0 ), TexCoord = new Vector2 ( 1, 1 ) },
			};

			vertexBuffer = new SharpDX.Direct3D11.Buffer ( Program.d3dDevice, new SharpDX.Direct3D11.BufferDescription ()
			{
				SizeInBytes = Marshal.SizeOf<Vertex> () * 4,
				BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
			} );
			Program.d3dDevice.ImmediateContext.UpdateSubresource<Vertex> ( vertices, vertexBuffer );

			vertexBufferBinding = new SharpDX.Direct3D11.VertexBufferBinding ( vertexBuffer, Marshal.SizeOf<Vertex> (), 0 );

			var compiledVertexShader = SharpDX.D3DCompiler.ShaderBytecode.Compile ( @"
cbuffer transform : register ( b0 )
{
	matrix world : WORLD;
	matrix proj : PROJECTION;
	float4 ov;
};

struct VERTEX_IN
{
	float3 position : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct VERTEX_OUT
{
	float4 position : SV_POSITION;
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
", "main", "vs_5_0", SharpDX.D3DCompiler.ShaderFlags.PackMatrixRowMajor );
			vertexShader = new SharpDX.Direct3D11.VertexShader ( Program.d3dDevice, compiledVertexShader );
			pixelShader = new SharpDX.Direct3D11.PixelShader ( Program.d3dDevice, SharpDX.D3DCompiler.ShaderBytecode.Compile ( @"
Texture2D tex : register ( t0 );
SamplerState texSampler : register ( s0 );

struct PIXEL_IN
{
	float4 position : SV_POSITION;
	float4 diffuse : COLOR;
	float2 texcoord : TEXCOORD0;
};

float4 main( PIXEL_IN inVal ) : SV_TARGET
{
	return tex.Sample ( texSampler, inVal.texcoord ) * inVal.diffuse;
}
", "main", "ps_5_0" ) );

			inputLayout = new SharpDX.Direct3D11.InputLayout ( Program.d3dDevice, compiledVertexShader, new SharpDX.Direct3D11.InputElement []
			{
				new SharpDX.Direct3D11.InputElement ( "POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0,
					SharpDX.Direct3D11.InputClassification.PerVertexData, 0 ),
				new SharpDX.Direct3D11.InputElement ( "TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, sizeof ( float ) * 3, 0,
					SharpDX.Direct3D11.InputClassification.PerVertexData, 0 ),
			} );

			rasterizerState = new SharpDX.Direct3D11.RasterizerState ( Program.d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription ()
			{
				CullMode = SharpDX.Direct3D11.CullMode.None,
				FillMode = SharpDX.Direct3D11.FillMode.Solid,
				IsFrontCounterClockwise = true,
				IsDepthClipEnabled = true,
			} );

			depthStencilState = new SharpDX.Direct3D11.DepthStencilState ( Program.d3dDevice, new SharpDX.Direct3D11.DepthStencilStateDescription ()
			{
				IsDepthEnabled = false,
				IsStencilEnabled = false,
			} );
		}

		protected override void Dispose ( bool disposing )
		{
			depthStencilState.Dispose ();
			rasterizerState.Dispose ();
			inputLayout.Dispose ();
			pixelShader.Dispose ();
			vertexShader.Dispose ();
			vertexBuffer.Dispose ();
			base.Dispose ( disposing );
		}

		public void Draw ( IBitmap bitmap, World2 world, IDrawer drawer, object ctx )
		{
			SharpDX.Direct3D11.DeviceContext context = ctx as SharpDX.Direct3D11.DeviceContext;

			context.InputAssembler.InputLayout = inputLayout;
			context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
			context.InputAssembler.SetVertexBuffers ( 0, vertexBufferBinding );

			context.OutputMerger.SetDepthStencilState ( depthStencilState );
			context.Rasterizer.State = rasterizerState;

			context.VertexShader.SetShader ( vertexShader, null, 0 );
			context.PixelShader.SetShader ( pixelShader, null, 0 );
			context.PixelShader.SetShaderResource ( 0, ( bitmap as BitmapDX11 ).shaderResourceView );
			context.PixelShader.SetSampler ( 0, ( bitmap as BitmapDX11 ).samplerState );
			drawer.SetConstant ( bitmap, world, ctx );

			context.Draw ( 4, 0 );
		}
	}
}
