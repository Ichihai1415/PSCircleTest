using static PSCircleTest.TimeTable.TJMA2001.Raw_ForLine;

namespace PSCircleTest
{
    public class TimeTable
    {
        public static TJMA2001.Raw_ForLine rawDatas = new();
        static public void GetTT()
        {
            var path = @"D:\Ichihai1415\data\jma\tjma2001\tjma2001";
            var data = File.ReadAllLines(path);
            rawDatas = new TJMA2001.Raw_ForLine();
            foreach (var line in data)
            {
                var parts = line.Split([' ', 'P', 'S'], StringSplitOptions.RemoveEmptyEntries);
                var pTime = double.Parse(parts[0]);
                var sTime = double.Parse(parts[1]);
                var depth = int.Parse(parts[2]);
                var dist = int.Parse(parts[3]);
                /*//TJMA2001.Rawのほう
                var newPSTime = new TJMA2001.Raw.PSTime
                {
                    PTime = pTime,
                    STime = sTime
                };
                if (!rawDatas.Dists.TryGetValue(depth, out Dictionary<int, TJMA2001.Raw.PSTime>? dists))
                    rawDatas.Dists.Add(depth, new Dictionary<int, TJMA2001.Raw.PSTime>
                    {
                        { dist, newPSTime }
                    });
                else
                    dists.Add(dist, newPSTime);
                */
                var newDistPSTime = new List<DistPSTime>
                {
                    new DistPSTime
                    {
                        Dist = dist,
                        PTime = pTime,
                        STime = sTime
                    }
                };
                if (!rawDatas.Dists.TryGetValue(depth, out List<DistPSTime>? dists))
                    rawDatas.Dists.Add(depth, newDistPSTime);
                else
                    dists.AddRange(newDistPSTime);
            }


            //throw new Exception();
        }


        public static (double PDist, double SDist) GetDistLinear(double depth, double sec)//参考:https://zenn.dev/boocsan/articles/travel-time-table-converter-adcal2020
        {

            if (depth > 705)
                return (double.NaN, double.NaN);


            var depthList = rawDatas.Dists.Keys.ToList();
            depthList.Add(-1000);//近い値を求めるため
            depthList.Add(1000);
            depthList.Sort();//-1000,0,...,700,1000

            var cDepth = -1;

            for (int i = 1; i < depthList.Count; i++)
                if (depthList[i] >= depth)//初めて depth以上
                {
                    var depth_nearL_diff = depth - depthList[i - 1];//depth以上で最小のものとの差
                    var depth_nearH_diff = depthList[i] - depth;    //depth以下で最大のものとの差
                    cDepth = depth_nearL_diff > depth_nearH_diff ? depthList[i] : depthList[i - 1];//Hのほうが近ければHを参照
                    break;
                }


            var distPSTime = rawDatas.Dists[cDepth];//深さに対応する震央距離とPS時間

            var pDist = double.NaN;
            for (int i = 1; i < distPSTime.Count; i++)
                if (distPSTime[i].PTime >= sec)
                {
                    pDist = (sec - distPSTime[i - 1].PTime) / (distPSTime[i].PTime - distPSTime[i - 1].PTime) * (distPSTime[i].Dist - distPSTime[i - 1].Dist) + distPSTime[i - 1].Dist;
                    break;
                }

            var sDist = double.NaN;
            for (int i = 1; i < distPSTime.Count; i++)
                if (distPSTime[i].STime >= sec)
                {
                    sDist = (sec - distPSTime[i - 1].STime) / (distPSTime[i].STime - distPSTime[i - 1].STime) * (distPSTime[i].Dist - distPSTime[i - 1].Dist) + distPSTime[i - 1].Dist;
                    break;
                }

            //Console.WriteLine($"d:{depth} s:{sec}  pd:{pDist} sd:{sDist}   * d<0 => d=0");

            if (pDist < 0)
                pDist = 0;
            if (sDist < 0)
                sDist = 0;
            return (pDist, sDist);
        }

        public class TJMA2001
        {
            public class Raw
            {

                public Dictionary<int, Dictionary<int, PSTime>> Dists { get; set; } = [];//<depth,<dist,[Ptime,Stime]>>

                public class PSTime
                {
                    public double PTime { get; set; }
                    public double STime { get; set; }
                }
            }

            public class Raw_ForLine//線形補間用
            {

                public Dictionary<int, List<DistPSTime>> Dists { get; set; } = [];//<depth,<[dist,Ptime,Stime]>>

                public class DistPSTime
                {
                    public int Dist { get; set; }
                    public double PTime { get; set; }
                    public double STime { get; set; }
                }
            }
        }
    }
}