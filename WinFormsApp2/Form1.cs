using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.WinForms;
using WinFormsApp2.src;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        private bool _loaded;

        private readonly float[] _vertices1 = {
             0.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,  1.0f,  // top right
             0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,  0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f,  0.0f,  1.0f   // top left
        };

        private readonly float[] _vertices2 = {
             1.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,  1.0f,  // top right
             1.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,  0.0f,  // bottom right
            -1.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // bottom left
            -1.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f,  0.0f,  1.0f   // top left
        };

        private readonly uint[] indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        private Shader _shader1 = null!;
        private Shader _shader2 = null!;

        private Texture _texture1 = null!;
        private Texture _texture2 = null!;

        private Model _VAO1;
        private Model _VAO2;

        private Camera _camera = null!;

        private bool IsFPS = true;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private INativeInput _nativeInput;


        private Stopwatch _timer1 = null!;
        private System.Windows.Forms.Timer _timer = null!;


        private Dictionary<string, int> _modelidx = new Dictionary<string, int>();




        const float FPScameraSpeed = 0.5f;


        public Form1()
        {
            InitializeComponent();
        }

        private void glControl_Load(object? sender, EventArgs e)
        {
            glControl1.Paint += glControl_Paint;


            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += (sender, e) =>
            {
 
                Render();
                
            };
            _timer.Interval = 16;   // 1000 ms per sec / 16 ms per frame = 60 FPS
            _timer.Start();



            _timer1 = new Stopwatch();
            _timer1.Start();


            //Generate VAO
            _VAO1 = Model.GenVAO(_vertices1, indices);
            _VAO2 = Model.GenVAO(_vertices2, indices);


            //Generate Textures and set to Uniforms
            _texture1 = Texture.LoadFromFile("D:/CGDevelop/WebGL/WebGLTest/awesomeface.png");
            _texture2 = Texture.LoadFromFile("D:/CGDevelop/WebGL/WebGLTest/Lava.png");


            _shader1 = new Shader("C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/shader.vert", "C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/shader1.frag");
            _shader1.UseProgram();


            int tex0Location = GL.GetUniformLocation(_shader1.Handle, "texture0");
            GL.Uniform1(tex0Location, 0);
            //_shader.SetInt("texture0", 0);

            int tex1Location = GL.GetUniformLocation(_shader1.Handle, "texture1");
            GL.Uniform1(tex1Location, 1);
            //_shader.SetInt("texture1", 1);


            _shader2 = new Shader("C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/shader.vert", "C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/shader2.frag");
            _shader2.UseProgram();


            int tex0Location2 = GL.GetUniformLocation(_shader2.Handle, "texture0");
            GL.Uniform1(tex0Location2, 0);
            //_shader.SetInt("texture0", 0);

            int tex1Location2 = GL.GetUniformLocation(_shader2.Handle, "texture1");
            GL.Uniform1(tex1Location2, 1);
            //_shader.SetInt("texture1", 1);





            //カメラの初期化(位置とウィンドウ)
            _camera = new Camera(Vector3.UnitZ * 2, Width / (float)Height);





            glControl1.MouseDown += (sender, e) =>
            {
                glControl1.Focus();
                Log($"WinForms Mouse down: ({e.X},{e.Y})");
            };
            glControl1.MouseUp += (sender, e) =>
                Log($"WinForms Mouse up: ({e.X},{e.Y})");
            glControl1.MouseMove += (sender, e) =>
                Log($"WinForms Mouse move: ({e.X},{e.Y})");
            glControl1.KeyDown += (sender, e) =>
            {
                Log($"WinForms Key down: {e.KeyCode}");
                if(e.KeyCode == Keys.W)
                _camera.Position += _camera.Front * FPScameraSpeed; // Forward
            };
            glControl1.KeyUp += (sender, e) =>
                Log($"WinForms Key up: {e.KeyCode}");
            glControl1.KeyPress += (sender, e) =>
                Log($"WinForms Key press: {e.KeyChar}");


        }


        private void glControl_Paint(object sender, PaintEventArgs e)
        {

            Render();


        }

        private void Render()
        {
            glControl1.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);


            _texture1.Bind(TextureUnit.Texture0);
            _texture2.Bind(TextureUnit.Texture1);



            //Time
            double timeValue = _timer1.Elapsed.TotalSeconds;
            float tValue = (float)(timeValue);







            _shader1.UseProgram();


            //Register as Uniform

            _shader1.SetFloat("iTime", tValue);
            //int timeLocation = GL.GetUniformLocation(_shader1.Handle, "iTime");
            //GL.Uniform1(timeLocation, tValue);



            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(tValue * 10f));
            _shader1.SetMatrix4("model", model);
            _shader1.SetMatrix4("view", _camera.GetViewMatrix());
            _shader1.SetMatrix4("projection", _camera.GetProjectionMatrix());


            
            //first Object Draw
            _VAO1.Bind();
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);



            _texture2.Bind(TextureUnit.Texture0);



            _shader2.UseProgram();

            //_shader2.SetFloat("iTime", tValue);
            int timeLocation2 = GL.GetUniformLocation(_shader2.Handle, "iTime");
            GL.Uniform1(timeLocation2, tValue);

            _shader2.SetMatrix4("view", _camera.GetViewMatrix());
            _shader2.SetMatrix4("projection", _camera.GetProjectionMatrix());


            model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(tValue * 20f));
            _shader2.SetMatrix4("model", model);

            //Second Object Draw
            _VAO2.Bind();
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);




            glControl1.SwapBuffers();
        }

        private void UpdateCamera(EventArgs s)
        {


            INativeInput nativeInput = glControl1.EnableNativeInput();

            if (_nativeInput == null)
            {
                _nativeInput = nativeInput;

                _nativeInput.MouseDown += (e) =>
                {
                    glControl1.Focus();
                    Log("Native Mouse down");
                };
                _nativeInput.MouseUp += (e) =>
                    Log("Native Mouse up");

                _nativeInput.MouseMove += (e) =>
                    Log($"Native Mouse move: {e.DeltaX},{e.DeltaY}");


                _nativeInput.KeyDown += (e) =>
                {
                    if (_nativeInput.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                    {
                        Log("AAAAAA");
                    }
                };
                    _nativeInput.KeyUp += (e) =>
                {
                    if (_nativeInput.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                    {

                        Log($"Native Key up: {e.Key}");
                    }
                };

                //_nativeInput.TextInput += (e) =>
                //    Log($"Native Text input: {e.AsString}");

                //_nativeInput.JoystickConnected += (e) =>
                //    Log($"Native Joystick connected: {e.JoystickId}");

                _nativeInput.KeyDown += (e) =>
                {
                    if (_nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.Escape))
                    {
                        Close();
                    }


                  
                    

                    if (IsFPS)
                    {
                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.W))
                        {
                            _camera.Position += _camera.Front * FPScameraSpeed; // Forward
                        }

                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.S))
                        {
                            _camera.Position -= _camera.Front * FPScameraSpeed; // Backwards
                        }
                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.A))
                        {
                            _camera.Position -= _camera.Right * FPScameraSpeed; // Left
                        }
                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.D))
                        {
                            _camera.Position += _camera.Right * FPScameraSpeed; // Right
                        }
                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.E))
                        {
                            _camera.Position += _camera.Up * FPScameraSpeed; // Up
                        }
                        if (nativeInput.IsKeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)Keys.Q))
                        {
                            _camera.Position -= _camera.Up * FPScameraSpeed; // Down
                        }

                    }
                };


                //const float sensitivity = 0.2f;

                ////Not FPS
                //const float PararellCameraSpeed = 0.01f;




                //if (_firstMove) // このbool変数は、初期値としてtrueに設定されている。
                //{
                //    _lastPos = new Vector2(mouse.X, mouse.Y);

                //    //最初のワンフレームだけの処理 Unityでいうvoid Start()と同じ
                //    _firstMove = false;
                //}
                //else
                //{
                //    Vector2 _storePos = new Vector2(mouse.X, mouse.Y);


                //    // マウスの位置のオフセットを計算
                //    float deltaX = mouse.X - _lastPos.X;
                //    float deltaY = mouse.Y - _lastPos.Y;

                //    _lastPos = new Vector2(mouse.X, mouse.Y);



                //    if (mouse.IsButtonDown(MouseButton.Right))
                //    {
                //        // カメラのピッチとヨーを適用する（カメラクラスでピッチをクランプしてあります。）
                //        _camera.Yaw += deltaX * sensitivity;
                //        _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
                //    }

                //    if (!IsFPS)
                //    {
                //        if (mouse.IsButtonDown(MouseButton.Left))
                //        {
                //            _camera.Position -= _camera.Right * PararellCameraSpeed * deltaX;
                //            _camera.Position += _camera.Up * PararellCameraSpeed * deltaY;
                //        }
                //    }
                //}






            }
        }

        private void Log(string message)
        {
            textBox1.AppendText(message + "\r\n");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine("timer hello");
            UpdateCamera(EventArgs.Empty);

        }

        private void glControl1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("glClick");
        }
    }
}
