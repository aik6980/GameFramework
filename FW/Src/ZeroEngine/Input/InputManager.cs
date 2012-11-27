using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;
using SlimDX.DirectInput;
using SlimDX.Multimedia;
using System.Windows.Forms;

// for keyboard input
using System.Runtime.InteropServices;
// for mouse input
using System.Drawing;

namespace ZeroEngine.Input
{
    public class CKeyboardStatus
    {
        byte[] m_KeyState = new byte[256];
        byte[] m_PrevKeyState = new byte[256];

        public CKeyboardStatus()
        {

        }

        public void UpdateKeyState()
        {
            m_KeyState.CopyTo(m_PrevKeyState, 0);
            GetKeyboardState(m_KeyState);
        }

        private bool IsKeyDown(Keys key, byte[] keyStateBuffer)
        {
            return ((keyStateBuffer[(int)key] & 0x80) != 0);
        }

        public bool IsKeyDown(Keys key)
        {
            return IsKeyDown(key, m_KeyState);
        }

        public bool IsKeyPressed(Keys key)
        {
            return IsKeyDown(key, m_KeyState) && !IsKeyDown(key, m_PrevKeyState);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetKeyboardState(byte[] lpKeyState);
    }

    public class CMouseStatus
    {
        Vector2 m_CurrPos;
        Vector2 m_PrevPos;
        Vector2 m_Velocity;
        public Vector2 Velocity
        {
            get { return m_Velocity; }
        }

        private Dictionary<int, bool> buttonsPressed = new Dictionary<int, bool>();

        public Vector2 GetCurrPosition( Form hWindow )
        {
            var posRel = hWindow.PointToClient(new Point((int)m_CurrPos.X, (int)m_CurrPos.Y));
            return new Vector2(posRel.X, posRel.Y);
        }

        public void UpdateMouseState()
        {
            m_PrevPos = m_CurrPos;
            m_CurrPos = new Vector2( Cursor.Position.X, Cursor.Position.Y);

            if ((m_CurrPos - m_PrevPos).LengthSquared() < Single.Epsilon)
                m_Velocity = Vector2.Zero;
            else
                m_Velocity = m_CurrPos - m_PrevPos;
        }

        public void ButtonPressed(int button)
        {
            buttonsPressed[button] = true;
        }

        public void ButtonReleased(int button)
        {
            buttonsPressed[button] = false;
        }

        public bool IsButtonDown(int button)
        {
            bool isButtonDown;
            if (buttonsPressed.TryGetValue(button, out isButtonDown))
                return isButtonDown;
            return false;
        }
    }

    class CInputManager
    {
        Form m_hWindow;
        // keys state
        CKeyboardStatus m_KeyboardStatus;
        CMouseStatus m_MouseStatus;

        public bool IsKeyDown(Keys key)
        {
            return m_KeyboardStatus.IsKeyDown(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return m_KeyboardStatus.IsKeyPressed(key);
        }

        public Vector2 GetCursorVelocity() { return m_MouseStatus.Velocity; }

        public void Initialize(Form hWindow)
        {
            m_hWindow = hWindow;
            CreateDevice();
        }

        int count = 0;
        public void Update()
        {
            m_KeyboardStatus.UpdateKeyState();
            m_MouseStatus.UpdateMouseState();

            if (m_KeyboardStatus.IsKeyPressed(Keys.LButton))
            {
                Debug.Helper.Log(String.Format("Point {0}", Cursor.Position));
                Debug.Helper.Log(String.Format("Point {0}", m_hWindow.PointToClient(Cursor.Position)));
                count++;
            }
            
        }

        public void Destroy()
        {
            ReleaseDevice();
        }

        void CreateDevice()
        {
            m_KeyboardStatus = new CKeyboardStatus();
            m_MouseStatus = new CMouseStatus();
        }

        void ReleaseDevice()
        {

        }
    }
}
