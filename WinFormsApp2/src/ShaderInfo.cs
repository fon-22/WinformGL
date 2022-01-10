using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using WinFormsApp2.src;
using WinFormsApp2;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace WinFormsApp2.src
{
    public class ShaderInfo
    {


        private List<Texture> Tex = new List<Texture>();

        private Stopwatch timer;
        private Camera _camera;
        private List<float[]> Vert;
        private List<uint[]> idx;
        private List<Shader> _shader;
        private List<string> texpath;

        public  ShaderInfo(Stopwatch _timer, Camera __camera, List<float[]> _Vert, List<uint[]> _idx, List<Shader> __shader, List<string> _texpath)
        {
            timer = _timer;
            _camera = __camera;
            Vert = _Vert;
            idx = _idx;
            _shader = __shader;
            texpath = _texpath;



        }

        



        public void GenUniform()
        {
            for(int i = 0; i < Vert.Count; i++)
            {

                _shader[i].UseProgram();

                for (int j = 0; j < texpath.Count; j++)
                {
                    //Texture型の変数のリストにテクスチャの数だけ入れる
                    Tex.Add(Texture.LoadFromFile(texpath[j]));
                    //Uniform登録
                    int tex0Location = GL.GetUniformLocation(_shader[i].Handle, "texture" + j.ToString());
                    GL.Uniform1(tex0Location, j);
                    
                    
                }

                for(int h = 0; h < texpath.Count; h++)
                {
                    //テクスチャの数だけバインド
                    Tex[h].Bind(TextureUnit.Texture0 + h);
                }


                float timeValue = (float)timer.Elapsed.TotalSeconds;

                int tLocation = GL.GetUniformLocation(_shader[i].Handle, "iTime");
                GL.Uniform1(tLocation, timeValue);
 

                var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(timeValue * 10f));
                _shader[i].SetMatrix4("model", model);
                _shader[i].SetMatrix4("view", _camera.GetViewMatrix());
                _shader[i].SetMatrix4("projection", _camera.GetProjectionMatrix());


                Model.GenVAO(Vert[i], idx[0]);
                GL.DrawElements(PrimitiveType.Triangles, idx[0].Length, DrawElementsType.UnsignedInt, 0);

            }

          
        }




    }
}
