using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WinFormsApp2.src
{

    public class Texture
    {
        public readonly int Handle;

        public static Texture LoadFromFile(string path)
        {
            // Generate handle
            int _texObj = GL.GenTexture();


            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texObj);

            //Load the image
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

            //ImageSharp loads from the top-left pixel, whereas OpenGL loads from the bottom-left, causing the texture to be flipped vertically.
            //This will correct that, making the texture display properly.
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }

            //フィルター方法
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //ラップ方法
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);


            //この時点でバインドされているテクスチャオブジェクトに実際の画像データを入れる
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());

            //ミップマップ生成
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return new Texture(_texObj);
        }

        //Handleに変換
        public Texture(int input)
        {
            Handle = input;
        }

        //テクスチャユニットをアクティブにしてさっき作ったhandleをHandleに変換しバインドすることでテクスチャユニットにテクスチャ登録
        public void Bind(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

        }


    }
}
