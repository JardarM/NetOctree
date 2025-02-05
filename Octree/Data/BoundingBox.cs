﻿// <copyright file="BoundingBox.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>

using System.Runtime.CompilerServices;

namespace Octree
{
    using System;
    using System.Numerics;

    public readonly struct BoundingBox
    {
        public Vector3 Center { get; }
        public Vector3 Extents { get; }

        public BoundingBox(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Vector3 pos)
        {
            var mm = Center - Extents;
            if (! (
                mm.X <= pos.X &&
                mm.Y <= pos.Y &&
                mm.Z <= pos.Z)) return false;

            mm = Center + Extents;
            return 
                mm.X >= pos.X &&
                mm.Y >= pos.Y &&
                mm.Z >= pos.Z;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(ref Vector3 pos, float maxDistance)
        {
            var e = Extents + new Vector3(maxDistance);
            var mm = Center - e;
            if (! (
                mm.X <= pos.X &&
                mm.Y <= pos.Y &&
                mm.Z <= pos.Z)) return false;

            mm = Center + e;
            return 
                mm.X >= pos.X &&
                mm.Y >= pos.Y &&
                mm.Z >= pos.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Intersects(ref BoundingBox boxBound)
        {
            var mm = Center - Extents;
            var bm = boxBound.Center + boxBound.Extents;
            if (! (mm.X <= bm.X && mm.Y <= bm.Y && mm.Z <= bm.Z) ) return false;
            
            mm = Center + Extents;
            bm = boxBound.Center - boxBound.Extents;
            return mm.X >= bm.X && mm.Y >= bm.Y && bm.Z >= bm.Z;
        }
       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectRay(ref Ray ray, ref Vector3 dirFrac, out float distance)
        {
            var mm = Center - Extents;
            var d1 = (mm - ray.Origin) * dirFrac;
            mm = Center + Extents;
            var d2 = (mm - ray.Origin) * dirFrac;
            
            mm = Vector3.Max(d1, d2);
            var tMax = Math.Min(Math.Min(mm.X, mm.Y), mm.Z);

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tMax < 0)
            {
                distance = tMax;
                return false;
            }

            mm = Vector3.Min(d1, d2);
            var tMin = Math.Max(Math.Max(mm.X, mm.Y), mm.Z);

            // if tmin > tmax, ray doesn't intersect AABB
            if (tMin > tMax)
            {
                distance = tMax;
                return false;
            }

            distance = tMin;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectRay(ref Ray ray, ref Vector3 dirFrac, float maxDistance, out float distance)
        {
            var mm = Extents - new Vector3(maxDistance);
            var d1 = (mm - ray.Origin) * dirFrac;
            mm = Extents + new Vector3(maxDistance);
            var d2 = (mm - ray.Origin) * dirFrac;
            
            mm = Vector3.Max(d1, d2);
            var tMax = Math.Min(Math.Min(mm.X, mm.Y), mm.Z);

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if (tMax < 0)
            {
                distance = tMax;
                return false;
            }

            mm = Vector3.Min(d1, d2);
            var tMin = Math.Max(Math.Max(mm.X, mm.Y), mm.Z);

            // if tmin > tmax, ray doesn't intersect AABB
            if (tMin > tMax)
            {
                distance = tMax;
                return false;
            }

            distance = tMin;
            return true;
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Extents.GetHashCode() << 2;
        }

        public override bool Equals(object other)
        {
            bool result;
            if (!(other is BoundingBox))
            {
                result = false;
            }
            else
            {
                BoundingBox boxBound = (BoundingBox)other;
                result = (Center.Equals(boxBound.Center) && Extents.Equals(boxBound.Extents));
            }
            return result;
        }

        public override string ToString()
        {
            return String.Format("Center: {0}, Extents: {1}", 
                Center, 
                Extents
            );
        }

        public string ToString(string format)
        {
            return String.Format("Center: {0}, Extents: {1}",
                Center.ToString(format),
                Extents.ToString(format)
            );
        }

        public static bool operator ==(BoundingBox lhs, BoundingBox rhs)
        {
            return lhs.Center == rhs.Center && lhs.Extents == rhs.Extents;
        }

        public static bool operator !=(BoundingBox lhs, BoundingBox rhs)
        {
            return !(lhs == rhs);
        }
    }
}

