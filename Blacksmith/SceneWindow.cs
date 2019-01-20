using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Blacksmith
{
    public class SceneWindow : GameWindow
    {
        public SceneWindow() : base(800, 600)
        {
            //Keyboard.KeyDown += Keyboard_KeyDown;
        }

        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The KeyboardDevice which generated this event.</param>
        /// <param name="e">The key that was pressed.</param>
        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Exit();

            if (e.Key == Key.F11)
                if (WindowState == WindowState.Fullscreen)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Fullscreen;
        }

        private int _program;
        private int _vertexArray;

        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.Gray);

            GL.GenVertexArrays(1, out _vertexArray);
            GL.BindVertexArray(_vertexArray);
        }

        /// <summary>
        /// Respond to resize events here.
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
        }

        Vector3[] v = new Vector3[]
        {
new Vector3(0.1367f, 45.7578f, 22.0273f),
new Vector3(0.1367f, 45.7578f, 22.0273f),
new Vector3(0.1328f, 2.0078f, 21.9102f),
new Vector3(0.1328f, 2.0078f, 21.9102f),
new Vector3(0.1289f, 54.1797f, 21.2578f),
new Vector3(0.1289f, 54.1797f, 21.2578f),
new Vector3(7.3984f, 45.7578f, 21.0312f),
new Vector3(-7.3711f, 45.7578f, 20.9961f),
new Vector3(7.3477f, 2.0078f, 20.9258f),
new Vector3(-7.3242f, 2.0078f, 20.8906f),
new Vector3(0.1211f, 59.9258f, 19.4961f),
new Vector3(0.1211f, 59.9258f, 19.4961f),
new Vector3(0.1172f, -0.0000f, 19.2734f),
new Vector3(0.1172f, -0.0000f, 19.2734f),
new Vector3(0.1172f, -0.0000f, 19.2734f),
new Vector3(6.5273f, -0.0000f, 18.3984f),
new Vector3(6.5273f, -0.0000f, 18.3984f),
new Vector3(-6.5039f, -0.0000f, 18.3672f),
new Vector3(-6.5039f, -0.0000f, 18.3672f),
new Vector3(13.7773f, 45.7578f, 17.6406f),
new Vector3(-13.8633f, 45.7578f, 17.585f),
new Vector3(13.6875f, 2.0078f, 17.5586f),
new Vector3(-13.7695f, 2.0078f, 17.5039f),
new Vector3(13.0273f, 54.1797f, 17.1367f),
new Vector3(-13.1211f, 54.1797f, 17.097f),
new Vector3(11.8945f, 59.9258f, 15.7109f),
new Vector3(-11.9805f, 59.9258f, 15.675f),
new Vector3(0.0898f, 66.8672f, 15.4414f),
new Vector3(0.0898f, 66.8672f, 15.4414f),
new Vector3(12.1562f, -0.0000f, 15.4102f),
new Vector3(12.1562f, -0.0000f, 15.4102f),
new Vector3(-12.2266f, -0.0000f, 15.359f),
new Vector3(-12.2266f, -0.0000f, 15.359f),
new Vector3(-19.0117f, 45.7578f, 12.839f),
new Vector3(-18.8711f, 2.0078f, 12.7891f),
new Vector3(0.0664f, 69.9492f, 12.3594f),
new Vector3(0.0664f, 69.9492f, 12.3594f),
new Vector3(9.1914f, 66.8672f, 12.3008f),
new Vector3(-9.2539f, 66.8672f, 12.2734f),
new Vector3(19.4336f, 45.7578f, 12.1914f),
new Vector3(19.2930f, 2.0078f, 12.1445f),
new Vector3(-16.7578f, -0.0000f, 11.171f),
new Vector3(-16.7578f, -0.0000f, 11.171f),
new Vector3(17.1367f, -0.0000f, 10.6016f),
new Vector3(17.1367f, -0.0000f, 10.6016f),
new Vector3(6.8867f, 69.9492f, 10.1562f),
new Vector3(-6.9375f, 69.9492f, 10.1367f),
new Vector3(0.0391f, 72.2734f, 9.1367f),
new Vector3(0.0391f, 72.2734f, 9.1367f),
new Vector3(0.0391f, 72.2734f, 9.1367f),
new Vector3(0.0391f, 72.2734f, 9.1367f),
new Vector3(0.0391f, 75.0000f, 9.1328f),
new Vector3(0.0391f, 75.0000f, 9.1328f),
new Vector3(0.0352f, 76.1289f, 7.8945f),
new Vector3(0.0352f, 76.1289f, 7.8945f),
new Vector3(0.0352f, 76.1289f, 7.8945f),
new Vector3(0.0352f, 76.1289f, 7.8945f),
new Vector3(0.0352f, 80.0156f, 7.8945f),
new Vector3(0.0352f, 80.0156f, 7.8945f),
new Vector3(0.0352f, 80.0156f, 7.8945f),
new Vector3(4.1719f, 72.2734f, 7.8086f),
new Vector3(4.1719f, 72.2734f, 7.8086f),
new Vector3(4.1719f, 75.0000f, 7.8047f),
new Vector3(-4.1953f, 72.2734f, 7.7930f),
new Vector3(-4.1953f, 72.2734f, 7.7930f),
new Vector3(-4.1992f, 75.0000f, 7.7891f),
new Vector3(3.4414f, 76.1289f, 6.8008f),
new Vector3(3.4414f, 76.1289f, 6.8008f),
new Vector3(3.4414f, 80.0156f, 6.8008f),
new Vector3(3.4414f, 80.0156f, 6.8008f),
new Vector3(-3.4648f, 76.1289f, 6.7891f),
new Vector3(-3.4648f, 76.1289f, 6.7891f),
new Vector3(-3.4648f, 80.0156f, 6.7891f),
new Vector3(-3.4648f, 80.0156f, 6.7891f),
new Vector3(0.0234f, 80.8906f, 6.0156f),
new Vector3(21.5078f, 54.1758f, 5.8477f),
new Vector3(22.5820f, 45.7578f, 5.6953f),
new Vector3(22.4258f, 2.0078f, 5.6914f),
new Vector3(19.6133f, 59.9258f, 5.6367f),
new Vector3(-21.5977f, 54.1797f, 5.5977f),
new Vector3(-22.5352f, 2.0078f, 5.4609f),
new Vector3(-22.7031f, 45.7578f, 5.4570f),
new Vector3(-19.6992f, 59.9258f, 5.4375f),
new Vector3(2.3555f, 80.8906f, 5.2695f),
new Vector3(-2.3672f, 80.8906f, 5.2578f),
new Vector3(19.9180f, -0.0000f, 4.8711f),
new Vector3(19.9180f, -0.0000f, 4.8711f),
new Vector3(-20.0156f, -0.0000f, 4.6641f),
new Vector3(-20.0156f, -0.0000f, 4.6641f),
new Vector3(15.2383f, 66.8672f, 4.4688f),
new Vector3(-15.2695f, 66.8672f, 4.4492f),
new Vector3(6.8359f, 72.2773f, 4.1953f),
new Vector3(6.8359f, 72.2773f, 4.1953f),
new Vector3(6.8359f, 75.0000f, 4.1875f),
new Vector3(11.4023f, 69.9492f, 4.1641f),
new Vector3(-6.8672f, 72.2773f, 4.1250f),
new Vector3(-6.8672f, 72.2773f, 4.1250f),
new Vector3(-6.8711f, 75.0000f, 4.1172f),
new Vector3(-11.4531f, 69.9492f, 4.0273f),
new Vector3(5.6406f, 76.1289f, 3.8164f),
new Vector3(5.6406f, 76.1289f, 3.8164f),
new Vector3(5.6406f, 80.0156f, 3.8164f),
new Vector3(5.6406f, 80.0156f, 3.8164f),
new Vector3(-5.6680f, 76.1289f, 3.7578f),
new Vector3(-5.6680f, 76.1289f, 3.7578f),
new Vector3(-5.6680f, 80.0156f, 3.7578f),
new Vector3(-5.6680f, 80.0156f, 3.7578f),
new Vector3(3.8594f, 80.8906f, 3.2266f),
new Vector3(-3.8750f, 80.8906f, 3.1875f),
new Vector3(0.0000f, 80.5391f, 1.9453f),
new Vector3(3.8984f, 80.8906f, 0.6758f),
new Vector3(-3.8477f, 80.8906f, 0.6289f),
new Vector3(5.6992f, 76.1289f, 0.0859f),
new Vector3(5.6992f, 76.1289f, 0.0859f),
new Vector3(5.6992f, 80.0156f, 0.0859f),
new Vector3(5.6992f, 80.0156f, 0.0859f),
new Vector3(-5.6250f, 76.1289f, 0.0195f),
new Vector3(-5.6250f, 76.1289f, 0.0195f),
new Vector3(-5.6250f, 80.0156f, 0.0195f),
new Vector3(-5.6250f, 80.0156f, 0.0195f),
new Vector3(6.9023f, 72.2773f, -0.3281f),
new Vector3(6.9023f, 72.2773f, -0.3281f),
new Vector3(6.9062f, 75.0000f, -0.3359f),
new Vector3(-6.8125f, 72.2734f, -0.4102f),
new Vector3(-6.8125f, 72.2734f, -0.4102f),
new Vector3(-6.8203f, 75.0000f, -0.4141f),
new Vector3(-2.2617f, 80.8906f, -1.4297f),
new Vector3(2.2227f, 80.8906f, -1.4531f),
new Vector3(21.2500f, -0.0000f, -1.8906f),
new Vector3(21.2500f, -0.0000f, -1.8906f),
new Vector3(24.0469f, 45.7617f, -1.9023f),
new Vector3(11.1836f, 69.9492f, -1.9102f),
new Vector3(23.9297f, 2.0078f, -1.9180f),
new Vector3(-21.1211f, -0.0000f, -1.976f),
new Vector3(-21.1211f, -0.0000f, -1.976f),
new Vector3(-11.0625f, 69.9492f, -1.996f),
new Vector3(0.0000f, -0.0000f, -2.0000f),
new Vector3(-23.9219f, 45.7461f, -2.015f),
new Vector3(-23.7812f, 2.0078f, -2.0156f),
new Vector3(-0.0195f, 80.8906f, -2.0781f),
new Vector3(-3.3086f, 76.1289f, -2.9922f),
new Vector3(-3.3086f, 76.1289f, -2.9922f),
new Vector3(-3.3086f, 80.0156f, -2.9922f),
new Vector3(-3.3086f, 80.0156f, -2.9922f),
new Vector3(3.2500f, 76.1289f, -3.0234f),
new Vector3(3.2500f, 76.1289f, -3.0234f),
new Vector3(3.2500f, 80.0156f, -3.0234f),
new Vector3(3.2500f, 80.0156f, -3.0234f),
new Vector3(-14.4141f, 66.8672f, -3.210f),
new Vector3(14.5820f, 66.8672f, -3.2383f),
new Vector3(-0.0312f, 76.1289f, -3.9375f),
new Vector3(-0.0312f, 76.1289f, -3.9375f),
new Vector3(-0.0312f, 80.0156f, -3.9375f),
new Vector3(-0.0312f, 80.0156f, -3.9375f),
new Vector3(-4.0078f, 72.2734f, -4.0156f),
new Vector3(-4.0078f, 72.2734f, -4.0156f),
new Vector3(3.9375f, 72.2773f, -4.0508f),
new Vector3(3.9375f, 72.2773f, -4.0508f),
new Vector3(-4.0078f, 75.0000f, -4.0664f),
new Vector3(3.9414f, 75.0000f, -4.1016f),
new Vector3(-0.0391f, 75.0000f, -5.2109f),
new Vector3(0.0273f, 72.2617f, -5.2969f),
new Vector3(0.0273f, 72.2617f, -5.2969f),
new Vector3(19.0195f, 59.9258f, -5.6094f),
new Vector3(-18.7734f, 59.9258f, -5.757f),
new Vector3(-11.9688f, 66.8672f, -5.863f),
new Vector3(11.6172f, 66.8672f, -6.1641f),
new Vector3(-6.5352f, 69.9492f, -6.6992f),
new Vector3(6.4219f, 69.9492f, -6.7461f),
new Vector3(21.0938f, 54.2070f, -7.2031f),
new Vector3(-20.8125f, 54.2031f, -7.429f),
new Vector3(-7.8906f, 66.8672f, -7.7031f),
new Vector3(7.7500f, 66.8672f, -7.7148f),
new Vector3(20.1250f, -0.0000f, -8.3086f),
new Vector3(20.1250f, -0.0000f, -8.3086f),
new Vector3(-19.8672f, -0.0000f, -8.535f),
new Vector3(-19.8672f, -0.0000f, -8.535f),
new Vector3(-1.0273f, 69.9492f, -8.7070f),
new Vector3(-1.0273f, 69.9492f, -8.7070f),
new Vector3(-1.0273f, 69.9492f, -8.7070f),
new Vector3(1.1992f, 69.9492f, -8.7227f),
new Vector3(1.1992f, 69.9492f, -8.7227f),
new Vector3(1.1992f, 69.9492f, -8.7227f),
new Vector3(-2.0430f, 66.8672f, -8.8516f),
new Vector3(-2.0430f, 66.8672f, -8.8516f),
new Vector3(-2.0430f, 66.8672f, -8.8516f),
new Vector3(2.1953f, 66.8672f, -8.8672f),
new Vector3(2.1953f, 66.8672f, -8.8672f),
new Vector3(2.1953f, 66.8672f, -8.8672f),
new Vector3(-15.6953f, 59.9258f, -9.023f),
new Vector3(22.7656f, 45.7578f, -9.1328f),
new Vector3(22.6602f, 2.0078f, -9.1484f),
new Vector3(-22.4727f, 45.7578f, -9.386f),
new Vector3(-22.3672f, 2.0078f, -9.4023f),
new Vector3(15.3477f, 59.9258f, -9.5625f),
new Vector3(-10.5078f, 59.9258f, -11.03f),
new Vector3(10.4336f, 59.9258f, -11.425f),
new Vector3(-17.4883f, 54.2383f, -11.53f),
new Vector3(0.1133f, 59.9258f, -11.6484f),
new Vector3(16.8828f, 54.8398f, -11.933f),
new Vector3(-2.0234f, 66.0508f, -12.054f),
new Vector3(-2.0234f, 66.0508f, -12.054f),
new Vector3(2.1875f, 66.0508f, -12.0625f),
new Vector3(2.1875f, 66.0508f, -12.0625f),
new Vector3(-1.1367f, 68.7656f, -13.031f),
new Vector3(-1.1367f, 68.7656f, -13.031f),
new Vector3(1.3164f, 68.7656f, -13.0430f),
new Vector3(1.3164f, 68.7656f, -13.0430f),
new Vector3(-17.1172f, -0.0000f, -14.20f),
new Vector3(-17.1172f, -0.0000f, -14.20f),
new Vector3(-11.7109f, 54.2812f, -14.21f),
new Vector3(16.8008f, -0.0000f, -14.402f),
new Vector3(16.8008f, -0.0000f, -14.402f),
new Vector3(0.1680f, 54.3320f, -14.8203f),
new Vector3(11.6719f, 54.2188f, -14.972f),
new Vector3(-19.4062f, 45.8164f, -15.62f),
new Vector3(-19.2734f, 2.0078f, -15.789f),
new Vector3(19.0703f, 45.8086f, -15.871f),
new Vector3(18.9141f, 2.0078f, -16.0078f),
new Vector3(2.1641f, 61.7812f, -16.2539f),
new Vector3(2.1641f, 61.7812f, -16.2539f),
new Vector3(-1.9805f, 61.7812f, -16.257f),
new Vector3(-1.9805f, 61.7812f, -16.257f),
new Vector3(1.5625f, 64.4727f, -18.3516f),
new Vector3(1.5625f, 64.4727f, -18.3516f),
new Vector3(-1.3633f, 64.4727f, -18.355f),
new Vector3(-1.3633f, 64.4727f, -18.355f),
new Vector3(-11.6484f, -0.0000f, -18.91f),
new Vector3(-11.6484f, -0.0000f, -18.91f),
new Vector3(2.8008f, 57.4375f, -18.9375f),
new Vector3(2.8008f, 57.4375f, -18.9375f),
new Vector3(-2.5547f, 57.4414f, -18.941f),
new Vector3(-2.5547f, 57.4414f, -18.941f),
new Vector3(11.4453f, -0.0000f, -18.996f),
new Vector3(11.4453f, -0.0000f, -18.996f),
new Vector3(4.1641f, 50.4648f, -19.2188f),
new Vector3(4.1641f, 50.4648f, -19.2188f),
new Vector3(4.1641f, 50.4648f, -19.2188f),
new Vector3(-3.5898f, 50.4648f, -19.218f),
new Vector3(-3.5898f, 50.4648f, -19.218f),
new Vector3(-3.5898f, 50.4648f, -19.218f),
new Vector3(-3.1055f, 50.5742f, -19.601f),
new Vector3(-3.1055f, 50.5742f, -19.601f),
new Vector3(3.3242f, 50.7969f, -19.6562f),
new Vector3(3.3242f, 50.7969f, -19.6562f),
new Vector3(-2.7891f, 53.2109f, -19.957f),
new Vector3(-2.7891f, 53.2109f, -19.957f),
new Vector3(3.0312f, 53.2070f, -19.9609f),
new Vector3(3.0312f, 53.2070f, -19.9609f),
new Vector3(-13.1133f, 2.0078f, -21.082f),
new Vector3(12.8867f, 2.0078f, -21.1797f),
new Vector3(-13.2422f, 45.7227f, -21.31f),
new Vector3(13.0156f, 45.7109f, -21.421f),
new Vector3(6.1172f, -0.0000f, -21.4844f),
new Vector3(6.1172f, -0.0000f, -21.4844f),
new Vector3(-5.9805f, -0.0000f, -21.683f),
new Vector3(-5.9805f, -0.0000f, -21.683f),
new Vector3(2.0078f, 58.9688f, -21.9219f),
new Vector3(2.0078f, 58.9688f, -21.9219f),
new Vector3(-1.7773f, 58.9688f, -21.921f),
new Vector3(-1.7773f, 58.9688f, -21.921f),
new Vector3(0.1094f, -0.0000f, -22.4336f),
new Vector3(0.1094f, -0.0000f, -22.4336f),
new Vector3(6.8867f, 2.0078f, -23.9844f),
new Vector3(-6.7344f, 2.0078f, -24.2070f),
new Vector3(-1.9336f, 52.5781f, -24.441f),
new Vector3(-1.9336f, 52.5781f, -24.441f),
new Vector3(2.1758f, 52.5781f, -24.4570f),
new Vector3(2.1758f, 52.5781f, -24.4570f),
new Vector3(-7.3203f, 45.6875f, -24.480f),
new Vector3(7.1367f, 45.6836f, -24.6250f),
new Vector3(0.1250f, 2.0078f, -25.0508f),
new Vector3(-2.5859f, 45.6641f, -25.144f),
new Vector3(-2.5859f, 45.6641f, -25.144f),
new Vector3(-2.5859f, 45.6641f, -25.144f),
new Vector3(-2.0898f, 46.3242f, -25.160f),
new Vector3(-2.0898f, 46.3242f, -25.160f),
new Vector3(2.8359f, 45.6641f, -25.1719f),
new Vector3(2.8359f, 45.6641f, -25.1719f),
new Vector3(2.8359f, 45.6641f, -25.1719f),
new Vector3(2.3398f, 46.3242f, -25.1914f),
new Vector3(2.3398f, 46.3242f, -25.1914f)
        };

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //GL.Begin(PrimitiveType.Points);

            //GL.Color3(Color.White);
            GL.DrawArrays(PrimitiveType.Points, 0, v.Length);
            GL.PointSize(16);

            //GL.End();

            SwapBuffers();
        }
    }
}