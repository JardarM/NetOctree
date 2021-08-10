﻿// <copyright file="BoundingBox.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>
namespace Octree
{
    using System;
    using System.Runtime.Serialization;
    using System.Numerics;

    /// <summary>
    /// Represents an axis aligned bounding box (AABB).
    /// </summary>
    /// <remarks>
    /// This class was inspired by the Bounds type of the Unity Engine and 
    /// designed with the exact same interface to provide maximum compatibility.
    /// </remarks>
    [DataContract]
    public struct BoundingBox
    {
        /// <summary>
        /// Gets or sets the center of the bounding box.
        /// </summary>
        [DataMember]
        public Vector3 Center { get; set; }

        /// <summary>
        /// Gets or sets the extents of the bounding box. This is always half of the <see cref="Size"/>.
        /// </summary>
        [DataMember]
        public Vector3 Extents { get; set; }

        /// <summary>
        /// Gets or sets the size of the bounding box. This is always twice as large as the <see cref="Extents"/>.
        /// </summary>
        public Vector3 Size
        {
            get { return Extents * 2; }
            set { Extents = value * 0.5f; }
        }

        /// <summary>
        /// Gets or sets the minimal point of the box.
        /// </summary>
        /// <remarks>
        /// This is always equal to <c>center-extents</c>.
        /// </remarks>
        public Vector3 Min;
        // public Vector3 Min
        // {
        //     get { return Center - Extents; }
        //     set { SetMinMax(value, Max); }
        // }

        /// <summary>
        /// Gets or sets the maximal point of the box.
        /// </summary>
        /// <remarks>
        /// This is always equal to <c>center+extents</c>.
        /// </remarks>
        public Vector3 Max;
        // public Vector3 Max
        // {
        //     get { return Center + Extents; }
        //     set { SetMinMax(Min, value); }
        // }

        /// <summary>
        /// Creates a new bounding box.
        /// </summary>
        /// <param name="center">The center of the box.</param>
        /// <param name="size">The size of the box.</param>
        public BoundingBox(Vector3 center, Vector3 size)
        {
            Center = center;
            Extents = size * 0.5f;
            Min = Center - Extents;
            Max = Center + Extents;
        }

        /// <summary>
        /// Sets the bounds to the min and max value of the box.
        /// </summary>
        /// <param name="min">The minimal point.</param>
        /// <param name="max">The maximal point.</param>
        // public void SetMinMax(Vector3 min, Vector3 max)
        // {
        //     Extents = (max - min) * 0.5f;
        //     Center = min + Extents;
        // }

        // /// <summary>
        // /// Grows the bounding box include the point.
        // /// </summary>
        // /// <param name="vector3">The specified point to include.</param>
        // public void Encapsulate(Vector3 vector3)
        // {
        //     SetMinMax(Vector3.Min(Min, vector3), Vector3.Max(Max, vector3));
        // }
        //
        // /// <summary>
        // /// Grows the bounding box include the other box.
        // /// </summary>
        // /// <param name="box">The specified box to include.</param>
        // public void Encapsulate(BoundingBox box)
        // {
        //     Encapsulate(box.Center - box.Extents);
        //     Encapsulate(box.Center + box.Extents);
        // }

        /// <summary>
        /// Expands the bounds by increasing its <see cref="Size"/> by <paramref name="amount"/> along each side.
        /// </summary>
        /// <param name="amount">The expansions for each dimension.</param>
        public void Expand(float amount)
        {
            amount *= 0.5f;
            Extents += new Vector3(amount, amount, amount);
            Min = Center - Extents;
            Max = Center + Extents;
        }

        /// <summary>
        /// Expands the bounds by increasing its <see cref="Size"/> by <paramref name="amount"/> along each side.
        /// </summary>
        /// <param name="amount">The expansions for each dimension in order.</param>
        public void Expand(Vector3 amount)
        {
            Extents += amount * 0.5f;
            Min = Center - Extents;
            Max = Center + Extents;
        }

        /// <summary>
        /// Determines whether the box contains the point.
        /// </summary>
        /// <param name="vector3">The point to test.</param>
        /// <returns><c>true</c> if the box contains the point; otherwise, <c>false</c>.</returns>
        public bool Contains(Vector3 vector3)
        {
            return 
                Min.X <= vector3.X && Max.X >= vector3.X && 
                Min.Y <= vector3.Y && Max.Y >= vector3.Y && 
                Min.Z <= vector3.Z && Max.Z >= vector3.Z;
        }

