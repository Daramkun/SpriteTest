using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	[StructLayout ( LayoutKind.Sequential )]
	struct UniformBuffer
	{
		public Matrix4x4 World;
		public Matrix4x4 Projection;
		public Vector4 OverlayColor;
	}
}
