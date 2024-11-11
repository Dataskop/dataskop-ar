using System;
using System.Collections.Generic;
using Mapbox.VectorTile.Geometry;
using UnityEngine;

namespace Assets.Mapbox.Unity.MeshGeneration.Modifiers.MeshModifiers {

	public static class EarcutLibrary {

		public static List<int> Earcut(List<float> data, List<int> holeIndices, int dim) {
			dim = Math.Max(dim, 2);

			int hasHoles = holeIndices.Count;
			int outerLen = hasHoles > 0 ? holeIndices[0] * dim : data.Count;
			Node outerNode = linkedList(data, 0, outerLen, dim, true);
			List<int> triangles = new((int)(outerNode.i * 1.5));

			if (outerNode == null) {
				return triangles;
			}

			float minX = 0f;
			float minY = 0f;
			float maxX = 0f;
			float maxY = 0f;
			float x = 0f;
			float y = 0f;
			float size = 0f;

			if (hasHoles > 0) {
				outerNode = EliminateHoles(data, holeIndices, outerNode, dim);
			}

			// if the shape is not too simple, we'll use z-order curve hash later; calculate polygon bbox
			if (data.Count > 80 * dim) {
				minX = maxX = data[0];
				minY = maxY = data[1];

				for (int i = dim; i < outerLen; i += dim) {
					x = data[i];
					y = data[i + 1];

					if (x < minX) {
						minX = x;
					}

					if (y < minY) {
						minY = y;
					}

					if (x > maxX) {
						maxX = x;
					}

					if (y > maxY) {
						maxY = y;
					}
				}

				// minX, minY and size are later used to transform coords into integers for z-order calculation
				size = Math.Max(maxX - minX, maxY - minY);
			}

			earcutLinked(outerNode, triangles, dim, minX, minY, size);

			return triangles;
		}

		private static void earcutLinked(Node ear, List<int> triangles, int dim, float minX, float minY, float size,
			int pass = 0) {
			if (ear == null) {
				return;
			}

			// interlink polygon nodes in z-order
			if (pass == 0 && size > 0) {
				indexCurve(ear, minX, minY, size);
			}

			Node stop = ear;
			Node prev;
			Node next;

			// iterate through ears, slicing them one by one
			while (ear.prev != ear.next) {
				prev = ear.prev;
				next = ear.next;

				if (size > 0 ? isEarHashed(ear, minX, minY, size) : isEar(ear)) {
					// cut off the triangle
					triangles.Add(prev.i / dim);
					triangles.Add(next.i / dim);
					triangles.Add(ear.i / dim);

					removeNode(ear);

					// skipping the next vertice leads to less sliver triangles
					ear = next.next;
					stop = next.next;

					continue;
				}

				ear = next;

				// if we looped through the whole remaining polygon and can't find any more ears
				if (ear == stop) {
					// try filtering points and slicing again
					if (pass == 0) {
						earcutLinked(FilterPoints(ear, null), triangles, dim, minX, minY, size, 1);

						// if this didn't work, try curing all small self-intersections locally
					}
					else if (pass == 1) {
						ear = cureLocalIntersections(ear, triangles, dim);
						earcutLinked(ear, triangles, dim, minX, minY, size, 2);

						// as a last resort, try splitting the remaining polygon into two
					}
					else if (pass == 2) {
						splitEarcut(ear, triangles, dim, minX, minY, size);
					}

					break;
				}
			}
		}


		private static bool isEarHashed(Node ear, float minX, float minY, float size) {
			Node a = ear.prev;
			Node b = ear;
			Node c = ear.next;

			if (area(a, b, c) >= 0) {
				return false; // reflex, can't be an ear
			}

			// triangle bbox; min & max are calculated like this for speed
			float minTX = a.x < b.x ? a.x < c.x ? a.x : c.x : b.x < c.x ? b.x : c.x;
			float minTY = a.y < b.y ? a.y < c.y ? a.y : c.y : b.y < c.y ? b.y : c.y;
			float maxTX = a.x > b.x ? a.x > c.x ? a.x : c.x : b.x > c.x ? b.x : c.x;
			float maxTY = a.y > b.y ? a.y > c.y ? a.y : c.y : b.y > c.y ? b.y : c.y;

			// z-order range for the current triangle bbox;
			int minZ = zOrder(minTX, minTY, minX, minY, size);
			int maxZ = zOrder(maxTX, maxTY, minX, minY, size);

			// first look for points inside the triangle in increasing z-order
			Node p = ear.nextZ;

			while (p != null && p.mZOrder <= maxZ) {
				if (p != ear.prev && p != ear.next &&
				    pointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
				    area(p.prev, p, p.next) >= 0) {
					return false;
				}

				p = p.nextZ;
			}

			// then look for points in decreasing z-order
			p = ear.prevZ;

			while (p != null && p.mZOrder >= minZ) {
				if (p != ear.prev && p != ear.next &&
				    pointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
				    area(p.prev, p, p.next) >= 0) {
					return false;
				}

				p = p.prevZ;
			}

			return true;
		}

