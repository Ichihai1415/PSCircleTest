using PSCircleTest.Properties;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.Json.Nodes;

namespace PSCircleTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BackgroundImage = Draw_Map();
        }

        public static Config_Color color = new();

        public static Config_Map config = new();


        public static Bitmap Draw_Map()
        {

            var centerLat = 25d;
            var centerLon = 125d;

            var a = 255;
            var cor = 1000 / 255d;

            var deltaD = 0.1d;//360/deltaD+1�̓_���v�Z���܂� 
            var newDist = 1000 * 2000d;


            var mapImg = new Bitmap(config.MapSize, config.MapSize);
            var zoomW = config.MapSize / (config.LonEnd - config.LonSta);
            var zoomH = config.MapSize / (config.LatEnd - config.LatSta);
            var mapjson = JsonNode.Parse(Resources.AreaForecastLocalE_GIS_20190125_01);
            var g = Graphics.FromImage(mapImg);
            g.Clear(color.Map.Sea);


            var stTime = DateTime.Now;
            Console.WriteLine($"�����`��J�n {DateTime.Now:HH:mm:ss.ffff}");
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
                    var level = dist / cor;//�F�i�K
                    var co = level > 765 ? Color.FromArgb(a, 128, 128, 128) : level > 510 ? Color.FromArgb(a, 0, 0, (int)level - 510) : level > 255 ? Color.FromArgb(a, 0, (int)level - 255, 0) : Color.FromArgb(a, (int)level, 0, 0);
                    g.DrawLine(new Pen(new SolidBrush(co)), x, y, x + 1, y);

                }//255*cor km����

                var remain = (DateTime.Now - stTime) / (x + 1) * (config.MapSize - x - 1);
                Console.Write($"\r�����`��:[{x + 1}/{config.MapSize}]  �c��{remain:mm\\:ss\\.ffff}");
            }
            Console.WriteLine($"\n�����`�抮�� {DateTime.Now:HH:mm:ss.ffff} ({(DateTime.Now - stTime):mm\\:ss\\.ffff})");

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
            Console.WriteLine("�n�}�`��I��");


            var psApprox = new List<Point>();
            for (double d = 0; d <= 360; d += deltaD)
            {
                var pt = Vincenty.VincentyDirect(centerLat, centerLon, d, newDist, 1);
                psApprox.Add(new Point((int)((pt.Value.lon - config.LonSta) * zoomW), (int)((config.LatEnd - pt.Value.lat) * zoomH)));
            }

            g.DrawPolygon(new Pen(Color.FromArgb(128, 255, 255, 255), 2), psApprox.ToArray());
            Console.WriteLine("�V�~�`��I��");
            //throw new Exception();


            var path = $"output\\{DateTime.Now:yyyyMMddHHmmss}.png";
            Directory.CreateDirectory("output");
            mapImg.Save(path, ImageFormat.Png);
            Console.WriteLine(Path.GetFullPath(path) + " �ɕۑ����܂���");

            g.Dispose();
            return mapImg;
        }

        public class Config_Map
        {
            /// <summary>
            /// �摜�̍���
            /// </summary>
            public int MapSize { get; set; } = 1080;

            /// <summary>
            /// �ܓx�̎n�_
            /// </summary>
            public double LatSta { get; set; } = 20;

            /// <summary>
            /// �ܓx�̏I�_
            /// </summary>
            public double LatEnd { get; set; } = 50;

            /// <summary>
            /// �o�x�̎n�_
            /// </summary>
            public double LonSta { get; set; } = 120;

            /// <summary>
            /// �o�x�̏I�_
            /// </summary>
            public double LonEnd { get; set; } = 150;
        }
        public class Config_Color
        {
            /// <summary>
            /// �n�}�̐F
            /// </summary>
            public MapColor Map { get; set; } = new MapColor();

            /// <summary>
            /// �n�}�̐F
            /// </summary>
            public class MapColor
            {
                /// <summary>
                /// �C�m�̓h��Ԃ��F
                /// </summary>
                public Color Sea { get; set; } = Color.FromArgb(0, 0, 0);

                /// <summary>
                /// ���E(���{����)�̓h��Ԃ��F
                /// </summary>
                public Color World { get; set; } = Color.FromArgb(100, 100, 150);
                /*
                /// <summary>
                /// ���E(���{����)�̋��E���F
                /// </summary>
                public Color World_Border { get; set; }
                */
                /// <summary>
                /// ���{�̓h��Ԃ��F
                /// </summary>
                public Color Japan { get; set; } = Color.FromArgb(0, 0, 0, 0);

                /// <summary>
                /// ���{�̋��E���F
                /// </summary>
                public Color Japan_Border { get; set; } = Color.FromArgb(255, 255, 255);
            }

            /// <summary>
            /// �E�������w�i�F
            /// </summary>
            public Color InfoBack { get; set; } = Color.FromArgb(30, 60, 90);

            /// <summary>
            /// �E���������e�L�X�g�F
            /// </summary>
            public Color Text { get; set; } = Color.FromArgb(255, 255, 255);

            /// <summary>
            /// �k���~�̓����x
            /// </summary>
            public int Hypo_Alpha { get; set; } = 204;
        }
    }
}
