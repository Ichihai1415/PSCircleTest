namespace PSCircleTest
{
    //https://qiita.com/matsuda_tkm/items/4eba5632535ca2f699b4 を移植
    public class GeodesicDistance
    {
        // 世界測地系(GRS80)
        private const double A = 6378137.0;
        private const double F = 1 / 298.257222101;
        private const double B = A * (1 - F);

        // 日本測地系(Bessel)
        // private const double A = 6377397.155;
        // private const double F = 1 / 299.152813;
        // private const double B = A * (1 - F);

        // Google Maps
        // private const double A = 6371008;
        // private const double B = A;
        // private const double F = (A - B) / A;

        public static double Dist(double[] p1, double[] p2)
        {
            // 緯度経度をラジアンに変換
            double lat1 = ToRadians(p1[0]);
            double lon1 = ToRadians(p1[1]);
            double lat2 = ToRadians(p2[0]);
            double lon2 = ToRadians(p2[1]);

            // 化成緯度に変換
            double phi1 = Math.Atan2(B * Math.Tan(lat1), A);
            double phi2 = Math.Atan2(B * Math.Tan(lat2), A);

            // 球面上の距離
            double X = Math.Acos(Math.Sin(phi1) * Math.Sin(phi2) + Math.Cos(phi1) * Math.Cos(phi2) * Math.Cos(lon2 - lon1));

            // Lambert-Andoyer補正
            double drho = F / 8 * ((Math.Sin(X) - X) * Math.Pow(Math.Sin(phi1) + Math.Sin(phi2), 2) / Math.Pow(Math.Cos(X / 2), 2) - (Math.Sin(X) + X) * Math.Pow(Math.Sin(phi1) - Math.Sin(phi2), 2) / Math.Pow(Math.Sin(X / 2), 2));

            // 距離
            double rho = A * (X + drho);

            return rho;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}
