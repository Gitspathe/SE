using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SE.Engine.Utility;
using SE.Physics;
using SE.Core.Extensions;
using tainicom.Aether.Physics2D.Dynamics;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector2 = Microsoft.Xna.Framework.Vector2;
using AetherFixture = tainicom.Aether.Physics2D.Dynamics.Fixture;
using BodyType = SE.Physics.BodyType;
using AetherBodyType = tainicom.Aether.Physics2D.Dynamics.BodyType;

namespace SE.Core
{
    public static class Physics
    {
        public static float PixelsPerMeter = 64.0f;

        private static tainicom.Aether.Physics2D.Dynamics.World world;
        private static HashSet<PhysicsBody> pendingRemoveList = new HashSet<PhysicsBody>();
        private static HashSet<PhysicsBody> pendingCreateList = new HashSet<PhysicsBody>();
        private static List<RayCast2DHit> tmpHits = new List<RayCast2DHit>(32);

        public static Vector2 Gravity {
            get => world.Gravity.ToNumericsVector2();
            set => world.Gravity = value.ToMonoGameVector2();
        }

        public static void Initialize()
        {
            world = new tainicom.Aether.Physics2D.Dynamics.World();
            Gravity = Vector2.Zero;

            // Enable multi-threading.
            world.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
            world.ContactManager.PositionConstraintsMultithreadThreshold = 256;
            world.ContactManager.CollideMultithreadThreshold = 256;
        }

        public static void Update()
        {
            // Increment timer and step.
            int curStep = 0;
            while (curStep < Time.FixedTimeStepIterations) {
                world.Step(Time.FixedTimestep);
                ProcessPending();
                curStep++;
            }
        }

        /// <summary>
        /// Performs a ray-cast within the physics world.
        /// </summary>
        /// <param name="ray">Information used to perform the ray-cast.</param>
        /// <param name="hit">Information returned.</param>
        /// <returns>True if the ray-cast hit something.</returns>
        public static bool RayCast(Ray2D ray, out RayCast2DHit hit)
        {
            // The Aether.Physics2D call-back does not work as expected. So I return 1 to get all ray-cast results.
            float Callback(AetherFixture fixture, MonoGameVector2 point, MonoGameVector2 normal, float fraction)
            {
                if (fixture.DeeZFixture == null)
                    return 1;

                Vector2 hitPoint = ToPixels(point);
                Vector2 hitNormal = ToPixels(normal);
                float hitDistance = Vector2.Distance(hitPoint, ray.Position);
                tmpHits.Add(new RayCast2DHit(fixture.DeeZFixture, hitPoint, hitNormal, hitDistance));
                return 1;
            }

            tmpHits.Clear();
            RayCast2DHit foundHit = default;
            bool hitSomething = false;

            // Do the ray-cast, and determine the first hit.
            world.RayCast(Callback, ToMeters(ray.Position), ToMeters(ray.Destination));
            if (tmpHits.Count > 0) {
                tmpHits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                foundHit = tmpHits[0];
                hitSomething = true;
            }

            hit = foundHit;
            return hitSomething;
        }

        private static void ProcessPending()
        {
            // Create physics bodies which were created when the world was locked.
            foreach (PhysicsBody body in pendingCreateList) {
                world.Add(body.Body);
                body.AddedToPhysics = true;
                body.PendingCreate = false;
            }
            pendingCreateList.Clear();

            // Destroy physics bodies which were destroyed when the world was locked.
            foreach (PhysicsBody body in pendingRemoveList) {
                world.Remove(body.Body);
                body.AddedToPhysics = false;
                body.PendingRemove = false;
            }
            pendingRemoveList.Clear();
        }

        internal static void Add(PhysicsBody obj)
        {
            if(obj.AddedToPhysics || obj.PendingCreate)
                return;

            if (world.IsLocked) {
                pendingCreateList.Add(obj);
                obj.PendingCreate = true;
            } else {
                world.Add(obj.Body);
                obj.AddedToPhysics = true;
                obj.PendingCreate = false;
            }
            pendingRemoveList.Remove(obj);
            obj.PendingRemove = false;
        }

        internal static void Remove(PhysicsBody obj)
        {
            if(!obj.AddedToPhysics || obj.PendingRemove)
                return;

            if (world.IsLocked) {
                pendingRemoveList.Add(obj);
                obj.PendingRemove = true;
            } else {
                world.Remove(obj.Body);
                obj.AddedToPhysics = false;
                obj.PendingRemove = false;
            }
            pendingCreateList.Remove(obj);
            obj.PendingCreate = false;
        }

