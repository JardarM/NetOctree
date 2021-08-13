// <copyright file="IBoundsOctree.cs">
//     Distributed under the BSD Licence (see LICENCE file).
// 
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Octree
{
    public interface IBoundsOctree<T>
    {
        /// <summary>
        /// The total amount of objects currently in the tree
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the bounding box that represents the whole octree
        /// </summary>
        /// <value>The bounding box of the root node.</value>
        BoundingBox MaxBounds { get; }

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        void Add(T obj, BoundingBox objBounds);

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        bool Remove(T obj);

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objBounds">3D bounding box around the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        bool Remove(T obj, BoundingBox objBounds);

        /// <summary>
        /// Check if the specified bounds intersect with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>True if there was a collision.</returns>
        bool IsColliding(BoundingBox checkBounds);

        /// <summary>
        /// Check if the specified ray intersects with anything in the tree. See also: GetColliding.
        /// </summary>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>True if there was a collision.</returns>
        bool IsColliding(Ray checkRay, float maxDistance);

        /// <summary>
        /// Returns an array of objects that intersect with the specified bounds, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkBounds">bounds to check.</param>
        /// <returns>Objects that intersect with the specified bounds.</returns>
        void GetColliding(List<T> collidingWith, BoundingBox checkBounds);

        /// <summary>
        /// Returns an array of objects that intersect with the specified ray, if any. Otherwise returns an empty array. See also: IsColliding.
        /// </summary>
        /// <param name="collidingWith">list to store intersections.</param>
        /// <param name="checkRay">ray to check.</param>
        /// <param name="maxDistance">distance to check.</param>
        /// <returns>Objects that intersect with the specified ray.</returns>
        void GetColliding(List<T> collidingWith, Ray checkRay, float maxDistance = float.PositiveInfinity);
    }
}