		private static int zOrder(float x, float y, float minX, float minY, float size) {
			//TODO casting here might be wrong
			x = 32767 * (x - minX) / size;
			y = 32767 * (y - minY) / size;

			x = ((int)x | (int)x << 8) & 0x00FF00FF;
			x = ((int)x | (int)x << 4) & 0x0F0F0F0F;
			x = ((int)x | (int)x << 2) & 0x33333333;
			x = ((int)x | (int)x << 1) & 0x55555555;

			y = ((int)y | (int)y << 8) & 0x00FF00FF;
			y = ((int)y | (int)y << 4) & 0x0F0F0F0F;
			y = ((int)y | (int)y << 2) & 0x33333333;
			y = ((int)y | (int)y << 1) & 0x55555555;

			return (int)x | (int)y << 1;
		}

		private static void splitEarcut(Node start, List<int> triangles, int dim, float minX, float minY, float size) {
			Node a = start;

			do {
				Node b = a.next.next;

				while (b != a.prev) {
					if (a.i != b.i && isValidDiagonal(a, b)) {
						// split the polygon in two by the diagonal
						Node c = SplitPolygon(a, b);

						// filter colinear points around the cuts
						a = FilterPoints(a, a.next);
						c = FilterPoints(c, c.next);

						// run earcut on each half
						earcutLinked(a, triangles, dim, minX, minY, size);
						earcutLinked(c, triangles, dim, minX, minY, size);
						return;
					}

					b = b.next;
				}

				a = a.next;
			} while (a != start);
		}

		private static bool isValidDiagonal(Node a, Node b) {
			return a.next.i != b.i && a.prev.i != b.i && !intersectsPolygon(a, b) &&
			       locallyInside(a, b) && locallyInside(b, a) && middleInside(a, b);
		}

		private static bool middleInside(Node a, Node b) {
			Node p = a;
			bool inside = false;
			float px = (a.x + b.x) / 2;
			float py = (a.y + b.y) / 2;

			do {
				if (p.y > py != p.next.y > py && p.next.y != p.y &&
				    px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x) {
					inside = !inside;
				}

				p = p.next;
			} while (p != a);

			return inside;
		}

		private static bool intersectsPolygon(Node a, Node b) {
			Node p = a;

			do {
				if (p.i != a.i && p.next.i != a.i && p.i != b.i && p.next.i != b.i &&
				    intersects(p, p.next, a, b)) {
					return true;
				}

				p = p.next;
			} while (p != a);

			return false;
		}

		private static Node cureLocalIntersections(Node start, List<int> triangles, int dim) {
			Node p = start;

			do {
				Node a = p.prev;
				Node b = p.next.next;

				if (!equals(a, b) && intersects(a, p, p.next, b) && locallyInside(a, b) && locallyInside(b, a)) {

					triangles.Add(a.i / dim);
					triangles.Add(p.i / dim);
					triangles.Add(b.i / dim);

					// remove two nodes involved
					removeNode(p);
					removeNode(p.next);

					p = start = b;
				}

				p = p.next;
			} while (p != start);

			return p;
		}

		private static bool intersects(Node p1, Node q1, Node p2, Node q2) {
			if (equals(p1, q1) && equals(p2, q2) ||
			    equals(p1, q2) && equals(p2, q1)) {
				return true;
			}

			return area(p1, q1, p2) > 0 != area(p1, q1, q2) > 0 &&
			       area(p2, q2, p1) > 0 != area(p2, q2, q1) > 0;
		}

		private static bool isEar(Node ear) {
			Node a = ear.prev;
			Node b = ear;
			Node c = ear.next;

			if (area(a, b, c) >= 0) {
				return false; // reflex, can't be an ear
			}

			// now make sure we don't have other points inside the potential ear
			Node p = ear.next.next;

			while (p != ear.prev) {
				if (pointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
				    area(p.prev, p, p.next) >= 0) {
					return false;
				}

				p = p.next;
			}

			return true;
		}

		private static void indexCurve(Node start, float minX, float minY, float size) {
			Node p = start;

			do {
				if (p.mZOrder == 0) {
					p.mZOrder = zOrder(p.x, p.y, minX, minY, size);
				}

				p.prevZ = p.prev;
				p.nextZ = p.next;
				p = p.next;
			} while (p != start);

			p.prevZ.nextZ = null;
			p.prevZ = null;

			sortLinked(p);
		}

