using PSCircleTest.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PSCircleTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static readonly double calSpan = 0.1;

        private void Form1_Load(object sender, EventArgs e)
        {
            TimeTable.GetTT();

            Directory.CreateDirectory($"output-csv-{calSpan}");

            var depthList = TimeTable.rawDatas.Dists.Keys.ToList();
            //var psds = new List<TimeTable.TJMA2001.TimePSDists>();
            foreach (var depth in depthList)
            {
                //var tmpData = new TimeTable.TJMA2001.TimePSDists();
                var tmpText = new StringBuilder("Seconds,P-Wave distance,S-Wave distance\n");
                for (double sec = 0; sec < 480; sec += calSpan)
                {
                    sec = Math.Round(sec, 2);//浮動小数点数計算誤差修正用
                    var (pDist, sDist) = TimeTable.GetDistLinear(depth, sec, 0, -1);
                    if (sec == (int)sec)
                        Console.WriteLine($"depth={depth:000}km  sec:{sec:000.0}  ->  P:{pDist}km, S:{sDist}km");
                    if (pDist != -1 || sDist != -1)
                    {
                        /*tmpData.TimeData.Add(new TimeTable.TJMA2001.TimePSDists.TimeData_
                        {
                            Seconds = sec,
                            PDist = pDist,
                            SDist = sDist
                        });*/
                        tmpText.Append(sec);
                        tmpText.Append(',');
                        tmpText.Append(pDist);
                        tmpText.Append(',');
                        tmpText.Append(sDist);
                        tmpText.AppendLine();
                    }

                }
                //psds.Add(tmpData);
                File.WriteAllText($"output-csv-{calSpan}\\{depth}.csv", tmpText.ToString());
            }
            /*
            var jsonText = JsonSerializer.Serialize(psds);
            File.WriteAllText($"psdists-{calSpan}.json", jsonText);
            */

            return;


            /*
            var psDist = TimeTable.GetDistLinear(300, 200.2);
            Console.WriteLine($"P:{psDist.PDist}km, S:{psDist.SDist}km");
            */

            config.HypoLat = 40.3;
            config.HypoLon = 145.2;
            config.HypoDepth = 170;

            //BackgroundImage = EEWMap(0.7);
            //Console.WriteLine((DateTime.Now - st).TotalMilliseconds + "ms!");

            AutoExe.Enabled = true;//アニメーション実行

            return;
            BackgroundImage = Draw_Map();
        }

        private const int V = 8;
        public static Config_Color color = new();

        public static Config_Map config = new();


        public static Bitmap Draw_Map()
        {

            var centerLat = 25d;
            var centerLon = 125d;

            var a = 255;
            var cor = 1000 / 255d;

            var deltaD = 0.1d;//360/deltaD+1個の点を計算します 
            var newDist = 1000 * 2000d;


            var mapImg = new Bitmap(config.MapSize, config.MapSize);
            var zoomW = config.MapSize / (config.LonEnd - config.LonSta);
            var zoomH = config.MapSize / (config.LatEnd - config.LatSta);
            var mapjson = JsonNode.Parse(Resources.AreaForecastLocalE_GIS_20190125_01);
            var g = Graphics.FromImage(mapImg);
            g.Clear(color.Map.Sea);


            var stTime = DateTime.Now;
            Console.WriteLine($"距離描画開始 {DateTime.Now:HH:mm:ss.ffff}");
            for (int x = 0; x < config.MapSize; x++)
            {
                var st2Time = DateTime.Now;
                for (int y = 0; y < config.MapSize; y++)
                {
                    var imgLat = config.LatEnd - y / zoomH;
                    var imgLon = config.LonSta + x / zoomW;
                    var dist = GeodesicDistance.Dist([centerLat, centerLon], [imgLat, imgLon]) / 1000d;//km
                    if (double.IsNaN(dist))
                        continue;
                    var level = dist / cor;//色段階
                    var co = level > 765 ? Color.FromArgb(a, 128, 128, 128) : level > 510 ? Color.FromArgb(a, 0, 0, (int)level - 510) : level > 255 ? Color.FromArgb(a, 0, (int)level - 255, 0) : Color.FromArgb(a, (int)level, 0, 0);
                    g.DrawLine(new Pen(new SolidBrush(co)), x, y, x + 1, y);

                }//255*cor kmが赤

                var remain = (DateTime.Now - stTime) / (x + 1) * (config.MapSize - x - 1);
                Console.Write($"\r距離描画:[{x + 1}/{config.MapSize}]  残り{remain:mm\\:ss\\.ffff}");
            }
            Console.WriteLine($"\n距離描画完了 {DateTime.Now:HH:mm:ss.ffff} ({(DateTime.Now - stTime):mm\\:ss\\.ffff})");

            var cx = (int)((centerLon - config.LonSta) * zoomW);
            var cy = (int)((config.LatEnd - centerLat) * zoomH);
            g.DrawLine(new Pen(Brushes.White), cx, 0, cx, config.MapSize);
            g.DrawLine(new Pen(Brushes.White), 0, cy, config.MapSize, cy);


            var gPath = new GraphicsPath();
            gPath.StartFigure();
            foreach (var mapjson_feature in mapjson["features"].AsArray().Where(x => x["geometry"] != null))
            {
                if ((string?)mapjson_feature["geometry"]["type"] == "Polygon")
                {
                    var points = mapjson_feature["geometry"]["coordinates"][0].AsArray().Select(mapjson_coordinate => new Point((int)(((double)mapjson_coordinate[0] - config.LonSta) * zoomW), (int)((config.LatEnd - (double)mapjson_coordinate[1]) * zoomH))).ToArray();
                    if (points.Length > 2)
                        gPath.AddPolygon(points);
                }
                else
                {
                    foreach (var mapjson_coordinates in mapjson_feature["geometry"]["coordinates"].AsArray())
                    {
                        var points = mapjson_coordinates[0].AsArray().Select(mapjson_coordinate => new Point((int)(((double)mapjson_coordinate[0] - config.LonSta) * zoomW), (int)((config.LatEnd - (double)mapjson_coordinate[1]) * zoomH))).ToArray();
                        if (points.Length > 2)
                            gPath.AddPolygon(points);
                    }
                }
            }
            g.FillPath(new SolidBrush(color.Map.Japan), gPath);
            g.DrawPath(new Pen(color.Map.Japan_Border, config.MapSize / 1080f), gPath);
            Console.WriteLine("地図描画終了");


            var psApprox = new List<Point>();
            for (double d = 0; d <= 360; d += deltaD)
            {
                var pt = Vincenty.VincentyDirect(centerLat, centerLon, d, newDist, 1);
                psApprox.Add(new Point((int)((pt.Value.lon - config.LonSta) * zoomW), (int)((config.LatEnd - pt.Value.lat) * zoomH)));
            }

            g.DrawPolygon(new Pen(Color.FromArgb(128, 255, 255, 255), 2), psApprox.ToArray());
            Console.WriteLine("新円描画終了");
            //throw new Exception();


            var path = $"output\\{DateTime.Now:yyyyMMddHHmmss}.png";
            Directory.CreateDirectory("output");
            mapImg.Save(path, ImageFormat.Png);
            Console.WriteLine(Path.GetFullPath(path) + " に保存しました");

            g.Dispose();
            return mapImg;
        }

        public Bitmap EEWMap(double sec = 10)
        {


            var a = 128;
            var cor = 1000 / 255d;

            var deltaD = 0.1d;//360/deltaD+1個の点を計算します 



            var mapImg = new Bitmap(config.MapSize, config.MapSize);
            var zoomW = config.MapSize / (config.LonEnd - config.LonSta);
            var zoomH = config.MapSize / (config.LatEnd - config.LatSta);
            var mapjson = JsonNode.Parse(Resources.AreaForecastLocalE_GIS_20190125_01);
            var g = Graphics.FromImage(mapImg);
            g.Clear(color.Map.Sea);



            var gPath = new GraphicsPath();
            gPath.StartFigure();
            foreach (var mapjson_feature in mapjson["features"].AsArray().Where(x => x["geometry"] != null))
            {
                if ((string?)mapjson_feature["geometry"]["type"] == "Polygon")
                {
                    var points = mapjson_feature["geometry"]["coordinates"][0].AsArray().Select(mapjson_coordinate => new Point((int)(((double)mapjson_coordinate[0] - config.LonSta) * zoomW), (int)((config.LatEnd - (double)mapjson_coordinate[1]) * zoomH))).ToArray();
                    if (points.Length > 2)
                        gPath.AddPolygon(points);
                }
                else
                {
                    foreach (var mapjson_coordinates in mapjson_feature["geometry"]["coordinates"].AsArray())
                    {
                        var points = mapjson_coordinates[0].AsArray().Select(mapjson_coordinate => new Point((int)(((double)mapjson_coordinate[0] - config.LonSta) * zoomW), (int)((config.LatEnd - (double)mapjson_coordinate[1]) * zoomH))).ToArray();
                        if (points.Length > 2)
                            gPath.AddPolygon(points);
                    }
                }
            }
            g.FillPath(new SolidBrush(color.Map.Japan), gPath);
            g.DrawPath(new Pen(color.Map.Japan_Border, config.MapSize / 1080f), gPath);


            var psDist = TimeTable.GetDistLinear(config.HypoDepth, sec);

            var pApprox = new List<Point>();
            for (double d = 0; d <= 360; d += deltaD)
            {
                var pt = Vincenty.VincentyDirect(config.HypoLat, config.HypoLon, d, psDist.PDist * 1000, 1);
                pApprox.Add(new Point((int)((pt.Value.lon - config.LonSta) * zoomW), (int)((config.LatEnd - pt.Value.lat) * zoomH)));
            }

            var sApprox = new List<Point>();
            for (double d = 0; d <= 360; d += deltaD)
            {
                var pt = Vincenty.VincentyDirect(config.HypoLat, config.HypoLon, d, psDist.SDist * 1000, 1);
                sApprox.Add(new Point((int)((pt.Value.lon - config.LonSta) * zoomW), (int)((config.LatEnd - pt.Value.lat) * zoomH)));
            }

            g.DrawPolygon(new Pen(Color.FromArgb(0, 0, 255), 2), pApprox.ToArray());
            g.DrawPolygon(new Pen(Color.FromArgb(255, 0, 0), 2), sApprox.ToArray());
            g.FillPolygon(new SolidBrush(Color.FromArgb(64, 255, 0, 0)), sApprox.ToArray());
            //throw new Exception();


            var cx = (int)((config.HypoLon - config.LonSta) * zoomW);
            var cy = (int)((config.LatEnd - config.HypoLat) * zoomH);

            var penW = 4;
            var penL = (int)(penW * 1.5);

            int yellowW = Math.Max((int)(penW * 0.5), 1);
            int yellowWx = Math.Max((int)(penW * 0.18), 1);
            g.DrawLine(new Pen(Color.Yellow, penW + yellowW), cx - penL - yellowWx, cy - penL - yellowWx, cx + penL + yellowWx, cy + penL + yellowWx);
            g.DrawLine(new Pen(Color.Yellow, penW + yellowW), cx - penL - yellowWx, cy + penL + yellowWx, cx + penL + yellowWx, cy - penL - yellowWx);
            g.DrawLine(new Pen(Color.Red, penW), cx - penL, cy - penL, cx + penL, cy + penL);
            g.DrawLine(new Pen(Color.Red, penW), cx - penL, cy + penL, cx + penL, cy - penL);

            /*
            var path = $"output\\{DateTime.Now:yyyyMMddHHmmss}.png";
            Directory.CreateDirectory("output");
            mapImg.Save(path, ImageFormat.Png);
            Console.WriteLine(Path.GetFullPath(path) + " に保存しました");
            */
            g.Dispose();
            return mapImg;
        }

        public class Config_Map
        {
            /// <summary>
            /// 画像の高さ
            /// </summary>
            public int MapSize { get; set; } = 1080;





            /// <summary>
            /// 緯度の始点
            /// </summary>
            public double LatSta { get; set; } = 30;

            /// <summary>
            /// 緯度の終点
            /// </summary>
            public double LatEnd { get; set; } = 40;

            /// <summary>
            /// 経度の始点
            /// </summary>
            public double LonSta { get; set; } = 130;

            /// <summary>
            /// 経度の終点
            /// </summary>
            public double LonEnd { get; set; } = 140;

            /*

            /// <summary>
            /// 緯度の始点
            /// </summary>
            public double LatSta { get; set; } = 20;

            /// <summary>
            /// 緯度の終点
            /// </summary>
            public double LatEnd { get; set; } = 50;

            /// <summary>
            /// 経度の始点
            /// </summary>
            public double LonSta { get; set; } = 120;

            /// <summary>
            /// 経度の終点
            /// </summary>
            public double LonEnd { get; set; } = 150;

            */

            /// <summary>
            /// 震央の緯度
            /// </summary>
            public double HypoLat { get; set; } = double.NaN;

            /// <summary>
            /// 震央の経度
            /// </summary>
            public double HypoLon { get; set; } = double.NaN;

            /// <summary>
            /// 震源の深さ
            /// </summary>
            public double HypoDepth { get; set; } = double.NaN;

        }

        public struct GeoCoordinates
        {
            double Lat;
            double Lon;
        }

        public class Config_Color
        {
            /// <summary>
            /// 地図の色
            /// </summary>
            public MapColor Map { get; set; } = new MapColor();

            /// <summary>
            /// 地図の色
            /// </summary>
            public class MapColor
            {
                /// <summary>
                /// 海洋の塗りつぶし色
                /// </summary>
                public Color Sea { get; set; } = Color.FromArgb(30, 30, 60);

                /// <summary>
                /// 世界(日本除く)の塗りつぶし色
                /// </summary>
                public Color World { get; set; } = Color.FromArgb(100, 100, 150);
                /*
                /// <summary>
                /// 世界(日本除く)の境界線色
                /// </summary>
                public Color World_Border { get; set; }
                */
                /// <summary>
                /// 日本の塗りつぶし色
                /// </summary>
                public Color Japan { get; set; } = Color.FromArgb(100, 100, 150);

                /// <summary>
                /// 日本の境界線色
                /// </summary>
                public Color Japan_Border { get; set; } = Color.FromArgb(255, 255, 255);
            }

            /// <summary>
            /// 右側部分背景色
            /// </summary>
            public Color InfoBack { get; set; } = Color.FromArgb(30, 60, 90);

            /// <summary>
            /// 右側部分等テキスト色
            /// </summary>
            public Color Text { get; set; } = Color.FromArgb(255, 255, 255);

            /// <summary>
            /// 震央円の透明度
            /// </summary>
            public int Hypo_Alpha { get; set; } = 204;
        }

        static int autoIndex = 240;
        static double fps = V;

        private void AutoExe_Tick(object sender, EventArgs e)
        {

            AutoExe.Interval = (int)(1000 / fps);
            var st = DateTime.Now;
            PicBox.BackgroundImage = EEWMap(autoIndex / fps);
            Console.WriteLine((autoIndex / fps).ToString("0.0000") + "s:" + (DateTime.Now - st).TotalMilliseconds.ToString("0.0000") + "ms!");
            autoIndex++;
        }
    }
}
