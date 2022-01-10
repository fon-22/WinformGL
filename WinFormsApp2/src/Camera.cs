using OpenTK;
using OpenTK.Mathematics;
using System;

namespace WinFormsApp2.src
{

    public class Camera
    {
        // これらのベクトルは、カメラの外側を指す方向で、カメラがどのように回転したかを定義します。
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // X軸を中心とした回転 (ラジアン)
        private float _pitch;

        // Y軸を中心とした回転 (ラジアン)
        private float _yaw = -MathHelper.PiOver2; // これがないと、右90度回転した状態でスタートしてしまう。

        // カメラの視野角 (ラジアン)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // カメラの位置
        public Vector3 Position { get; set; }

        // ビューポートのアスペクト比,。プロジェクション行列で使う。
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;




        // 角度が更新されたらその都度角度計算(Pitch)
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // カメラが逆さまにならないように、ピッチの値を-89～89の間でクランプしています。
                // あとジンバルロックを防ぐため
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        // 角度が更新されたらその都度角度計算(Yaw)
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        //
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }







        // ビュー行列生成
        public Matrix4 GetViewMatrix()
        {
            //第一引数がカメラ位置、ターゲット位置、ワールド空間の上ベクトル
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // プロジェクション行列生成
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // 角度が更新されたらベクトル計算
        private void UpdateVectors()
        {

            _front.X = (float)Math.Cos(_pitch) * (float)Math.Cos(_yaw);
            _front.Y = (float)Math.Sin(_pitch);
            _front.Z = (float)Math.Cos(_pitch) * (float)Math.Sin(_yaw);

            _front = Vector3.Normalize(_front);

            //  クロスプロダクトを使用して、右と上の両方のベクトルを計算します。
            //  この動作は、すべてのカメラに必要なものではないかもしれませんので、FPSカメラを必要としない場合は、この点に注意してください。
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}