using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class Scene : GameObject
	{
		public virtual bool IsParallelUpdate { get; set; } = true;
		public virtual bool IsParallelDraw { get; set; } = false;

		public override void OnUpdate ( GameTime gameTime )
		{
			var contains = from o in GetContains where o.IsEnabled select o;
			if ( IsParallelUpdate ) Parallel.ForEach<GameObject> ( contains, ( gameObject ) => gameObject.OnUpdate ( gameTime ) );
			else foreach ( var gameObject in contains ) gameObject.OnUpdate ( gameTime );
		}

		public override void OnPostProcess ()
		{
			var contains = from o in GetContains where o.IsPostProcessingMode select o;
			Parallel.ForEach<GameObject> ( contains, ( gameObject ) => gameObject.OnPostProcess () );

			base.OnPostProcess ();
		}

		static ParallelOptions parallelOptions = new ParallelOptions ()
		{
			MaxDegreeOfParallelism = Environment.ProcessorCount,
		};

		public override void OnDraw ( GameTime gameTime )
		{
			var contains = from o in GetContains where o.IsVisible select o;
			if ( IsParallelDraw )
				Task.WaitAll ( Task.Run (
					() => Parallel.ForEach<GameObject> (
						contains,
						parallelOptions,
						( gameObject ) => gameObject.OnDraw ( gameTime )
					)
				) );
			else foreach ( var gameObject in contains ) gameObject.OnDraw ( gameTime );
		}

		public virtual void OnPostDraw () { }
	}
}
