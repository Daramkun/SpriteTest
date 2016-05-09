using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteTest
{
	public class GameObject : StandardDispose
	{
		bool isAlreadyUninitialized = false;

		public bool IsEnabled { get; set; } = true;
		public bool IsVisible { get; set; } = true;

		public bool IsPostProcessingMode { get; set; } = false;

		WeakReference<GameObject> _parent;
		public GameObject Parent
		{
			get { if ( _parent == null ) return null; GameObject target; _parent.TryGetTarget ( out target ); return target; }
			set { _parent = new WeakReference<GameObject> ( value ); }
		}
		public GameObjectCollection Children { get; private set; }

		public GameObject () { Children = new GameObjectCollection ( this ); }

		protected override void Dispose ( bool disposing )
		{
			if ( disposing && !isAlreadyUninitialized )
				OnUninitialize ();

			base.Dispose ( disposing );
		}

		public virtual void OnInitialize () { }
		public virtual void OnUninitialize () { isAlreadyUninitialized = true; }
		public virtual void OnUpdate ( GameTime gameTime ) { }
		public virtual void OnPostProcess () { Children.Fetch (); IsPostProcessingMode = false; }
		public virtual void OnDraw ( GameTime gameTime ) { }

		public IEnumerable<GameObject> GetContains
		{
			get
			{
				foreach ( var child in Children )
				{
					yield return child;
					if ( child.Children.Count > 0 )
						foreach ( var c in child.GetContains )
							yield return c;
				}
			}
		}

		public class GameObjectCollection : StandardDispose, IReadOnlyList<GameObject>
		{
			public List<GameObject> GameObjects { get; private set; } = new List<GameObject> ();
			ConcurrentBag<GameObject> AddObjects { get; set; } = new ConcurrentBag<GameObject> ();
			ConcurrentBag<GameObject> RemoveObjects { get; set; } = new ConcurrentBag<GameObject> ();

			public GameObject this [ int index ] { get { AssertDisposed (); return GameObjects [ index ]; } }
			public int Count { get { AssertDisposed (); return GameObjects.Count; } }

			public IEnumerator<GameObject> GetEnumerator () { AssertDisposed (); return GameObjects.GetEnumerator (); }
			IEnumerator IEnumerable.GetEnumerator () { AssertDisposed (); return GameObjects.GetEnumerator (); }

			public GameObject Parent { get; private set; }

			public GameObjectCollection ( GameObject parent ) { Parent = parent; }

			protected override void Dispose ( bool disposing )
			{
				if ( disposing )
				{
					foreach ( var gameObject in GameObjects )
						Remove ( gameObject );
					Fetch ( true, false );
				}

				base.Dispose ( disposing );
			}

			public GameObject Add ( GameObject child )
			{
				AssertDisposed ();
				if ( child.Parent != null ) throw new ArgumentException ();
				if ( AddObjects.Contains ( child ) ) return null;
				AddObjects.Add ( child );
				Parent.IsPostProcessingMode = true;
				return child;
			}

			public void Remove ( GameObject child )
			{
				AssertDisposed ();
				if ( RemoveObjects.Contains ( child ) ) return;
				if ( !GameObjects.Contains ( child ) ) return;
				RemoveObjects.Add ( child );
				Parent.IsPostProcessingMode = true;
			}

			public void Fetch ( bool removeFetch = true, bool addFetch = true )
			{
				AssertDisposed ();

				if ( removeFetch )
				{
					while ( !RemoveObjects.IsEmpty )
					{
						GameObject removeObject;
						RemoveObjects.TryTake ( out removeObject );
						removeObject.OnUninitialize ();
						removeObject.Parent = null;
						GameObjects.Remove ( removeObject );
					}
				}

				if ( addFetch )
				{
					while ( AddObjects.Count > 0 )
					{
						GameObject addObject;
						AddObjects.TryTake ( out addObject );
						if ( addObject.Parent != null )
							throw new OperationCanceledException ();
						addObject.Parent = Parent;
						GameObjects.Add ( addObject );
						addObject.OnInitialize ();
					}
				}
			}
		}
	}
}
