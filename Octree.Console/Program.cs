using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Octree.Console
{
    public class BBObject
    {
        public int Id;
        public BoundingBoxBound BB;

        private static int count = 0;
        public static BBObject GenRandom()
        {
            var rand = new Random(123);
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

        public static Random rand = new Random(123);
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
            NearestPointTest();
//            RayIntersectionTest();
        }
        
        private static void NearestPointTest()
        {
            var oc = new PointOctree<BBObject>(1.0f, Vector3.Zero, 0.01f);
            var oco = new OctreeOrg.PointOctree<BBObject>(1.0f, new OctreeOrg.Point(0.0f, 0.0f, 0.0f), 0.01f);
            var nObjects = 100000;
            var bbObjects = Enumerable.Range(0, nObjects).Select(t => BBObject.GenRandom()).ToList();
            
            var st = DateTime.UtcNow;
            foreach (var t in bbObjects)
            {
                oco.Add(t,new OctreeOrg.Point(t.BB.Center.X, t.BB.Center.Y, t.BB.Center.Z));
                
            }
            var duration = DateTime.UtcNow - st;
            System.Console.WriteLine($"Add time org: {duration}");
            
            st = DateTime.UtcNow;
            foreach (var t in bbObjects)
            {
                oc.Add(t, t.BB.Center);
            }
            duration = DateTime.UtcNow - st;
            System.Console.WriteLine($"Add time new: {duration}");

            var nRays = 1000000;
            var hitList = new List<BBObject>();
            var pointList = new List<(Vector3,float)>();
            var pointListOrg = new List<(OctreeOrg.Point,float)>();
            for (var i = 0; i < nRays; i++)
            {
                var pointTuple = ((Vector3, float)) (BBObject.RandVec(1.0f), BBObject.rand.NextDouble() * 0.1f);
                pointList.Add(pointTuple);
                var rvecOrg = new OctreeOrg.Point(pointTuple.Item1.X, pointTuple.Item1.Y, pointTuple.Item1.Z);
                pointListOrg.Add((rvecOrg, pointTuple.Item2));
            }

            st = DateTime.UtcNow;
            var hitCount = 0;
            for (var i = 0; i < nRays; i++)
            {
                var pt = pointListOrg[i];
                var hitListOrg = oco.GetNearby(pt.Item1, pt.Item2);
                hitCount += hitListOrg.Length;
                
                // var ptn = pointList[i];
                // hitList.Clear();
                // oc.GetNearby(ptn.Item1, ptn.Item2, hitList, false);
                //
                // if (hitListOrg.Length != hitList.Count)
                // {
                //     System.Console.WriteLine("Sequence Old:");
                //     oco.GetNearby(pt.Item1, pt.Item2, true );
                //     System.Console.WriteLine();
                //     System.Console.WriteLine("Sequence new:");
                //     oc.GetNearby(ptn.Item1, ptn.Item2, hitList, true);
                //     System.Console.WriteLine("Why?");
                // }
            }

            var durationOld = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time org: {durationOld}");
            System.Console.WriteLine($"Items: {hitCount}");
            System.Console.WriteLine($"per ms org: #{nRays / durationOld.TotalMilliseconds}");

            
            st = DateTime.UtcNow;
            hitCount = 0;
            for (var i = 0; i < nRays; i++)
            {
                var pt = pointList[i];
                hitList.Clear();
                oc.GetNearby(pt.Item1, pt.Item2, hitList);
                hitCount += hitList.Count;
            }

            var durationNew = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time new: {durationNew}");
            System.Console.WriteLine($"Items: {hitCount}");
            System.Console.WriteLine($"per ms new: #{nRays / durationNew.TotalMilliseconds}");
            
            System.Console.WriteLine($"RATIO: {durationOld/durationNew}");
        }

        private static void RayIntersectionTest()
        {
            var oc = new BoundsOctree<BBObject>(1.0f, Vector3.Zero, 0.01f, 1.0f);
            var oco = new OctreeOrg.BoundsOctree<BBObject>(1.0f, new OctreeOrg.Point(0.0f, 0.0f, 0.0f), 0.01f, 1.0f);
            var nObjects = 1000;
            foreach (var i in Enumerable.Range(0, nObjects))
            {
                var t = BBObject.GenRandom();
                oc.Add(t, t.BB);
                oco.Add(t,
                    new OctreeOrg.BoundingBox(new OctreeOrg.Point(t.BB.Center.X, t.BB.Center.Y, t.BB.Center.Z),
                        new OctreeOrg.Point(t.BB.Extents.X*2.0f, t.BB.Extents.Y*2.0f, t.BB.Extents.Z*2.0f)));
            }

            var nRays = 1000000;
            var hitList = new List<BBObject>();
            var hitListOrg = new List<BBObject>();
            var rayList = new List<Ray>();
            var rayListOrg = new List<OctreeOrg.Ray>();
            for (var i = 0; i < nRays; i++)
            {
                var rvec = Vector3.Normalize(BBObject.RandVec(1.0f));
                rayList.Add(new Ray(Vector3.Zero, rvec));
                var rvecOrg = new OctreeOrg.Point(rvec.X, rvec.Y, rvec.Z);
                rayListOrg.Add(new OctreeOrg.Ray(new OctreeOrg.Point(0.0f, 0.0f, 0.0f), rvecOrg));
            }


            var st = DateTime.UtcNow;
            for (var i = 0; i < nRays; i++)
            {
                oco.GetColliding(hitListOrg, rayListOrg[i]);
            }

            var durationOld = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time org: {durationOld}");
            System.Console.WriteLine($"#Hits: {hitListOrg.Count}");
            System.Console.WriteLine($"rays/ms org: #{nRays / durationOld.TotalMilliseconds}");

            st = DateTime.UtcNow;
            hitList.Clear();
            for (var i = 0; i < nRays; i++)
            {
                oc.GetColliding(hitList, rayList[i]);
            }

            var durationNew = DateTime.UtcNow - st;
            System.Console.WriteLine($"Time old: {durationNew}");
            System.Console.WriteLine($"#Hits: {hitList.Count}");
            System.Console.WriteLine($"rays/ms old: #{nRays / durationOld.TotalMilliseconds}");
            
            System.Console.WriteLine($"RATIO: {durationOld/durationNew}");

        }
    }
}