        public static float ToMeters(float pixel) 
            => pixel / PixelsPerMeter;

        public static MonoGameVector2 ToMeters(Vector2 pixelCoordinates) 
            => new MonoGameVector2(pixelCoordinates.X / PixelsPerMeter, pixelCoordinates.Y / PixelsPerMeter);

        public static MonoGameVector2 ToMeters(MonoGameVector2 pixelCoordinates)
            => new MonoGameVector2(pixelCoordinates.X / PixelsPerMeter, pixelCoordinates.Y / PixelsPerMeter);

        public static RectangleF ToMeters(RectangleF pixelRect) 
            => new RectangleF(pixelRect.X / PixelsPerMeter, pixelRect.Y / PixelsPerMeter, pixelRect.Width / PixelsPerMeter, pixelRect.Height / PixelsPerMeter);

        public static Vector2 ToPixels(Vector2 meterCoordinates) 
            => new Vector2(meterCoordinates.X * PixelsPerMeter, meterCoordinates.Y * PixelsPerMeter);

        public static Vector2 ToPixels(MonoGameVector2 meterCoordinates)
            => new Vector2(meterCoordinates.X * PixelsPerMeter, meterCoordinates.Y * PixelsPerMeter);

        public static RectangleF ToPixels(RectangleF metersRect)
            => new RectangleF(metersRect.X * PixelsPerMeter, metersRect.Y * PixelsPerMeter, metersRect.Width * PixelsPerMeter, metersRect.Height * PixelsPerMeter);

        public static float ToPixels(float meter) 
            => meter * PixelsPerMeter;

        public static PhysicsBody CreateBody(float rotation = 0, BodyType bodyType = BodyType.Static)
        {
            return new PhysicsBody(new Body {
                Rotation = rotation,
                BodyType = (AetherBodyType)(int)bodyType
            });
        }

        public static PhysicsBody CreateCircle(float radius, float density = 1.0f, BodyType bodyType = BodyType.Static)
            => CreateCircle(radius, density, default, bodyType);

        public static PhysicsBody CreateCircle(float radius, float density = 1.0f, Vector2 offset = default, BodyType bodyType = BodyType.Static)
        {
            PhysicsBody b = new PhysicsBody(new Body {
                BodyType = (AetherBodyType)(int)bodyType
            });
            b.AddCircle(radius, density, offset);
            return b;
        }

        public static PhysicsBody CreateRectangle(float width, float height, float density = 1.0f, Vector2 offset = default, BodyType bodyType = BodyType.Static)
            => CreateRectangle(width, height, 0f, density, offset, bodyType);

        public static PhysicsBody CreateRectangle(Rectangle rectangle, float density = 1.0f, BodyType bodyType = BodyType.Static) 
            => CreateRectangle(rectangle.Width, -rectangle.Height, 0f, density, new Vector2(rectangle.X, rectangle.Y), bodyType);

        public static PhysicsBody CreateRectangle(float width, float height, float rotation, float density, 
            Vector2 offset = default, BodyType bodyType = BodyType.Static)
        {
            PhysicsBody b = new PhysicsBody(new Body {
                BodyType = (AetherBodyType)(int)bodyType,
                Rotation = rotation
            });
            b.AddRectangle(width, height, density, offset);
            return b;
        }

        private static void Swap(ref int a, ref int b)
        {
            int c = a;
            a = b;
            b = c;
        }

        public static Point[] BresenhamLine(Point p0, Point p1)
        {
            return BresenhamLine(p0.X, p0.Y, p1.X, p1.Y);
        }

        private static Point[] BresenhamLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1) {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = 0;
            int ystep;
            int y = y0;
            if (y0 < y1) {
                ystep = 1;
            } else {
                ystep = -1;
            }

            Point[] result = new Point[(x1 - x0) + 1];
            int index = 0;
            for (int x = x0; x <= x1; x++) {
                if (steep) {
                    result[index] = new Point(y, x);
                } else {
                    result[index] = new Point(x, y);
                }

                error += deltay;
                if (2 * error >= deltax) {
                    y += ystep;
                    error -= deltax;
                }

                index++;
            }
            return result;
        }

    }
}
