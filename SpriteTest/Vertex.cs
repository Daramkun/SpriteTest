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
	struct Vertex
	{
		public Vector3 Position;
		public Vector2 TexCoord;
	}
}
