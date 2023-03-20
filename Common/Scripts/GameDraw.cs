using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animations
{
	// Simple utility for drawing in-game
	public class GameDraw : MonoBehaviour
	{
		// PRIVATE MEMBERS

		private static List<Record> _records = new(256);
		private static Material _material;

		// PUBLIC METHODS

		public static void WireBox(Vector3 center, Quaternion rotation, Vector3 size, Color color, float duration = 0f)
		{
			var record = new Record()
			{
				Type = EType.WireBox,
				Matrix = Matrix4x4.TRS(center, rotation, size),
				Color = color,
				StartTime = Time.timeSinceLevelLoad,
				Duration = duration,
			};

			_records.Add(record);
		}

		public static void Clear()
		{
			_records.Clear();
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			if (GetComponent<Camera>() == false)
			{
				Debug.LogError("GameDraw script needs to be placed on object with Camera component");
			}
		}

		protected void OnPostRender()
		{
			if (_material == null)
			{
				var shader = Shader.Find("Hidden/Internal-Colored");

				_material = new Material(shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}

			float time = Time.timeSinceLevelLoad;

			for (int i = 0; i < _records.Count; i++)
			{
				var record = _records[i];

				DrawRecord(record);

				if (record.StartTime + record.Duration <= time)
				{
					_records.RemoveAt(i--);
				}
			}
		}

		protected void OnDestroy()
		{
			Clear();
		}

		// PRIVATE METHODS

		private void DrawRecord(Record record)
		{
			GL.PushMatrix();
	        _material.SetPass(0);
		    GL.MultMatrix(record.Matrix);

			if (record.Type == EType.WireBox)
			{
				GL.Begin(GL.LINES);
				GL.Color(record.Color);

				DrawWireBox(new Bounds(Vector3.zero, Vector3.one));

				GL.End();
			}

			GL.PopMatrix();
		}

		private void DrawWireBox(Bounds bounds)
		{
			var min = bounds.min;
			var max = bounds.max;

			DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z));
			DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z));
			DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z));
			DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, min.y, min.z));

			DrawLine(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z));
			DrawLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z));
			DrawLine(new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z));
			DrawLine(new Vector3(min.x, max.y, max.z), new Vector3(min.x, max.y, min.z));

			DrawLine(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z));
			DrawLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z));
			DrawLine(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z));
			DrawLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void DrawLine(Vector3 a, Vector3 b)
		{
			GL.Vertex(a);
			GL.Vertex(b);
		}

		// DATA STRUCTURES

		private struct Record
		{
			public EType Type;
			public Matrix4x4 Matrix;
			public Color Color;
			public float StartTime;
			public float Duration;
		}

		private enum EType
		{
			None,
			WireBox,
		}
	}
}
