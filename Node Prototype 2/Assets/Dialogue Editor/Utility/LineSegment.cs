using System;
using UnityEngine;

namespace WolfEditor.Utility
{
    public enum LineIntersectionResult { False, True, Collinear }

    public struct LineSegment
    {
        public Vector2 start;
        public Vector2 end;

        public LineSegment(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
        }

        public LineSegment(float x1, float y1, float x2, float y2)
        {
            start = new Vector2(x1, y1);
            end = new Vector2(x2, y2);
        }

        public LineSegment Flipped { get { return new LineSegment(end, start); } }

        public void Flip()
        {
            Vector2 temp = start;
            start = end;
            end = temp;
        }

        public static bool WideLineSegmentPointCheck(Vector2 point, LineSegment lineSegment, float width)
        {
            return WideLineSegmentPointCheck(point, lineSegment.start, lineSegment.end, width);
        }

        public static bool WideLineSegmentPointCheck(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float width)
        {
            Vector2 p = point; Vector2 a = lineStart; Vector2 b = lineEnd;
            return LineCheck(p, a, b, width);// && LineCheck(p, b, a, width);
        }

        private static bool LineCheck(Vector2 point, Vector2 lineStart, Vector2 lineEnd, float width)
        {
            width /= 2;
            float sqrWidth = width * width;
            Vector2 p = point;
            Vector2 a = lineStart;
            Vector2 b = lineEnd;

            Vector2 ap = p - a;
            Vector2 ab = b - a;

            //Less Expensive
            float dot = Vector2.Dot(ap, ab); //not a regular dot
            if (dot < 0 && ap.sqrMagnitude > sqrWidth) return false;

            Vector2 bp = p - b;
            float dot2 = Vector2.Dot(bp, -ab);
            if (dot2 < 0 && ap.sqrMagnitude > sqrWidth) return false;

            float normalizedDot = dot / ab.sqrMagnitude; //between 0 and 1
            Vector2 closestPoint = a + ab * normalizedDot;

            ////More expensive
            //Vector2 normalizedAB = ab.normalized;
            //float dot = Vector2.Dot(ap, normalizedAB);
            //Vector2 closestPoint = a + dot * normalizedAB;

            return (closestPoint - p).sqrMagnitude <= sqrWidth;
        }

        public static float LineRectOverlapPercentageCheck(Rect rect, Vector2 lineStart, Vector2 lineEnd)
        {
            Rect r = rect;
            Vector2 a = lineStart;
            Vector2 b = lineEnd;
            float abLength = (b - a).magnitude;

            LineSegment ab = new LineSegment(a, b);

            bool overlapA = r.Contains(a);
            bool overlapB = r.Contains(b);

            if (overlapA && overlapB) return 1;
            if (overlapA)
            {
                Vector2[] intersection;
                LineSegment[] edges = rect.GetEdges();
                if (HandleIntersectionTest(ab, edges, 1, out intersection))
                    return (intersection[0] - a).magnitude / abLength;
            }

            else if (overlapB)
            {
                Vector2[] intersection;
                LineSegment[] edges = rect.GetEdges();
                if (HandleIntersectionTest(ab, edges, 1, out intersection))
                    return (intersection[0] - b).magnitude / abLength;
            }

            else
            {
                Vector2[] intersections;
                LineSegment[] edges = rect.GetEdges();
                if (HandleIntersectionTest(ab, edges, 2, out intersections))
                    return (intersections[0] - intersections[1]).magnitude / abLength;
            }

            return 0;
        }

        private static bool HandleIntersectionTest(LineSegment line,
            LineSegment[] lines, int intersectionsRequired, out Vector2[] intersections)
        {
            intersections = new Vector2[intersectionsRequired];
            for (int i = 0; i < intersectionsRequired; i++) intersections[i] = Vector2.positiveInfinity;

            //foreach (Segment otherLine in lines)
            int intersectionsMade = 0;
            foreach (var otherLine in lines)
            {
                switch (LineIntersectionTest(line, otherLine, out intersections[intersectionsMade]))
                {
                    case LineIntersectionResult.False: break;
                    case LineIntersectionResult.True:
                        intersectionsRequired--;
                        intersectionsMade++;
                        break;
                    case LineIntersectionResult.Collinear:
                        //intersectionsRequired = 0;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
                if (intersectionsRequired <= 0) return true;
            }

            return false;
        }

        private static LineIntersectionResult LineIntersectionTest(LineSegment line1, LineSegment line2, out Vector2 intersectionPoint)
        {
            return LineIntersectionTest(line1.start, line1.end, line2.start, line2.end, out intersectionPoint);
        }

        //Adapted from http://www.realtimerendering.com/resources/GraphicsGems/gemsii/xlines.c
        private static LineIntersectionResult LineIntersectionTest(
            Vector2 line1start, Vector2 line1end,
            Vector2 line2start, Vector2 line2end,
            out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.positiveInfinity;

            float a1, a2, b1, b2, c1, c2; //Coefficients of line equations.
            float r1, r2, r3, r4;         //'Sign' values
            float denom, offset, num;     //Intermediate values
            float x1, y1, x2, y2, x3, y3, x4, y4, x, y; //Adaptation values;

            x1 = line1start.x; x2 = line1end.x; x3 = line2start.x; x4 = line2end.x;
            y1 = line1start.y; y2 = line1end.y; y3 = line2start.y; y4 = line2end.y;

            // Compute a1, b1, c1, where line joining points 1 and 2
            // is "a1 x  +  b1 y  +  c1  =  0".
            a1 = y2 - y1;
            b1 = x1 - x2;
            c1 = x2 * y1 - x1 * y2;

            //Compute r3 and r4.
            r3 = a1 * x3 + b1 * y3 + c1;
            r4 = a1 * x4 + b1 * y4 + c1;

            //Check signs of r3 and r4.  If both point 3 and point 4 lie on
            //same side of line 1, the line segments do not intersect.
            if (
                //!Utilities.NearlyEqual(r3, 0, float.Epsilon) &&
                //!Utilities.NearlyEqual(r4, 0, float.Epsilon) &&
                Utilities.NearlyEqual(Mathf.Sign(r3), Mathf.Sign(r4), float.Epsilon))
                return LineIntersectionResult.False;

            /* Compute a2, b2, c2 */
            a2 = y4 - y3;
            b2 = x3 - x4;
            c2 = x4 * y3 - x3 * y4;

            /* Compute r1 and r2 */
            r1 = a2 * x1 + b2 * y1 + c2;
            r2 = a2 * x2 + b2 * y2 + c2;

            //Check signs of r1 and r2.  If both point 1 and point 2 lie
            //on same side of second line segment, the line segments do
            //not intersect.
            if (
                //!Utilities.NearlyEqual(r1, 0, float.Epsilon) &&
                //!Utilities.NearlyEqual(r2, 0, float.Epsilon) &&
                Utilities.NearlyEqual(Mathf.Sign(r1), Mathf.Sign(r2), float.Epsilon))
                return LineIntersectionResult.False;

            //Line segments intersect: compute intersection point. 
            denom = a1 * b2 - a2 * b1;
            if (Utilities.NearlyEqual(denom, 0, float.Epsilon))
                return LineIntersectionResult.Collinear;

            offset = denom < 0 ? -denom / 2 : denom / 2;

            //The denom/2 is to get rounding instead of truncating.  It
            //is added or subtracted to the numerator, depending upon the
            //sign of the numerator.

            num = b1 * c2 - b2 * c1;
            x = (num < 0 ? num - offset : num + offset) / denom;

            num = a2 * c1 - a1 * c2;
            y = (num < 0 ? num - offset : num + offset) / denom;

            intersectionPoint = new Vector2(x, y);
            return LineIntersectionResult.True;
        }
    }

