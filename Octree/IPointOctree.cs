// <copyright file="IPointOctree.cs">
//     Distributed under the BSD Licence (see LICENCE file).
// 
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Numerics;

namespace Octree
{
    public interface IPointOctree<T>
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
        /// <param name="objPos">Position of the object.</param>
        void Add(T obj, Vector3 objPos);

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
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        bool Remove(T obj, Vector3 objPos);

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        void GetNearby(Ray ray, float maxDistance, List<T> collidingWith);

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified position.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="position">The position. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <returns>Objects within range.</returns>
        void GetNearby(Vector3 position, float maxDistance, List<T> collidingWith);

        /// <summary>
        /// Returns all objects in the tree.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <returns>All objects.</returns>
        ICollection<T> GetAll();
    }
    
    
}