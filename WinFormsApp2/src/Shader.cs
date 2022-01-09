using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;

namespace WinFormsApp2.src
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;



        public Shader(string vertexPath, string fragmentPath)
        {

            string VertexShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }


            //Vertex Shader Compile
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            CompileShader(vertexShader);



            //Fragment Shader Compile
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShaderSource);
            CompileShader(fragmentShader);



            //Create Program and Link
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);


            //compile code is copied into the shader program so no longer need
            //Detach and Delete
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);





            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // このuniformの名前を得て、
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // Locationを得て、
                var location = GL.GetUniformLocation(Handle, key);

                // それをdictionaryに追加する
                _uniformLocations.Add(key, location);
            }
        }



        private static void CompileShader(int shader)
        {
            // シェーダーをコンパイルする
            GL.CompileShader(shader);

            // コンパイルエラーをチェック
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // プログラムをリンクする
            GL.LinkProgram(program);

            // リンクエラーをチェック
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }



        public void UseProgram()
        {
            GL.UseProgram(Handle);
        }








        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }



        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);

            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}
