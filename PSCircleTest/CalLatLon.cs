namespace PSCircleTest
{
    //https://qiita.com/r-fuji/items/5eefb451cf7113f1e51b を移植
    public class Vincenty
    {
        // 楕円体
        public const int ELLIPSOID_GRS80 = 1; // GRS80
        public const int ELLIPSOID_WGS84 = 2; // WGS84

        // 楕円体別の長軸半径と扁平率
        public static readonly Dictionary<int, (double, double)> GEODETIC_DATUM = new Dictionary<int, (double, double)>
        {
            { ELLIPSOID_GRS80, (6378137.0, 1 / 298.257222101) }, // [GRS80]長軸半径と扁平率
            { ELLIPSOID_WGS84, (6378137.0, 1 / 298.257223563) }  // [WGS84]長軸半径と扁平率
        };

        // 反復計算の上限回数
        public const int ITERATION_LIMIT = 1000;

        public static (double lat, double lon, double azimuth)? VincentyDirect(double lat, double lon, double azimuth, double distance, int ellipsoid = ELLIPSOID_GRS80)
        {
            // 計算時に必要な長軸半径(a)と扁平率(f)を定数から取得し、短軸半径(b)を算出する
            var (a, f) = GEODETIC_DATUM[ellipsoid];
            double b = (1 - f) * a;

            // ラジアンに変換する(距離以外)
            double φ1 = ToRadians(lat);
            double λ1 = ToRadians(lon);
            double α1 = ToRadians(azimuth);
            double s = distance;

            double sinα1 = Math.Sin(α1);
            double cosα1 = Math.Cos(α1);

            // 更成緯度(補助球上の緯度)
            double U1 = Math.Atan((1 - f) * Math.Tan(φ1));

            double sinU1 = Math.Sin(U1);
            double cosU1 = Math.Cos(U1);
            double tanU1 = Math.Tan(U1);

            double σ1 = Math.Atan2(tanU1, cosα1);
            double sinα = cosU1 * sinα1;
            double cos2α = 1 - sinα * sinα;
            double u2 = cos2α * (a * a - b * b) / (b * b);
            double A = 1 + u2 / 16384 * (4096 + u2 * (-768 + u2 * (320 - 175 * u2)));
            double B = u2 / 1024 * (256 + u2 * (-128 + u2 * (74 - 47 * u2)));

            // σをs/(b*A)で初期化
            double σ = s / (b * A);


            double cos2σm = 0d;
            double sinσ = 0d;
            double cosσ = 0d;
            double Δσ = 0d;
            double σʹ = 0d;
            // 以下の計算をσが収束するまで反復する
            for (int i = 0; i < ITERATION_LIMIT; i++)
            {
                cos2σm = Math.Cos(2 * σ1 + σ);
                sinσ = Math.Sin(σ);
                cosσ = Math.Cos(σ);
                Δσ = B * sinσ * (cos2σm + B / 4 * (cosσ * (-1 + 2 * cos2σm * cos2σm) - B / 6 * cos2σm * (-3 + 4 * sinσ * sinσ) * (-3 + 4 * cos2σm * cos2σm)));
                σʹ = σ;
                σ = s / (b * A) + Δσ;

                // 偏差が.000000000001以下ならbreak
                if (Math.Abs(σ - σʹ) <= 1e-12)
                    break;
            }

            // σが所望の精度まで収束したら以下の計算を行う
            double x = sinU1 * sinσ - cosU1 * cosσ * cosα1;
            double φ2 = Math.Atan2(sinU1 * cosσ + cosU1 * sinσ * cosα1, (1 - f) * Math.Sqrt(sinα * sinα + x * x));
            double λ = Math.Atan2(sinσ * sinα1, cosU1 * cosσ - sinU1 * sinσ * cosα1);
            double C = f / 16 * cos2α * (4 + f * (4 - 3 * cos2α));
            double L = λ - (1 - C) * f * sinα * (σ + C * sinσ * (cos2σm + C * cosσ * (-1 + 2 * cos2σm * cos2σm)));
            double λ2 = L + λ1;

            double α2 = Math.Atan2(sinα, -x) + Math.PI;

            return (ToDegrees(φ2), ToDegrees(λ2), ToDegrees(α2));
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }

}
