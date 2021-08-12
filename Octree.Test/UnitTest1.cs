using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;

namespace Octree.Test
{
    public class BBObject
    {
        public int Id;
        public BoundingBoxBound BB;

        private static int count = 0;
        public static BBObject GenRandom()
        {
            var rand = new Random();
            count++;
            return new BBObject()
            {
                Id = count,
                BB = new BoundingBoxBound(
                    RandVec(2.0f),
                    RandVec(0.1f)
                )
            };
        }

        private static Random rand = new Random();
        public static Vector3 RandVec(float s)
        {
            return new Vector3((float) (rand.NextDouble() - 0.5f) * s, (float) (rand.NextDouble() - 0.5f) * s,
                (float) (rand.NextDouble() - 0.5f) * s);
        }
    }
    
    public class BoundsOctreeTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRayCollision()
        {
            var oc = new BoundsOctree<BBObject>(1.0f, Vector3.Zero, 0.01f, 1.0f);
            var nObjects = 1000;
            foreach (var i in Enumerable.Range(0, nObjects))
            {
                var t = BBObject.GenRandom(); 
                oc.Add(t, t.BB);
            }

            var st = DateTime.UtcNow;
            var nRays = 10000;
            var hitList = new List<BBObject>();
            for (var i = 0; i < nRays; i++)
            {
                var d = Vector3.Normalize(BBObject.RandVec(1.0f));
                oc.GetColliding(hitList, new Ray(Vector3.Zero, d));
            }

            var duration = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time: {duration}");
            System.Console.WriteLine($"rays/ms: #{nRays/duration.TotalMilliseconds}");
        }
    }
}