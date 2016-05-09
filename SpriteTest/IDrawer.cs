using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public interface IDrawer : IDisposable
	{
		void SetConstant ( IBitmap bitmap, World2 world, object ctx );
	}
}
