﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbJamTotem
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using PastaGameLibrary;
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

		public class DrawingList : IPDrawable
		{
			struct DrawListData
			{
				public float depth;
				public GameObject drawable;
			};
			List<DrawListData> m_drawables = new List<DrawListData>();

			public void Add(GameObject gameObject, float depth)
			{
				DrawListData newData;

				if (!gameObject.IsInitialised)
					gameObject.Initialise();
				gameObject.m_drawingList = this;

				int i;
				for (i = 0; i < m_drawables.Count; ++i)
					if (depth < m_drawables[i].depth)
						break;

				newData.depth = depth;
				newData.drawable = gameObject;
				m_drawables.Insert(i, newData);
			}

			public void Remove(GameObject gameObject)
			{
				m_drawables.RemoveAll(dld => dld.drawable == gameObject);
				gameObject.m_drawingList = null;
			}
			public void Draw()
			{
				for (int i = 0; i < m_drawables.Count; ++i)
					m_drawables[i].drawable.Draw();
			}
		}
		public class UpdateList : IPUpdatable
		{
			struct UpdateListData
			{
				public float depth;
				public GameObject updatable;
			}
			List<UpdateListData> m_updatables = new List<UpdateListData>();

			public void Clear()
			{
				m_updatables.Clear();
			}

			public void Add(GameObject gameObject, float depth)
			{
				UpdateListData newData;

				if (!gameObject.IsInitialised)
					gameObject.Initialise();
				gameObject.m_updateList = this;

				int i;
				for (i = 0; i < m_updatables.Count; ++i)
					if (depth < m_updatables[i].depth)
						break;

				newData.depth = depth;
				newData.updatable = gameObject;
				m_updatables.Insert(i, newData);
			}

			public void Remove(GameObject gameObject)
			{
				m_updatables.RemoveAll(dld => dld.updatable == gameObject);
				gameObject.m_updateList = null;
			}

			public void Update()
			{
				for (int i = 0; i < m_updatables.Count; ++i)
					m_updatables[i].updatable.Update();
			}
		}

		public enum ActorState
		{
			StandBy,
			Active,
			Destroyed,
		}


		public class ObjectState
		{
			ActorState m_state;
			public delegate void StateAction();
			StateAction m_onStandby, m_onDestroy, m_onBegin;

			public ActorState State
			{
				get { return m_state; }
			}
			public StateAction StandbyAction
			{
				set { m_onStandby = value; }
			}
			public StateAction DestroyAction
			{
				set { m_onDestroy = value; }
			}
			public StateAction BeginAction
			{
				set { m_onBegin = value; }
			}

			public void Standby()
			{
				m_state = ActorState.StandBy;
				if (m_onStandby != null)
					m_onStandby();
			}
			public void Destroy()
			{
				m_state = ActorState.Destroyed;
				if (m_onDestroy != null)
					m_onDestroy();
			}
			public void Begin()
			{
				m_state = ActorState.Active;
				if (m_onBegin != null)
					m_onBegin();
			}
		}

		public abstract class GameObject : IPUpdatable, IPDrawable
		{
			protected Transform m_transform = new Transform();
			private ObjectState m_objectState = new ObjectState();
			private bool m_isInitialised = false;
			protected Sprite m_sprite = null;

			internal DrawingList m_drawingList = null;
			internal UpdateList m_updateList = null;

			public GameObject(GameObject objectToCopy)
			{
				m_transform = new Transform(objectToCopy.Transform);
				if (objectToCopy.Sprite != null)
					m_sprite = new Sprite(objectToCopy.Sprite, m_transform);
				if (objectToCopy.m_updateList != null)
					m_updateList = objectToCopy.m_updateList;
				if (objectToCopy.m_drawingList != null)
					m_drawingList = objectToCopy.m_drawingList;
			}

			public Transform Transform
			{
				get { return m_transform; }
			}
			public ObjectState ObjectState
			{
				get { return m_objectState; }
			}
			public Sprite Sprite
			{
				get { return m_sprite; }
			}

			public bool IsInitialised
			{
				get { return m_isInitialised; }
			}

			public GameObject()
			{ }

			public void ClearLists()
			{
				if (m_drawingList != null)
					m_drawingList.Remove(this);
				if (m_updateList != null)
					m_updateList.Remove(this);
			}

			internal void Initialise() { m_isInitialised = true; OnInitialise(); }
			protected virtual void OnInitialise() { }
			public abstract void Update();
			public abstract void Draw();
		}

}
