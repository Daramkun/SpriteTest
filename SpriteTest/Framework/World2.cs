using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class World2
	{
		public Vector3 Translate;
		public Vector2 ScaleCenter;
		public Vector2 Scale;
		public float Rotation;
		public Vector2 RotationCenter;

		public static World2 Identity
		{
			get { return new World2 () { Scale = new Vector2 ( 1, 1 ) }; }
		}

		public World2 ()
		{
			Scale = new Vector2 ( 1 );
		}

		public World2 ( Vector3 translate, Vector2 scale, Vector2 scaleCenter, float rotation, Vector2 rotationCenter )
			: this ()
		{
			Translate = translate;
			ScaleCenter = scaleCenter;
			Scale = scale;
			Rotation = rotation;
			RotationCenter = rotationCenter;
		}

		public static World2 operator + ( World2 v1, World2 v2 )
		{
			return new World2 ( v1.Translate + v2.Translate, v1.ScaleCenter + v2.ScaleCenter,
				v1.Scale * v2.Scale, v1.Rotation + v2.Rotation, v1.RotationCenter + v2.RotationCenter );
		}

		public static World2 operator - ( World2 v1, World2 v2 )
		{
			return new World2 ( v1.Translate - v2.Translate, v1.ScaleCenter - v2.ScaleCenter,
				v1.Scale / v2.Scale, v1.Rotation - v2.Rotation, v1.RotationCenter - v2.RotationCenter );
		}
		
		public void GetMatrix ( out Matrix4x4 result, Vector2 size )
		{
			result = Matrix4x4.CreateScale ( new Vector3 ( size, 1 ) );
			result *= Matrix4x4.CreateTranslation ( new Vector3 ( -RotationCenter, 0 ) );
			result *= Matrix4x4.CreateRotationZ ( Rotation );
			result *= Matrix4x4.CreateTranslation ( new Vector3 ( RotationCenter, 0 ) );
			result *= Matrix4x4.CreateTranslation ( new Vector3 ( -ScaleCenter, 0 ) );
			result *= Matrix4x4.CreateScale ( new Vector3 ( Scale, 1 ) );
			result *= Matrix4x4.CreateTranslation ( new Vector3 ( ScaleCenter, 0 ) );
			result *= Matrix4x4.CreateTranslation ( Translate );
		}
	}
}