		private static Node sortLinked(Node list) {
			int i = 0;
			Node p;
			Node q;
			Node e;
			Node tail;
			int numMerges = 0;
			;
			int pSize = 0;
			int qSize = 0;
			int inSize = 1;

			do {
				p = list;
				list = null;
				tail = null;
				numMerges = 0;

				while (p != null) {
					numMerges++;
					q = p;
					pSize = 0;

					for (i = 0; i < inSize; i++) {
						pSize++;
						q = q.nextZ;

						if (q == null) {
							break;
						}
					}

					qSize = inSize;

					while (pSize > 0 || qSize > 0 && q != null) {

						if (pSize != 0 && (qSize == 0 || q == null || p.mZOrder <= q.mZOrder)) {
							e = p;
							p = p.nextZ;
							pSize--;
						}
						else {
							e = q;
							q = q.nextZ;
							qSize--;
						}

						if (tail != null) {
							tail.nextZ = e;
						}
						else {
							list = e;
						}

						e.prevZ = tail;
						tail = e;
					}

					p = q;
				}

				tail.nextZ = null;
				inSize *= 2;

			} while (numMerges > 1);

			return list;
		}

		private static Node EliminateHoles(List<float> data, List<int> holeIndices, Node outerNode, int dim) {
			int i = 0;
			int len = holeIndices.Count;
			int start = 0;
			int end = 0;
			Node list = null;
			List<Node> queue = new(len);

			for (i = 0; i < len; i++) {
				start = holeIndices[i] * dim;
				end = i < len - 1 ? holeIndices[i + 1] * dim : data.Count;
				list = linkedList(data, start, end, dim, false);

				if (list == list.next) {
					list.steiner = true;
				}

				queue.Add(getLeftmost(list));
			}

			queue.Sort(delegate(Node a, Node b) { return (int)Math.Ceiling(a.x - b.x); });

			// process holes from left to right
			for (i = 0; i < queue.Count; i++) {
				EliminateHole(queue[i], outerNode);
				outerNode = FilterPoints(outerNode, outerNode.next);
			}

			return outerNode;
		}

		private static void EliminateHole(Node hole, Node outerNode) {
			outerNode = FindHoleBridge(hole, outerNode);

			if (outerNode != null) {
				Node b = SplitPolygon(outerNode, hole);
				FilterPoints(b, b.next);
			}
		}

		private static Node FilterPoints(Node start, Node end) {
			if (start == null) {
				return start;
			}

			if (end == null) {
				end = start;
			}

			Node p = start;
			bool again = true;

			do {
				again = false;

				if (!p.steiner && (equals(p, p.next) || area(p.prev, p, p.next) == 0)) {
					removeNode(p);
					p = end = p.prev;

					if (p == p.next) {
						return null;
					}

					again = true;

				}
				else {
					p = p.next;
				}
			} while (again || p != end);

			return end;
		}

		private static Node SplitPolygon(Node a, Node b) {
			Node a2 = new(a.i, a.x, a.y);
			Node b2 = new(b.i, b.x, b.y);
			Node an = a.next;
			Node bp = b.prev;

			a.next = b;
			b.prev = a;

			a2.next = an;
			an.prev = a2;

			b2.next = a2;
			a2.prev = b2;

			bp.next = b2;
			b2.prev = bp;

			return b2;
		}

		private static Node FindHoleBridge(Node hole, Node outerNode) {
			Node p = outerNode;
			float hx = hole.x;
			float hy = hole.y;
			float qx = float.MinValue;
			Node m = null;

			// find a segment intersected by a ray from the hole's leftmost point to the left;
			// segment's endpoint with lesser x will be potential connection point
			do {
				if (hy <= p.y && hy >= p.next.y && p.next.y != p.y) {
					float x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);

					if (x <= hx && x > qx) {
						qx = x;

						if (x == hx) {
							if (hy == p.y) {
								return p;
							}

							if (hy == p.next.y) {
								return p.next;
							}
						}

						m = p.x < p.next.x ? p : p.next;
					}
				}

				p = p.next;
			} while (p != outerNode);

			if (m == null) {
				return null;
			}

			if (hx == qx) {
				return m.prev; // hole touches outer segment; pick lower endpoint
			}

			// look for points inside the triangle of hole point, segment intersection and endpoint;
			// if there are no points found, we have a valid connection;
			// otherwise choose the point of the minimum angle with the ray as connection point

			Node stop = m;
			float mx = m.x;
			float my = m.y;
			float tanMin = float.MaxValue;
			float tan = 0f;

			p = m.next;

			while (p != stop) {
				if (hx >= p.x && p.x >= mx && hx != p.x &&
				    pointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y)) {

					tan = Math.Abs(hy - p.y) / (hx - p.x); // tangential

					if ((tan < tanMin || tan == tanMin && p.x > m.x) && locallyInside(p, hole)) {
						m = p;
						tanMin = tan;
					}
				}

