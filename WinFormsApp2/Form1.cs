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

namespace WinFormsApp2.src
{
    public partial class Form1 : Form
    {
        private bool _loaded;

        private float[] _vertices1 = {
             0.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,  1.0f,  // top right
             0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,  0.0f,  // bottom right
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // bottom left
            -0.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f,  0.0f,  1.0f   // top left
        };

        private float[] _vertices2 = {
             1.5f,  0.5f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f,  1.0f,  // top right
             1.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,  1.0f,  0.0f,  // bottom right
            -1.5f, -0.5f, 0.0f,  0.0f, 0.0f, 1.0f,  0.0f,  0.0f,  // bottom left
            -1.5f,  0.5f, 0.0f,  1.0f, 1.0f, 0.0f,  0.0f,  1.0f   // top left
        };

        private uint[] _indices = {  // note that we start from 0!
            0, 1, 3,   // first triangle
            1, 2, 3    // second triangle
        };

        private List<string> modelPath = new List<string>();

        private List<float[]> Vertex = new List<float[]>();
        private List<uint[]> Idx = new List<uint[]>();



        private ShaderInfo _shaderinfo;

        private List<Shader> _shader = new List<Shader>();
        private List<string> texPath = new List<string>();



        private Camera _camera;

        private bool IsFPS = true;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private INativeInput _nativeInput;


        public Stopwatch _timer1 = null!;
        private System.Windows.Forms.Timer _timer = null!;


        private ListBox ModelBox = new ListBox();
        private ListBox ShaderBox = new ListBox();
        private ListBox TextureBox = new ListBox();


        const float FPScameraSpeed = 0.5f;


        public Form1()
        {
            InitializeComponent();
        }

        private void glControl_Load(object? sender, EventArgs e)
        {

            glControl1.Paint += glControl_Paint;
            glControl1.Resize += glControl_Resize;
            
            //タブのリストのそれぞれの初期化
            ModelBox.Dock = DockStyle.Fill;
            tabPage1.Controls.Add(ModelBox);

            ShaderBox.Dock = DockStyle.Fill;
            tabPage2.Controls.Add(ShaderBox);

            TextureBox.Dock = DockStyle.Fill;
            tabPage3.Controls.Add(TextureBox);

            //リスト内にものがある場合は表示(中に何もない場合はクラッシュしてしまうのでクラッシュ防止用)
            if (!(modelPath.Count == 0))
                listBox1.Items.AddRange(modelPath.ToArray());



            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += (sender, e) =>
            {
 
                Render();
                
            };
            _timer.Interval = 16;   // 1000 ms per sec / 16 ms per frame = 60 FPS
            _timer.Start();


            _timer1 = new Stopwatch();
            _timer1.Start();

            


            Vertex.Add(_vertices1);
            Vertex.Add(_vertices2);

            Idx.Add(_indices);

            Shader _shader1 = new Shader("C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/Shader.vert", "C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/Texture.frag");
            Shader _shader2 = new Shader("C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/Shader.vert", "C:/dev/OpenGL/WindowsFormsApp1/WindowsFormsApp1/src/shader/Shader2.frag");
            _shader.Add(_shader1);
            _shader.Add(_shader2);


            string s = "D:/CGDevelop/WebGL/WebGLTest/awesomeface.png";
            string h = "D:/CGDevelop/WebGL/WebGLTest/Lava.png";
            texPath.Add(s);
            texPath.Add(h);


            //カメラの初期化(位置とウィンドウ)
            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);
            _camera = new Camera(Vector3.UnitZ * 2, (float)glControl1.ClientSize.Width / (float)glControl1.ClientSize.Height);


            #region CameraControl
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
            #endregion
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


            _shaderinfo = new ShaderInfo(_timer1, _camera, Vertex, Idx, _shader, texPath);
            _shaderinfo.GenUniform();

            //model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(tValue * 20f));
            //_shader2.SetMatrix4("model", model);

            ////Second Object Draw
            //_VAO2.Bind();
            //GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            glControl1.SwapBuffers();

        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            glControl1.MakeCurrent();
            Log("Resized");
            if (glControl1.ClientSize.Height == 0)
                glControl1.ClientSize = new System.Drawing.Size(glControl1.ClientSize.Width, 1);


            GL.Viewport(0, 0, glControl1.ClientSize.Width, glControl1.ClientSize.Height);
            // リサイズされたらアスペクト比も更新
            _camera.AspectRatio = (float)glControl1.ClientSize.Width / (float)glControl1.ClientSize.Height;

            //glControl.Dock = DockStyle.Right;

   
        }

        private void UpdateCamera()
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
            //Debug.WriteLine("timer hello");
            UpdateCamera();

        }

        private void glControl1_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("glClick");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

            // デフォルトのフォルダを指定する
            ofDialog.InitialDirectory = @"D:";
            //ファイルフィルタ 
            ofDialog.Filter = "Model File(*.fbx,*.obj,*.dae,*.tif)|*.fbx;*.obj;*.dae;*.tif|FBX(*.fbx)|*.fbx|Wavefront OBJ(*.obj)|*.obj|Collada(*.dae)|*.dae";
            //ダイアログのタイトルを指定する
            ofDialog.Title = "3Dモデルを選択";

            //ダイアログを表示する
            if (ofDialog.ShowDialog() == DialogResult.OK)
            {
                modelPath.Add(ofDialog.FileName);
                listBox1.Items.AddRange(modelPath.ToArray());
                ModelBox.Items.Clear();
                ModelBox.Items.AddRange(modelPath.ToArray());
                Debug.WriteLine(ofDialog.FileName);
            }
            else
            {
                Debug.WriteLine("キャンセルされました");
            }

            // オブジェクトを破棄する
            ofDialog.Dispose();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();

  
            ofDialog.InitialDirectory = @"D:";
 
            ofDialog.Filter = "Shader File(*.vert,*.frag,*.glsl,*.tif)|*.vert;*.frag;*.glsl;*.tif|Vertex(*.vert)|*.vert|Fragment(*.frag)|*.frag|GLSL(*.glsl)|*.glsl";

            ofDialog.Title = "シェーダーを選択";


            if (ofDialog.ShowDialog() == DialogResult.OK)
            {
                modelPath.Add(ofDialog.FileName);
                listBox1.Items.AddRange(modelPath.ToArray());
                ShaderBox.Items.Clear();
                ShaderBox.Items.AddRange(modelPath.ToArray());

                Debug.WriteLine(ofDialog.FileName);
            }
            else
            {
                Debug.WriteLine("キャンセルされました");
            }

            // オブジェクトを破棄する
            ofDialog.Dispose();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();


            ofDialog.InitialDirectory = @"D:";
 
            ofDialog.Filter = "Image File(*.bmp,*.jpg,*.png,*.tif)|*.bmp;*.jpg;*.png;*.tif|Bitmap(*.bmp)|*.bmp|Jpeg(*.jpg)|*.jpg|PNG(*.png)|*.png";
         
            ofDialog.Title = "テクスチャを選択";

       
            if (ofDialog.ShowDialog() == DialogResult.OK)
            {
                modelPath.Add(ofDialog.FileName);
                listBox1.Items.AddRange(texPath.ToArray());
                TextureBox.Items.Clear();
                TextureBox.Items.AddRange(texPath.ToArray());

                Debug.WriteLine(ofDialog.FileName);
            }
            else
            {
                Debug.WriteLine("キャンセルされました");
            }

            // オブジェクトを破棄する
            ofDialog.Dispose();
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }




    }
}
