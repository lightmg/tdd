﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace TagsCloudVisualisation.Layouter
{
    internal class CandidatePoint
    {
        public CandidatePoint(int x, int y, Point cloudCenter, PointDirection direction)
        {
            X = x;
            Y = y;
            Direction = direction;
            var relativeX = x - cloudCenter.X;
            var relativeY = y - cloudCenter.Y;
            CloudCenterDistance = Math.Sqrt(relativeX * relativeX + relativeY * relativeY);
        }

        public int X { get; }
        public int Y { get; }
        public double CloudCenterDistance { get; }
        public PointDirection Direction { get; }

        public static IComparer<CandidatePoint> ByDistanceComparer =>
            Comparer<CandidatePoint>.Create((p1, p2) =>
            {
                if (ReferenceEquals(p1, p2)) return 0;
                if (ReferenceEquals(null, p2)) return 1;
                return p1.CloudCenterDistance.CompareTo(p2.CloudCenterDistance);
            });
    }
}