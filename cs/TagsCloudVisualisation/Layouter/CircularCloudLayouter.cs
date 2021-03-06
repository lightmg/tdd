﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace TagsCloudVisualisation.Layouter
{
    public class CircularCloudLayouter : ICircularCloudLayouter
    {
        public CircularCloudLayouter(Point cloudCenter, Size minRectangleSize)
        {
            CloudCenter = cloudCenter;
            this.minRectangleSize = minRectangleSize;
        }

        private readonly Size minRectangleSize;
        public Point CloudCenter { get; }
        private readonly List<Rectangle> rectangles = new List<Rectangle>();

        private readonly ISet<CandidatePoint> points = new SortedSet<CandidatePoint>(CandidatePoint.ByDistanceComparer);

        public Rectangle PutNextRectangle(Size rectangleSize)
        {
            if (rectangles.Count == 0)
                return CreateAndRegisterRectangle(new Point(rectangleSize / -2) + (Size) CloudCenter, rectangleSize);

            var createdRectangle = PutRectangleAndGetOutdatedPoints(rectangleSize, out var pointsToRemove);
            foreach (var point in pointsToRemove)
                points.Remove(point);

            return RegisterRectangle(createdRectangle);
        }

        private Rectangle PutRectangleAndGetOutdatedPoints(Size rectangleSize, out IList<CandidatePoint> toRemove)
        {
            toRemove = new List<CandidatePoint>();
            foreach (var point in points)
            {
                var isCreated = TryCreateRectangle(point, rectangleSize, out var createdRectangle,
                    out var intersectsWithMinRect);

                if (isCreated || intersectsWithMinRect)
                    toRemove.Add(point);

                if (isCreated)
                    return createdRectangle;
            }

            throw new InvalidOperationException($"Can't find point to place rectangle {rectangleSize}");
        }

        private bool TryCreateRectangle(CandidatePoint location, Size nextRectangleSize, out Rectangle result,
            out bool intersectsWithMinRectangle)
        {
            var minSizedRectangle = PlaceRectangle(location, minRectangleSize);
            result = PlaceRectangle(location, nextRectangleSize);
            intersectsWithMinRectangle = false;

            foreach (var existingRectangle in rectangles)
            {
                intersectsWithMinRectangle = IntersectsOrConnected(minSizedRectangle, existingRectangle);
                if (intersectsWithMinRectangle || IntersectsOrConnected(result, existingRectangle))
                {
                    result = default;
                    return false;
                }
            }

            return true;
        }

        private Rectangle CreateAndRegisterRectangle(Point position, Size size)
        {
            return RegisterRectangle(new Rectangle(position, size));
        }

        private Rectangle RegisterRectangle(Rectangle rectangle)
        {
            foreach (var point in GetCornersOfRectangle(rectangle))
                points.Add(point);
            rectangles.Add(rectangle);
            return rectangle;
        }

        private ICollection<CandidatePoint> GetCornersOfRectangle(Rectangle rectangle) => new[]
        {
            new CandidatePoint(rectangle.Left, rectangle.Top, CloudCenter, PointDirection.Up),
            new CandidatePoint(rectangle.Right, rectangle.Top, CloudCenter, PointDirection.Right),
            new CandidatePoint(rectangle.Left, rectangle.Bottom, CloudCenter, PointDirection.Left),
            new CandidatePoint(rectangle.Right, rectangle.Bottom, CloudCenter, PointDirection.Down)
        };

        private static Rectangle PlaceRectangle(CandidatePoint point, Size size) =>
            new Rectangle(point.Direction switch
            {
                PointDirection.Up => new Point(point.X, point.Y - size.Height - 1),
                PointDirection.Down => new Point(point.X - size.Width, point.Y + 1),
                PointDirection.Left => new Point(point.X - size.Width - 1, point.Y - size.Height),
                PointDirection.Right => new Point(point.X + 1, point.Y),
                _ => throw new ArgumentOutOfRangeException(nameof(point.Direction))
            }, size);

        private static bool IntersectsOrConnected(Rectangle r1, Rectangle r2) =>
            r1.Left <= r2.Right && r2.Left <= r1.Right &&
            r1.Top <= r2.Bottom && r2.Top <= r1.Bottom;
    }
}