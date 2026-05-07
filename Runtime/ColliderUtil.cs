using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TW.Common
{
	public static class ColliderUtil
	{
		public static bool IsLayerInMask(int layer, LayerMask layerMask)
		{
			return (layerMask.value & (1 << layer)) != 0;
		}

		public static Vector3 GetCorner(Collider collider, int index)
		{

			var center = collider.bounds.center - collider.transform.position;
			//var center = collider.transform.position - collider.bounds.center;
			var size = GetColliderSize(collider);

			return collider.transform.TransformPoint(GetOffset(size, index)) + center;
		}

		private static Vector3 GetColliderSize(Collider collider)
		{

			var size = collider.bounds.size;
			var box = collider as BoxCollider;
			if (box)
			{
				return box.size;
			}

			var sphere = collider as SphereCollider;
			if (sphere)
			{
				return new Vector3(sphere.radius * 2, sphere.radius * 2, sphere.radius * 2);
			}

			var cap = collider as CapsuleCollider;
			if (cap)
			{
				if (cap.direction == 0)
				{
					size.x = cap.height;
					size.y = cap.radius * 2;
					size.z = cap.radius * 2;
				}
				else if (cap.direction == 1)
				{
					size.y = cap.height;
					size.x = cap.radius * 2;
					size.z = cap.radius * 2;
				}
				else
				{
					size.z = cap.height;
					size.y = cap.radius * 2;
					size.x = cap.radius * 2;
				}
			}


			return size;
		}

		public static void GetFaceVertices(Collider collider, Vector3 normal, Vector3[] corners)
		{
			int count = 0;
			Vector3 dir;
			var length = corners.Length;

			for (int i = 0; i < 8; i++)
			{
				dir = GetCornerDirection(collider, i);
				if (Vector3.Dot(normal, dir) >= 0)
				{
					if (count < corners.Length)
					{
						corners[count] = GetCorner(collider, i);
						count++;
					}
				}
			}
		}

		public static void GetTopEdges(Collider collider, Vector3 normal, out Vector3 topLeft, out Vector3 topRight)
		{
			Vector3 dir;
			topRight = Vector3.zero;
			topLeft = Vector3.zero;

			Vector3 leftDir = Quaternion.Euler(0f, -90f, 0) * normal;
			for (int i = 0; i < 8; i++)
			{
				dir = GetCornerDirection(collider, i);
				if (Vector3.Dot(normal, dir) >= 0)
				{
					if (Vector3.Dot(Vector3.up, dir) >= 0)
					{
						if (Vector3.Dot(leftDir, dir) >= 0)
						{
							topLeft = GetCorner(collider, i);
						}
						else
						{
							topRight = GetCorner(collider, i);
						}
					}
				}
			}
		}

		public static void GetCenterFaces(Collider collider, Vector3 normal, out Vector3 topLeft, out Vector3 topRight)
		{
			Vector3 dir;
			topRight = Vector3.zero;
			topLeft = Vector3.zero;
			float lDot = -1f;
			float rDot = -1f;

			Vector3 leftDir = Quaternion.Euler(0f, -90f, 0) * normal;
			for (int i = 0; i < 6; i++)
			{
				dir = GetAxisByIndex(collider.transform, i);

				var l = Vector3.Dot(leftDir, dir);
				if (l > lDot)
				{
					lDot = l;
					topLeft = GetFace(collider, i);
				}

				var r = Vector3.Dot(-leftDir, dir);
				if (r > rDot)
				{
					rDot = r;
					topRight = GetFace(collider, i);
				}

			}
		}

		public static Vector3 GetHorizontalFaces(Collider collider, Vector3 point, float width, out Vector3 topLeft, out Vector3 topRight)
		{
			Vector3 dir;
			topRight = Vector3.zero;
			topLeft = Vector3.zero;
			float lDot = -1f;

			var normal = GetClosestNormalByHorizontalPoint(collider, point, out Vector3 heading);
			var last = normal;
			Vector3 leftDir = Quaternion.Euler(0f, -90f, 0) * normal;
			int i = 0;
			while (i < 6)
			{
				var face = GetFace(collider, i);
				dir = Vector3.Normalize(face - collider.bounds.center);

				var l = Vector3.Dot(leftDir, dir);
				++i;
				if (l >= lDot)
				{
					var otherFace = GetFace(collider, i);
					if (Vector3.Distance(face, otherFace) < width)
					{
						i++;
						continue;
					}
					topLeft = face;
					topRight = otherFace;
					lDot = l;
					last = Quaternion.Euler(0f, -90f, 0) * dir;
					if (Vector3.Dot(last, heading) < 0)
					{

						last = Quaternion.Euler(0f, 90f, 0) * dir;
					}
				}
				i++;
			}
			Debug.DrawRay(collider.bounds.center, last, Color.red);
			return last;
		}

		public static void GetTopCorners(Collider collider, Vector3 normal, Vector3[] vectors)
		{
			if (vectors == null || vectors.Length < 4)
				return;


			Vector3 dir;
			Vector3 leftDir = Quaternion.Euler(0f, -90f, 0) * normal;
			for (int i = 0; i < 8; i++)
			{
				dir = GetCornerDirection(collider, i);
				if (Vector3.Dot(Vector3.up, dir) < 0)
					continue;

				if (Vector3.Dot(normal, dir) >= 0)
				{
					if (Vector3.Dot(leftDir, dir) >= 0)
					{
						vectors[0] = GetCorner(collider, i);
					}
					else
					{
						vectors[1] = GetCorner(collider, i);
					}
				}
				else
				{
					if (Vector3.Dot(leftDir, dir) >= 0)
					{
						vectors[2] = GetCorner(collider, i);
					}
					else
					{
						vectors[3] = GetCorner(collider, i);
					}
				}
			}
		}


		public static void GetTops(Collider collider, Vector3 direction, out Vector3 topLeft, out Vector3 topRight)
		{
			topRight = Vector3.zero;
			topLeft = Vector3.zero;
			using (var pooledObject = ListPool<Vector3>.Get(out List<Vector3> tempList))
			{
				Vector3 dir;
				for (int i = 0; i < 8; i++)
				{
					dir = GetCornerDirection(collider, i);
					if (Vector3.Dot(direction, dir) >= 0)
					{
						tempList.Add(GetCorner(collider, i));
					}
				}
				Vector3 center = Vector3.zero;
				foreach (var item in tempList)
				{
					center += item;
				}
				center /= tempList.Count;
				//Vector3 topDir = Quaternion.Euler(0, 0, -90) * direction;
				Vector3 leftDir = Quaternion.Euler(0f, -90f, 0) * direction;
				//Debug.DrawRay(center, topDir, Color.red);
				Debug.DrawRay(center, leftDir, Color.cyan);
				foreach (var item in tempList)
				{
					var heading = item - center;
					Debug.DrawRay(center, heading, Color.yellow);

					if (Vector3.Dot(Vector3.up, heading.normalized) >= 0)
					{
						if (Vector3.Dot(leftDir, heading.normalized) >= 0)
						{
							topLeft = item;
						}
						else
						{
							topRight = item;
						}

					}
				}
			}
		}

		public static Vector3 GetClosestFace(Collider collider, Vector3 normal)
		{
			float value = -1;

			Vector3 dir;
			Vector3 face = Vector3.zero;
			for (int i = 0; i < 8; i++)
			{
				dir = GetCornerDirection(collider, i);
				var dot = Vector3.Dot(normal, dir);
				if (dot > value)
				{
					value = dot;
					face = dir;
				}
			}
			return face;
		}

		public static Vector3 GetAxisByIndex(Transform other, int index)
		{
			index = Mathf.Clamp(index, 0, 5);
			switch (index)
			{
				case 0:
					return other.right;
				case 1:
					return -other.right;
				case 2:
					return other.forward;
				case 3:
					return -other.forward;
				case 4:
					return other.up;
				case 5:
					return -other.up;
				default:
					return Vector3.zero;
			}
		}

		public static Vector2 GetFaceSize(Collider collider, Vector3 normal)
		{
			Vector3 highest = GetCorner(collider, 0);
			Vector3 highest2 = highest;
			Vector3 leftest = highest;
			Vector3 leftest2 = highest2;

			int count = 0;
			Vector3 dir;
			for (int i = 0; i < 8; i++)
			{
				dir = GetCornerDirection(collider, i);
				if (Vector3.Dot(normal, dir) >= 0)
				{
					var corner = GetCorner(collider, i);
					if (count == 0)
					{
						highest = leftest = corner;
					}
					else
					{
						if (count == 1)
						{
							highest2 = leftest2 = corner;
						}
						if (corner.y > highest.y)
						{
							highest2 = highest;
							highest = corner;
						}
						else if (corner != highest && corner.y > highest2.y)
						{
							highest2 = corner;
						}

						if (corner.x < leftest.x)
						{
							leftest2 = leftest;
							leftest = corner;
						}
						else if (corner != leftest && corner.x < leftest2.x)
						{
							leftest2 = corner;
						}
					}
					count++;
				}
			}
			return new Vector2(Vector3.Distance(highest, highest2), Vector3.Distance(leftest, leftest2));
		}

		public static Vector2 GetTriangleSize(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			var farthestY = GetFarthestY(p0, p1, p2);
			var width = DistanceByFarthest(farthestY, p0, p1, p2);

			var farthestX = GetFarthestX(p0, p1, p2);
			var height = DistanceByFarthest(farthestX, p0, p1, p2);

			return new Vector3(width, height);
		}

		private static Vector3 GetFarthestY(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			var average = (p0.y + p1.y + p2.y) / 3;
			var farthest = p0;
			if (Mathf.Abs(farthest.y - average) < Mathf.Abs(p1.y - average))
			{
				farthest = p1;
			}
			if (Mathf.Abs(farthest.y - average) < Mathf.Abs(p2.y - average))
			{
				farthest = p2;
			}
			return farthest;
		}

		private static Vector3 GetFarthestX(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			var average = (p0.x + p1.x + p2.x) / 3;
			var farthest = p0;
			if (Mathf.Abs(farthest.x - average) < Mathf.Abs(p1.x - average))
			{
				farthest = p1;
			}
			if (Mathf.Abs(farthest.x - average) < Mathf.Abs(p2.x - average))
			{
				farthest = p2;
			}
			return farthest;
		}

		private static float DistanceByFarthest(Vector3 farthest, Vector3 p0, Vector3 p1, Vector3 p2)
		{
			Vector3 pt0, pt1;
			if (farthest != p0)
			{
				pt0 = p0;
			}
			else if (farthest != p1)
			{
				pt0 = p1;
			}
			else
			{
				pt0 = p2;
			}

			if (farthest != p0 && pt0 != p0)
			{
				pt1 = p0;
			}
			else if (farthest != p1 && pt0 != p1)
			{
				pt1 = p1;
			}
			else
			{
				pt1 = p2;
			}

			return Vector3.Distance(pt0, pt1);
		}

		public static Vector3 GetLocalCorner(Collider collider, int index)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			return GetLocalCorner(center, size, index);
		}

		public static Vector3 GetCornerDirection(Collider collider, int index)
		{
			var corner = GetCorner(collider, index);

			return Vector3.Normalize(corner - collider.bounds.center);
		}

		public static Vector3 GetLocalCornerDirection(Collider collider, int index)
		{
			var center = collider.bounds.center - collider.transform.position;

			var size = GetColliderSize(collider);
			var offset = GetOffset(size, index);
			var heading = offset - center;
			return heading.normalized;
		}

		public static void GetLocalCorners(Vector3 center, Vector3 size, ref List<Vector3> corners)
		{
			corners.Clear();
			for (int i = 0; i < 8; i++)
			{
				corners.Add(GetLocalCorner(center, size, i));
			}
		}

		private static Vector3 GetLocalCorner(Vector3 center, Vector3 size, int index)
		{

			return center + GetOffset(size, index);
		}

		public static Vector3 GetFace(Collider collider, int index)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			return collider.transform.TransformPoint(GetLocalFace(size, index)) + center;
		}

		public static Vector3 GetLocalFace(Collider collider, int index)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			return GetLocalFace(size, index) + center;
		}

		private static Vector3 GetLocalFace(Vector3 size, int index)
		{

			return GetFaceOffset(size, index);
		}

		public static Vector3 GetTopLeft(Collider collider, Vector3 direction)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			Vector3 point = Vector3.zero;
			point.z = size.z * direction.z;

			var dirLeft = Quaternion.Euler(0, -90, 0) * direction;
			point.x = size.x * dirLeft.z;
			var dirUp = Quaternion.Euler(-90, 0, 0) * direction;
			point.y = size.y * dirUp.z;

			return collider.transform.TransformPoint(point);
		}

		public static Vector3 GetTopRight(Collider collider, Vector3 direction)
		{
			//var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			Vector3 point = Vector3.zero;
			point.z = size.z * direction.z;

			var dirLeft = Quaternion.Euler(0, 90, 0) * direction;
			point.x = size.x * dirLeft.z;
			var dirUp = Quaternion.Euler(-90, 0, 0) * direction;
			point.y = size.y * dirUp.z;

			//var dirOff = Quaternion.Euler(-90, 90, 0) * direction;
			//point = Vector3.Scale(size, dirOff);

			return collider.transform.TransformPoint(point);
		}

		private static Vector3 GetOffset(Vector3 size, int index)
		{
			index = Mathf.Clamp(index, 0, 7);

			Vector3 offset = Vector3.zero;
			switch (index)
			{
				case 0:
					offset = new Vector3(-size.x, -size.y, -size.z);
					break;
				case 1:
					offset = new Vector3(size.x, -size.y, size.z);
					break;
				case 2:
					offset = new Vector3(size.x, -size.y, -size.z);
					break;
				case 3:
					offset = new Vector3(-size.x, -size.y, size.z);
					break;
				case 4:
					offset = new Vector3(size.x, size.y, -size.z);
					break;
				case 5:
					offset = new Vector3(size.x, size.y, size.z);
					break;
				case 6:
					offset = new Vector3(-size.x, size.y, -size.z);
					break;
				case 7:
					offset = new Vector3(-size.x, size.y, size.z);
					break;
			}
			return offset * 0.5f;
		}

		private static Vector3 GetFaceOffset(Vector3 size, int index)
		{
			index = Mathf.Clamp(index, 0, 5);

			Vector3 offset = Vector3.zero;
			switch (index)
			{
				case 0:
					offset = new Vector3(size.x, 0, 0);
					break;
				case 1:
					offset = new Vector3(-size.x, 0, 0);
					break;
				case 2:
					offset = new Vector3(0, 0, size.z);
					break;
				case 3:
					offset = new Vector3(0, 0, -size.z);
					break;
				case 4:
					offset = new Vector3(0, size.y, 0);
					break;
				case 5:
					offset = new Vector3(0, -size.y, 0);
					break;
			}
			return offset * 0.5f;
		}

		public static Vector3 GetQuadrent(Collider collider, int index, float ratio = 0.5f)
		{
			var center = collider.bounds.center - collider.transform.position;
			UnityEngine.Profiling.Profiler.BeginSample("GetColliderSize");
			var size = GetColliderSize(collider);
			UnityEngine.Profiling.Profiler.EndSample();
			var corner = collider.transform.TransformPoint(GetLocalCorner(center, size, index));
			var heading = (collider.bounds.center - corner) * ratio;
			return collider.bounds.center + heading;
		}

		public static void GetAllQuadrents(Collider collider, ref List<Vector3> quadrents, float ratio = 0.5f)
		{
			quadrents.Clear();
			for (int i = 0; i < 8; i++)
			{
				quadrents.Add(GetQuadrent(collider, i, ratio));
			}
		}

		public static void GetTip(Collider collider, out Vector3 tip)
		{

			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			Vector3 top;
			Vector3 secondTop = top = collider.transform.TransformPoint(GetLocalCorner(center, size, 0));

			Vector3 point;
			for (int i = 1; i < 8; i++)
			{
				point = collider.transform.TransformPoint(GetLocalCorner(center, size, i));
				if (point.y > top.y)
				{
					secondTop = top;
					top = point;
				}
				//if (point.y > topLeft.y && point.x < topLeft.x)
				//{
				//	topLeft = point;
				//}
			}
			float y;
			if (secondTop.y > top.y)
			{
				y = (secondTop.y - top.y) * 0.5f;
				y += top.y;

			}
			else
			{
				y = (top.y - secondTop.y) * 0.5f;
				y += secondTop.y;
			}

			//Debug.Log(y);
			tip = new Vector3(collider.bounds.center.x, top.y, collider.bounds.center.z);

		}

		public static Vector3 GetTip(Vector3 p0, Vector3 p1, Vector3 p2)
		{
			Vector3 tip = (p0 + p1 + p2) / 3;
			if (p0.y > tip.y)
			{
				tip.y = p0.y;
			}
			if (p1.y > tip.y)
			{
				tip.y = p1.y;
			}
			if (p2.y > tip.y)
			{
				tip.y = p2.y;
			}
			return tip;
		}

		public static void GetToe(Collider collider, out Vector3 tip)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);
			Vector3 top;
			Vector3 secondTop = top = collider.transform.TransformPoint(GetLocalCorner(center, size, 0));

			Vector3 point;
			for (int i = 1; i < 8; i++)
			{
				point = collider.transform.TransformPoint(GetLocalCorner(center, size, i));
				if (point.y < top.y)
				{
					secondTop = top;
					top = point;
				}
				//if (point.y > topLeft.y && point.x < topLeft.x)
				//{
				//	topLeft = point;
				//}
			}
			float y;
			if (secondTop.y < top.y)
			{
				y = (secondTop.y - top.y) * 0.5f;
				y += top.y;
			}
			else
			{
				y = (top.y - secondTop.y) * 0.5f;
				y += secondTop.y;
			}

			//Debug.Log(y);
			tip = new Vector3(collider.bounds.center.x, top.y, collider.bounds.center.z);

		}

		public static void GetMinMax(Collider collider, out Vector3 min, out Vector3 max)
		{
			var center = collider.bounds.center - collider.transform.position;
			var size = GetColliderSize(collider);

			min = max = collider.transform.TransformPoint(GetLocalCorner(center, size, 0));

			Vector3 point;
			for (int i = 1; i < 8; i++)
			{
				point = collider.transform.TransformPoint(GetLocalCorner(center, size, i));
				if (point.x > max.x)
				{
					max.x = point.x;
				}
				if (point.y > max.y)
				{
					max.y = point.y;
				}
				if (point.z > max.z)
				{
					max.z = point.z;
				}

				if (point.x < min.x)
				{
					min.x = point.x;
				}
				if (point.y < min.y)
				{
					min.y = point.y;
				}
				if (point.z < min.z)
				{
					min.z = point.z;
				}
			}
		}

		public static Vector3 GetClosestNormal(Collider other, Vector3 normal, float width)
		{
			float value = -1;
			Vector3 face = Vector3.zero;
			Vector3 dir;
			for (int i = 0; i < 6; i++)
			{
				dir = ColliderUtil.GetAxisByIndex(other.transform, i);
				float dot = Vector3.Dot(dir, normal);
				if (dot > value)
				{
					var size = ColliderUtil.GetFaceSize(other, dir);
					if (size.x >= width)
					{
						value = dot;
						face = dir;
					}
				}
			}
			return face;
		}

		public static Vector3 GetClosestNormal(Collider other, Vector3 normal)
		{
			float value = -1;
			Vector3 face = Vector3.zero;
			Vector3 dir;
			for (int i = 0; i < 6; i++)
			{
				dir = ColliderUtil.GetAxisByIndex(other.transform, i);
				float dot = Vector3.Dot(dir, normal);
				if (dot > value)
				{

					value = dot;
					face = dir;
				}
			}
			return face;
		}

		public static Vector3 GetClosestNormalByHorizontalPoint(Collider other, Vector3 point, out Vector3 heading)
		{
			var pos = point;
			pos.y = other.bounds.center.y;
			heading = Vector3.Normalize(pos - other.bounds.center);
			float value = -1;
			Vector3 face = Vector3.zero;
			Vector3 dir;
			for (int i = 0; i < 6; i++)
			{
				dir = ColliderUtil.GetAxisByIndex(other.transform, i);
				float dot = Vector3.Dot(dir, heading);
				if (dot > value)
				{

					value = dot;
					face = dir;
				}
			}
			return face;
		}

		public static Vector3 GetLowestPoint(Collider collider)
		{
			var center = collider.bounds.center - collider.transform.position;
			Vector3 other;
			var lowest = collider.transform.TransformPoint(center + new Vector3(-collider.bounds.size.x, -collider.bounds.size.y, -collider.bounds.size.z) * 0.5f);

			other = collider.transform.TransformPoint(center + new Vector3(collider.bounds.size.x, -collider.bounds.size.y, -collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(collider.bounds.size.x, -collider.bounds.size.y, collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(-collider.bounds.size.x, -collider.bounds.size.y, collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(-collider.bounds.size.x, collider.bounds.size.y, -collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(collider.bounds.size.x, collider.bounds.size.y, -collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(collider.bounds.size.x, collider.bounds.size.y, collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}
			other = collider.transform.TransformPoint(center + new Vector3(-collider.bounds.size.x, collider.bounds.size.y, collider.bounds.size.z) * 0.5f);
			if (other.y < lowest.y)
			{
				lowest = other;
			}

			return lowest;
		}

		//public static void GetClosestFaceWidthHeight(Collider collider, Vector3 diection, out float width, out float height)
		//{
		//	collider.transform.TransformDirection(diection);
		//}

		public static void GetCorners(Collider collider, out Vector3[] corners)
		{
			corners = new Vector3[8];
			for (int i = 0; i < corners.Length; i++)
			{
				corners[i] = GetCorner(collider, i);
			}
		}

	}

}
