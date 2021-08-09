using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Octree.Console
{
    public class BBObject
    {
        public int Id;
        public BoundingBox BB;

        private static int count = 0;
        public static BBObject GenRandom()
        {
            var rand = new Random();
            count++;
            return new BBObject()
            {
                Id = count,
                BB = new BoundingBox(
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
    
    class Program
    {
        static void Main(string[] args)
        {
            var oc = new BoundsOctree<BBObject>(1.0f, Vector3.Zero, 0.01f, 1.0f);
            var nObjects = 1000;
            foreach (var i in Enumerable.Range(0, nObjects))
            {
                var t = BBObject.GenRandom(); 
                oc.Add(t, t.BB);
            }

            var nRays = 1000000;
            var hitList = new List<BBObject>();
            var rayList = new List<Ray>();
            for (var i = 0; i < nRays; i++)
                rayList.Add(new Ray(Vector3.Zero, Vector3.Normalize(BBObject.RandVec(1.0f))));

            var st = DateTime.UtcNow;
            for (var i = 0; i < nRays; i++)
            {
//                hitList.Clear();
                oc.GetColliding(hitList, rayList[i]);
            }
            var duration = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time old: {duration}");
            System.Console.WriteLine($"rays/ms old: #{nRays/duration.TotalMilliseconds}");

            st = DateTime.UtcNow;
            for (var i = 0; i < nRays; i++)
            {
//                hitList.Clear();
                oc.GetCollidingNew(hitList, rayList[i]);
            }
            duration = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time new: {duration}");
            System.Console.WriteLine($"rays/ms new: #{nRays/duration.TotalMilliseconds}");
        }
    }
}