				p = p.next;
			}

			return m;
		}

		private static bool locallyInside(Node a, Node b) {
			return area(a.prev, a, a.next) < 0 ?
				area(a, b, a.next) >= 0 && area(a, a.prev, b) >= 0 :
				area(a, b, a.prev) < 0 || area(a, a.next, b) < 0;
		}

		private static float area(Node p, Node q, Node r) {
			return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
		}

		private static bool pointInTriangle(float ax, float ay, float bx, float by, float cx, float cy, float px,
			float py) {
			return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
			       (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
			       (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
		}

		private static Node getLeftmost(Node start) {
			Node p = start;
			Node leftmost = start;

			do {
				if (p.x < leftmost.x) {
					leftmost = p;
				}

				p = p.next;
			} while (p != start);

			return leftmost;
		}

		// create a circular doubly linked list from polygon points in the specified winding order
		private static Node linkedList(List<float> data, int start, int end, int dim, bool clockwise) {
			int i = 0;
			Node last = null;

			if (clockwise == signedArea(data, start, end, dim) > 0) {
				for (i = start; i < end; i += dim) {
					last = insertNode(i, data[i], data[i + 1], last);
				}
			}
			else {
				for (i = end - dim; i >= start; i -= dim) {
					last = insertNode(i, data[i], data[i + 1], last);
				}
			}

			if (last != null && equals(last, last.next)) {
				removeNode(last);
				last = last.next;
			}

			return last;
		}

		private static void removeNode(Node p) {
			p.next.prev = p.prev;
			p.prev.next = p.next;

			if (p.prevZ != null) {
				p.prevZ.nextZ = p.nextZ;
			}

			if (p.nextZ != null) {
				p.nextZ.prevZ = p.prevZ;
			}
		}

		private static bool equals(Node p1, Node p2) {
			return p1.x == p2.x && p1.y == p2.y;
		}

		private static float signedArea(List<float> data, int start, int end, int dim) {
			float sum = 0f;
			int j = end - dim;

			for (int i = start; i < end; i += dim) {
				sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
				j = i;
			}

			return sum;
		}

		private static Node insertNode(int i, float x, float y, Node last) {
			Node p = new(i, x, y);

			if (last == null) {
				p.prev = p;
				p.next = p;

			}
			else {
				p.next = last.next;
				p.prev = last;
				last.next.prev = p;
				last.next = p;
			}

			return p;
		}

		public static Data Flatten(List<List<Vector3>> data) {
			int dataCount = data.Count;
			int totalVertCount = 0;

			for (int i = 0; i < dataCount; i++) {
				totalVertCount += data[i].Count;
			}

			Data result = new() {
				Dim = 2
			};
			result.Vertices = new List<float>(totalVertCount * 2);
			int holeIndex = 0;

			for (int i = 0; i < dataCount; i++) {
				int subCount = data[i].Count;

				for (int j = 0; j < subCount; j++) {
					result.Vertices.Add(data[i][j][0]);
					result.Vertices.Add(data[i][j][2]);
				}

				if (i > 0) {
					holeIndex += data[i - 1].Count;
					result.Holes.Add(holeIndex);
				}
			}

			return result;
		}

	}

	public class Data {

		public List<float> Vertices;
		public List<int> Holes;
		public int Dim;

		public Data() {
			Holes = new List<int>();
			Dim = 2;
		}

	}

	public class Node {

		/* Member Variables. */
		public int i;
		public float x;
		public float y;
		public int mZOrder;
		public Node prev;
		public Node next;
		public Node prevZ;
		public Node nextZ;
		public bool steiner;

		public Node(int ind, float pX, float pY) {
			/* Initialize Member Variables. */
			i = ind;
			x = pX;
			y = pY;
			mZOrder = 0;
			prev = null;
			next = null;
			prevZ = null;
			nextZ = null;
		}

		protected void setPreviousNode(Node pNode) {
			prev = pNode;
		}

		protected Node getPreviousNode() {
			return prev;
		}

		protected void setNextNode(Node pNode) {
			next = pNode;
		}

		protected Node getNextNode() {
			return next;
		}

		protected void setZOrder(int pZOrder) {
			mZOrder = pZOrder;
		}

		protected int getZOrder() {
			return mZOrder;
		}

		protected void setPreviousZNode(Node pNode) {
			prevZ = pNode;
		}

		protected Node getPreviousZNode() {
			return prevZ;
		}

		protected void setNextZNode(Node pNode) {
			nextZ = pNode;
		}

		protected Node getNextZNode() {
			return nextZ;
		}

	}

}