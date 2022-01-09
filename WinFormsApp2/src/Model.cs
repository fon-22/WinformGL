using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WinFormsApp2.src
{
    public class Model
    {
        public readonly int Handle;


        public static Model GenVAO(float[] _vertex, uint[] _index)
        {
            int _VAO = GL.GenVertexArray();
            GL.BindVertexArray(_VAO);

            int _VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertex.Length * sizeof(float), _vertex, BufferUsageHint.StaticDraw);

            int _EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _index.Length * sizeof(uint), _index, BufferUsageHint.StaticDraw);




            //バインドされているVBOの情報をattributeとしてシェーダーに送る(GPUに送る)
            //Attribute Pointers
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            return new Model(_VAO);


        }
        public Model(int input)
        {
            Handle = input;
        }


        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }






    }
}
