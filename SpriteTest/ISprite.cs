using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public interface ISprite : IDisposable
	{
		void Draw ( IBitmap bitmap, World2 world, IDrawer drawer, object context );
	}
}
