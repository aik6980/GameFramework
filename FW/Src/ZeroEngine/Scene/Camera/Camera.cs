using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

using SlimDX;
using SlimDX.DirectInput;

using ZeroEngine.World;

namespace ZeroEngine.Scene.Camera
{
    public class CCamera
    {
        // property
        protected Vector4 m_Position = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        public Vector4 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        protected Quaternion m_Orientation = Quaternion.Identity;
        public Quaternion Orientation
        {
            get { return m_Orientation; }
            set { m_Orientation = value; }
        }
        
        // projection
        float m_fAspect = 4.0f/3.0f;
        float m_fFov    = (float)Math.PI/4.0f ;
        float m_fZNear  = 0.1f;
        float m_fZFar   = 1000.0f;

        public virtual void Update() { } 

        public void LookAt(Vector3 lookDir, Vector3 up)
        {
            lookDir.Normalize();
            Matrix matView = Matrix.LookAtLH(Vector3.Zero, lookDir, up);
            Quaternion qtnView = Quaternion.RotationMatrix(matView);
            m_Orientation = Quaternion.Invert(qtnView);
        }

        public void Projection(float fov, float aspect, float znear, float zfar)
        {
            m_fFov = fov;
            m_fAspect = aspect;
            m_fZNear = znear;
            m_fZFar = zfar;
        }

        public Matrix WorldToCam()
        {
            Vector3 offset = new Vector3(m_Position.X, m_Position.Y, m_Position.Z);
            Matrix m = Matrix.Invert(Matrix.RotationQuaternion(m_Orientation) * Matrix.Translation(offset));
            return m;
        }

        public Matrix CamToProj()
        {
            Matrix m = Matrix.PerspectiveFovLH(m_fFov, m_fAspect, m_fZNear, m_fZFar);
            return m;
        }

        public Matrix WorldToProj()
        {
            Matrix worldToCam = WorldToCam();
            Matrix camToProj = CamToProj();
            return Matrix.Multiply(worldToCam, camToProj);
        }
    }

    public class CFreeCamGamePad : CCamera
    {
        static float m_MoveSpeed = 1.0f;
        static float m_LookSpeed = 0.1f;

        public void Update()
        {
//             float yaw = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X * -m_LookSpeed;
//             float pitch = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y * m_LookSpeed;
// 
//             // use human-like rotation (use UNIT-Y world for Yaw operation)
//             Quaternion qtnYaw = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);
//             m_Orientation = Quaternion.Concatenate(m_Orientation, qtnYaw);
// 
//             // pitch and move are operated in the local frame
//             Matrix matOrient = Matrix.CreateFromQuaternion(m_Orientation);
// 
//             Vector3 axis = Vector3.Transform(Vector3.UnitX, Matrix.CreateFromQuaternion(m_Orientation));
//             axis = matOrient.Right;
//             Quaternion qtnPitch = Quaternion.CreateFromAxisAngle(axis, pitch);
//             m_Orientation = Quaternion.Concatenate(m_Orientation, qtnPitch);
//             
//             m_Position += GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X * m_MoveSpeed * matOrient.Right;
//             m_Position += GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y * m_MoveSpeed * matOrient.Forward;
        }
    }

    public class CFreeCamKB : CCamera
    {
        // control
        public override void Update()
        {
            if (Engine.Global.InputManager.IsKeyDown(Keys.LButton))
            {
                var v = Engine.Global.InputManager.GetCursorVelocity() * 0.01f;
                Yaw(v.X);
                Pitch(v.Y);
            }

            var moveSpeed = 1.2f * Engine.Global.FIXEDFRAMETIME;
            if (Engine.Global.InputManager.IsKeyDown(Keys.W))
                Fly(moveSpeed);
            if (Engine.Global.InputManager.IsKeyDown(Keys.S))
                Fly(-moveSpeed);
            if (Engine.Global.InputManager.IsKeyDown(Keys.A))
                Strafe(-moveSpeed);
            if (Engine.Global.InputManager.IsKeyDown(Keys.D))
                Strafe(moveSpeed);
        }

        // behavior
        public void Walk(float v)
        {
            Vector4 dir = Matrix.RotationQuaternion(m_Orientation).get_Rows(2);
            Vector4 speed = new Vector4(v, 0.0f, v, 0.0f);
            m_Position += Vector4.Modulate(dir, speed);
        }

        public void Fly(float v)
        {
            Vector4 dir = Matrix.RotationQuaternion(m_Orientation).get_Rows(2);
            m_Position += dir * v;
        }

        public void Strafe(float v)
        {
            Vector4 dir = Matrix.RotationQuaternion(m_Orientation).get_Rows(0);
            Vector4 speed = new Vector4(v, 0.0f, v, 0.0f);
            m_Position += Vector4.Modulate(dir, speed);
        }

        public void Pitch(float v)
        {
            var orientMat = Matrix.RotationQuaternion(m_Orientation);
            var rightVec4 = orientMat.get_Rows(0);
            var rightVec3 = new Vector3(rightVec4.X, rightVec4.Y, rightVec4.Z);

            // rotation matrix
            var rot = Quaternion.RotationAxis(rightVec3, v);
            m_Orientation = m_Orientation * rot;
        }

        public void Yaw(float v)
        {
            // rotation matrix
            var rot = Quaternion.RotationAxis(CWorld.WORLD_UP, v);
            m_Orientation = m_Orientation * rot;
        }
    }
}