    public static class RectExtension
    {
        public static Vector2 TopLeftCorner(this Rect rect) { return rect.position; }
        public static Vector2 TopRightCorner(this Rect rect) { return new Vector2(rect.xMax, rect.y); }
        public static Vector2 BottomLeftCorner(this Rect rect) { return new Vector2(rect.x, rect.yMax); }
        public static Vector2 BottomRightCorner(this Rect rect) { return rect.position + rect.size; }

        /// <summary>
        /// Gets all the corners of the Rect in clockwise order, starting from the top left corner. 
        /// </summary>
        public static Vector2[] GetCorners(this Rect rect)
        {
            Vector2[] corners = new Vector2[4];
            corners[0] = rect.TopLeftCorner();
            corners[1] = rect.TopRightCorner();
            corners[2] = rect.BottomRightCorner();
            corners[3] = rect.BottomLeftCorner();
            return corners;
        }

        /// <summary>
        /// The top edge of the Rect; Left to Right.
        /// </summary>
        public static LineSegment TopEdge(this Rect rect)
        {
            return new LineSegment(rect.TopLeftCorner(), rect.TopRightCorner());
        }
        /// <summary>
        /// The right edge of the Rect; Top to Bottom.
        /// </summary>
        public static LineSegment RightEdge(this Rect rect)
        {
            return new LineSegment(rect.TopRightCorner(), rect.BottomRightCorner());
        }
        /// <summary>
        /// The bottom edge of the Rect; Right to Left.
        /// </summary>
        public static LineSegment BottomEdge(this Rect rect)
        {
            return new LineSegment(rect.BottomRightCorner(), rect.BottomLeftCorner());
        }
        /// <summary>
        /// The left edge of the Rect; Bottom to Top.
        /// </summary>
        public static LineSegment LeftEdge(this Rect rect)
        {
            return new LineSegment(rect.BottomLeftCorner(), rect.TopLeftCorner());
        }

        /// <summary>
        /// The top edge of the Rect; Right to Left.
        /// </summary>
        public static LineSegment TopEdgeAlt(this Rect rect)
        {
            return new LineSegment(rect.TopRightCorner(), rect.TopLeftCorner());
        }
        /// <summary>
        /// The right edge of the Rect; Bottom to Top.
        /// </summary>
        public static LineSegment RightEdgeAlt(this Rect rect)
        {
            return new LineSegment(rect.BottomRightCorner(), rect.TopRightCorner());
        }
        /// <summary>
        /// The bottom edge of the Rect; Left to Right.
        /// </summary>
        public static LineSegment BottomEdgeAlt(this Rect rect)
        {
            return new LineSegment(rect.BottomLeftCorner(), rect.BottomRightCorner());
        }
        /// <summary>
        /// The left edge of the Rect; Top to Bottom.
        /// </summary>
        public static LineSegment LeftEdgeAlt(this Rect rect)
        {
            return new LineSegment(rect.TopLeftCorner(), rect.BottomLeftCorner());
        }

        /// <summary>
        /// Gets all the edges of the Rect in clockwise order, starting from the top edge.
        /// </summary>
        public static LineSegment[] GetEdges(this Rect rect)
        {
            Vector2[] corners = rect.GetCorners();
            LineSegment[] edges = new LineSegment[4];
            for (int index = 0; index < 4; index++)
            {
                int nextIndex = index + 1;
                if (nextIndex >= 4) nextIndex = 0;
                edges[index] = new LineSegment(corners[index], corners[nextIndex]);
            }

            return edges;
        }
    }
}
