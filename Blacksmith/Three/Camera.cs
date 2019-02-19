using OpenTK;
//using SlimDX;
//using SlimDX.Direct3D11;
using System;

namespace Blacksmith.Three
{
    /*internal class Camera
    {
        private Matrix projection = Matrix.Identity;
        private Matrix view = Matrix.Identity;

        private Vector3 position;
        private Vector3 origTarget;
        private Vector3 cameraFinalTarget;

        private Vector3 up = new Vector3(0, 1, 0);

        private float leftRightRot = 0;
        private float upDownRot = 0;

        public Ray CameraRay
        {
            get
            {
                //Q - P is a vector pointing from P to Q
                Vector3 direction = (cameraFinalTarget - position);
                direction.Normalize();
                return new Ray(position, direction);
            }
        }

        public Vector3 CamPosition
        {
            get { return position; }
        }

        public Matrix ViewProjection
        {
            get { return view * projection; }
        }

        /*internal CameraEventHandler CamEventHandlers
        {
            get
            {
                return new CameraEventHandler(new EventHandler(CameraMove), new EventHandler(CameraRotate));
            }
        }*

        public Camera(Vector3 pos, Vector3 target, ref Device device)
        {
            DeviceContext ctx = device.ImmediateContext;
            Viewport vp = ctx.Rasterizer.GetViewports()[0];
            Console.WriteLine("{0}x{1}", vp.Width, vp.Height);

            projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, vp.Width / vp.Height, 0.3f, 20000f);
            view = Matrix.LookAtLH(position, target, up);

            position = pos;
            origTarget = target;
        }

        public void TakeALook()
        {
            Matrix cameraRotation = Matrix.RotationX(upDownRot) * Matrix.RotationY(leftRightRot);

            Vector3 cameraRotatedTarget = Vector3.TransformCoordinate(origTarget, cameraRotation);
            cameraFinalTarget = position + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.TransformCoordinate(up, cameraRotation);

            view = Matrix.LookAtLH(position, cameraFinalTarget, cameraRotatedUpVector);
        }

        public void CameraMove(Vector3 vectorToAdd)
        {
            //Eventhandler, called when camera moved
            //object == Vector3 -> vector to add

            Matrix cameraRotation = Matrix.RotationX(upDownRot) * Matrix.RotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.TransformCoordinate(vectorToAdd, cameraRotation);

            position += rotatedVector;
        }

        private void CameraRotate(Vector2 rotation)
        {
            //Eventhandler, called when camera rotated
            //object == Vector2
            //x == leftRightRot
            //y == upDownRot

            leftRightRot += rotation.X;
            upDownRot -= rotation.Y;
        }
    }*/

    public class Camera
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Orientation = new Vector3((float)Math.PI, 0, 0);
        public float MoveSpeed = 1f;
        public float MouseSensitivity = 0.0025f;

        public Matrix4 GetViewMatrix()
        {
            Vector3 lookat = new Vector3();
            lookat.X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y));
            lookat.Y = (float)Math.Sin(Orientation.Y);
            lookat.Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y));
            return Matrix4.LookAt(Position, Position + lookat, Vector3.UnitY);
        }

        public void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin(Orientation.X), 0, (float)Math.Cos(Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}