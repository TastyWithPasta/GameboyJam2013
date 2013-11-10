using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PastaGameLibrary;
using Microsoft.Xna.Framework;

namespace GbJamTotem
{

		public class Camera2D
		{
			float m_zoom = 1;
			Transform m_transform;
			Matrix m_centerMatrix, m_cameraMatrix;
			bool m_scaleToZoom = false;
			PTimer m_cameraShakeTimer;
			float m_shakeBreadth;
			Vector2 m_shakeOffset;
			bool m_shakeFlag;

			public Transform Transform
			{
				get { return m_transform; }
			}

			public bool ScaleToZoom
			{
				get { return m_scaleToZoom; }
				set { m_scaleToZoom = value; }
			}
			public float Zoom
			{
				get
				{
					if (m_scaleToZoom)
						return m_transform.SclX;
					return m_zoom;
				}
				set
				{
					if (m_scaleToZoom)
						m_transform.ScaleUniform = value;
					m_zoom = value;
				}
			}
			public Matrix CameraMatrix
			{
				get
				{
					return m_cameraMatrix * m_centerMatrix;
				}
			}

			public void Shake(float breadth, float time)
			{
				m_shakeBreadth = breadth;
				m_cameraShakeTimer.Interval = time;
				m_cameraShakeTimer.Stop();
				m_cameraShakeTimer.Start();
				m_shakeFlag = true;
			}

			private void StopShake()
			{
				m_cameraShakeTimer.Stop();
				m_shakeOffset = Vector2.Zero;
				m_shakeFlag = false;
			}
			

			private void UpdateCameraMatrix()
			{
				Matrix rotM, scaleM, posM, temp;

				if (m_shakeFlag)
				{
					m_shakeOffset.X = (float)(Program.Random.NextDouble() * 2 * m_shakeBreadth) -m_shakeBreadth;
					m_shakeOffset.Y = (float)(Program.Random.NextDouble() * 2 * m_shakeBreadth) - m_shakeBreadth;
				}

				Vector2 posGlobal = m_transform.PositionGlobal;
				Matrix.CreateRotationZ((float)m_transform.Direction, out rotM);
				if (m_scaleToZoom)
				{
					m_zoom = m_transform.ScaleGlobal.X;

					////Negates parent base zoom (1) so that it doesn't set the base zoom to two (parent(1) + child(1) = 2)
					//if (m_transform.ParentTransform != null)
					//    m_zoom--;
				}
				Matrix.CreateScale(m_zoom, m_zoom, 1, out scaleM);
				Matrix.CreateTranslation(-posGlobal.X + m_shakeOffset.X, -posGlobal.Y + m_shakeOffset.Y, 0, out posM);

				//Different order than transform matrix!
				Matrix.Multiply(ref posM, ref scaleM, out temp);
				Matrix.Multiply(ref temp, ref rotM, out m_cameraMatrix);
			}

			public Camera2D(Vector2 focusPoint)
			{
				m_transform = new Transform();
				m_centerMatrix = Matrix.CreateTranslation(focusPoint.X, focusPoint.Y, 0);
				m_cameraShakeTimer = new PTimer(Program.TheGame.TimerManager, StopShake);
			}

			public void Update()
			{
				//m_transform.Position += (_panTarget - m_transform.Position) * _panSharpness;
				//m_transform.ScaleUniform = m_transform.SclX + (_zoomTarget - m_transform.SclX) * _zoomSharpness;
				UpdateCameraMatrix();
			}
		}
	
}
