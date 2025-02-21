using Arch.Core;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Server.Components.Collisions
{
    public struct ColliderComponent
    {
        public ColliderComponent()
        {
            ActiveCollisions = new HashSet<EntityReference>();
        }

        public Shape Shape;
        public Vector2 Offset;
        public HashSet<EntityReference> ActiveCollisions;
        public Action<Entity, Entity>? OnStart;
        public Action<Entity, Entity>? OnContinue;
        public Action<Entity, Entity>? OnExit;
    }

    public record struct Shape
    {
        public ShapeType Type { get; }
        public float Radius { get; }
        public Vector2 Size { get; }
        public static Shape Circle(float radius) => new Shape(ShapeType.Circle, radius);
        public static Shape Box(float width, float height) => new Shape(ShapeType.Box, size: new Vector2(width,height));

        public Vector2 ClosestPoint(Vector2 thisPosition, Vector2 otherPosition)
        {
            switch (Type)
            {
                case ShapeType.Box:
                    return ClosestBox(thisPosition, otherPosition);
                case ShapeType.Circle:
                    return ClosestCircle(thisPosition, otherPosition);
                default:
                    return thisPosition; // Fallback case
            }
        }

        private Vector2 ClosestCircle(Vector2 thisPosition, Vector2 otherPosition)
        {
            Vector2 direction = Vector2.Normalize(otherPosition - thisPosition);
            return thisPosition + direction * Radius;
        }

        private Vector2 ClosestBox(Vector2 thisPosition, Vector2 otherPosition)
        {
            // Define box boundaries
            Vector2 halfSize = Size / 2;
            Vector2 min = thisPosition - halfSize;
            Vector2 max = thisPosition + halfSize;

            // Clamp the point inside the box
            float clampedX = Math.Clamp(otherPosition.X, min.X, max.X);
            float clampedY = Math.Clamp(otherPosition.Y, min.Y, max.Y);

            return new Vector2(clampedX, clampedY);
        }
        private Shape(ShapeType type, float radius = 0, Vector2 size = default)
        {
            Type = type;
            Radius = radius;
            Size = size;
        }
    }

    public enum ShapeType
    {
        Circle,
        Box,
    }
}