        /// <summary>
        /// Determines whether the bounding box intersects with another box.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns><c>true</c> if the bounding box intersects with another box, <c>false</c> otherwise.</returns>
        public bool Intersects(BoundingBox box)
        {
            return 
                Min.X <= box.Max.X && Max.X >= box.Min.X && 
                Min.Y <= box.Max.Y && Max.Y >= box.Min.Y && 
                Min.Z <= box.Max.Z && Max.Z >= box.Min.Z;
        }

        /// <summary>
        /// Determines whether the bounding box intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <returns><c>true</c> if the box intersects with the ray, <c>false</c> otherwise.</returns>
        public bool IntersectRay(Ray ray)
        {
            float distance;
            return IntersectRay(ray, out distance);
        }

        /// <summary>
        /// Determines whether the bounding box intersects with a ray.
        /// </summary>
        /// <param name="ray">The ray to test.</param>
        /// <param name="distance">The calculated distance from the origin of the ray to the box along the ray.</param>
        /// <returns><c>true</c> if the box intersects with the ray, <c>false</c> otherwise.</returns>
        public bool IntersectRay(Ray ray, out float distance)
        {
            var dirFrac = new Vector3(
                1f / ray.Direction.X,
                1f / ray.Direction.Y,
                1f / ray.Direction.Z
            );

            float t1 = (Min.X - ray.Origin.X) * dirFrac.X;
            float t2 = (Max.X - ray.Origin.X) * dirFrac.X;
            float t3 = (Min.Y - ray.Origin.Y) * dirFrac.Y;
            float t4 = (Max.Y - ray.Origin.Y) * dirFrac.Y;
            float t5 = (Min.Z - ray.Origin.Z) * dirFrac.Z;
            float t6 = (Max.Z - ray.Origin.Z) * dirFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                distance = tmax;
                return false;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                distance = tmax;
                return false;
            }

            distance = tmin;
            return true;
        }
        
        public bool IntersectRayNew(ref Ray ray, out float distance)
        {
            var dirFrac = Vector3.One /  ray.Direction;

            var d1 = (Min - ray.Origin) * dirFrac;
            var d2 = (Max - ray.Origin) * dirFrac;
            
            var max = Vector3.Max(d1, d2);
            var tmax = Math.Min(Math.Min(max.X, max.Y), max.Z);

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tmax < 0)
            {
                distance = tmax;
                return false;
            }

            var min = Vector3.Min(d1, d2);
            var tmin = Math.Max(Math.Max(min.X, min.Y), min.Z);

            // if tmin > tmax, ray doesn't intersect AABB
            if (tmin > tmax)
            {
                distance = tmax;
                return false;
            }

            distance = tmin;
            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Extents.GetHashCode() << 2;
        }

        /// <summary>
        /// Determines whether the specified object as a <see cref="BoundingBox" /> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="BoundingBox" /> object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified box is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            bool result;
            if (!(other is BoundingBox))
            {
                result = false;
            }
            else
            {
                BoundingBox box = (BoundingBox)other;
                result = (Center.Equals(box.Center) && Extents.Equals(box.Extents));
            }
            return result;
        }

        /// <summary>
        /// Returns a nicely formatted string for this bounding box.
        /// </summary>
        public override string ToString()
        {
            return String.Format("Center: {0}, Extents: {1}", 
                Center, 
                Extents
            );
        }

        /// <summary>
        /// Returns a nicely formatted string for this bounding box.
        /// </summary>
        /// <param name="format">The format for the center and the extent.</param>
        public string ToString(string format)
        {
            return String.Format("Center: {0}, Extents: {1}",
                Center.ToString(format),
                Extents.ToString(format)
            );
        }

        /// <summary>
        /// Determines whether two bounding boxes are equal.
        /// </summary>
        /// <param name="lhs">The first box.</param>
        /// <param name="rhs">The second box.</param>
        public static bool operator ==(BoundingBox lhs, BoundingBox rhs)
        {
            return lhs.Center == rhs.Center && lhs.Extents == rhs.Extents;
        }

        /// <summary>
        /// Determines whether two bounding boxes are different.
        /// </summary>
        /// <param name="lhs">The first box.</param>
        /// <param name="rhs">The second box.</param>
        public static bool operator !=(BoundingBox lhs, BoundingBox rhs)
        {
            return !(lhs == rhs);
        }
    }
}

