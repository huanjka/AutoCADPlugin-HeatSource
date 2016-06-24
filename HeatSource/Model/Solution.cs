using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using HeatSource.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeatSource.Formula;

namespace HeatSource.Model
{
    public class Solution : BaseModel
    {
        public static string modelType = "solution";

        private String _solutionname;
        private int _sid;
        public String SolutionName
        {
            set
            {
                _solutionname = value;
                this.Save();
            }
            get
            {
                return _solutionname;
            }
        }
        public int SId
        {
            set
            {
                _sid = value;
                this.Save();
            }
            get
            {
                return _sid;
            }
        }
        public ChromeTabs.ChromeTabItem TabItem { set; get; }
        public Dictionary<ObjectId, HeatProducer> HeatProducers { set; get; }
        public Dictionary<ObjectId, SubStation> SubStations { set; get; }
        public Dictionary<ObjectId, District> Districts { set; get; }
        public Dictionary<ObjectId, PipeLine> PipeLines { set; get; }
        public Dictionary<ObjectId, SimplePipeJoint> SimplePipeJoints { set; get; }
        public Dictionary<ObjectId, MultiPipeJoint> MultiPipeJoints { set; get; }

        public enum PipeLineStyle
        {
            HeatSourceConnectHeatProducer,
            AnyConnectedBuilding
        }

        public PipeLineStyle CurrentLineStyle { set; get; }

        //总运行费用
        public double GetTotalFee()
        {
            double totalFee = 0;
            double waterPumpCostForProducer = GetGasHeatingCost();
            double gasHeatingCostForProducer = GetWaterPumpCost();
            totalFee = waterPumpCostForProducer + gasHeatingCostForProducer;
            return Math.Round(totalFee, 2);
        }
        //全年耗热量
        public double TotalLoad
        {
            get
            {
                double value = 0;
                foreach(var item in HeatProducers.Values)
                {
                    value += item.TotalLoad;
                }
                return value;
            }
        }
        
        //水泵全年运行总能耗
        public double GetTotalWaterPumpEnergyConsumption()
        {
            double waterPumpEnergyConsumptionForProducers = 0;
            foreach (HeatProducer h in HeatProducers.Values)
            {
                //计算方案中热源的水泵总能耗
                waterPumpEnergyConsumptionForProducers += h.totalWaterPumpEnergyConsumption;
            }

            return waterPumpEnergyConsumptionForProducers;
        }

        // 燃气供热运行费用
        public double GetGasHeatingCost()
        {
            double gasHeatingCostForProducer = 0;
            double inTemp = HeatSourceLayoutApp.globalProperty.IndoorTemperature;
            double outTemp = HeatSourceLayoutApp.globalProperty.OutDoorTemp;
            double outAvgTemp = HeatSourceLayoutApp.globalProperty.OutAverageTemp;
            double heatingDays = HeatSourceLayoutApp.globalProperty.HeatingDays;

            foreach (HeatProducer h in HeatProducers.Values)
            {
                //计算燃气供热运行费用
                gasHeatingCostForProducer += new GasHeatingCost(HeatSourceLayoutApp.globalProperty.GasPrice, h.BoilerEfficiency, h.TotalLoad).GetResult();
            }

            return Math.Round(gasHeatingCostForProducer, 2);
        }
        // 水泵运行费用
        public double GetWaterPumpCost()
        {
            double waterPumpCostForProducer = 0;
            foreach (HeatProducer h in HeatProducers.Values)
            {
                waterPumpCostForProducer += new WaterPumpOperationCost(HeatSourceLayoutApp.globalProperty.ElecPrice, h.totalWaterPumpEnergyConsumption).GetResult();
            }

            return Math.Round(waterPumpCostForProducer, 2);
        }
        //总运行费用
        public double GetTotalOperationCost()
        {
            return this.GetGasHeatingCost() + this.GetWaterPumpCost();
        }
        public Solution(bool needId) : base(needId)
        {
            this.HeatProducers = new Dictionary<ObjectId, HeatProducer>();
            this.SubStations = new Dictionary<ObjectId, SubStation>();
            this.Districts = new Dictionary<ObjectId, District>();
            this.PipeLines = new Dictionary<ObjectId, PipeLine>();
            this.SimplePipeJoints = new Dictionary<ObjectId, SimplePipeJoint>();
            this.MultiPipeJoints = new Dictionary<ObjectId, MultiPipeJoint>();
        }

        /// <summary>
        /// 解决方案命名的管理
        /// </summary>
        /// <returns></returns>
        public static String NextDefaultName()
        {
            int max = 0;
            foreach (var item in HeatSource.HeatSourceLayoutApp.solutions)
            {
                if (item.SId > max)
                {
                    max = item.SId;
                }
            }
            return "方案" + HeatSource.Utils.ChineseNum.NumToChinese(max + 1);
        }
        /// <summary>
        /// 解决方案编号的管理
        /// </summary>
        /// <returns></returns>
        public static int NextDefaultSId()
        {
            int max = 0;
            foreach (var item in HeatSource.HeatSourceLayoutApp.solutions)
            {
                if (item.SId > max)
                {
                    max = item.SId;
                }
            }
            return max + 1;

        }

        /// <summary>
        /// 计算以包含某管道的最长管道路径
        /// </summary>
        /// <param name="pipeLine">指定的管道</param>
        /// <param name="maxPipelines">最长管道路径所包含的节点</param>
        /// <returns>最长管道路径的长度</returns>
        public double GetMainPipeLineLength(PipeLine pipeLine, out List<PipeLine> maxPipelines)
        {
            double max_head = 0;
            double max_tail = 0;
            maxPipelines = new List<PipeLine>();
            List<PipeLine> maxLeftPipeLines = new List<PipeLine>();
            List<PipeLine> maxRightPipeLines = new List<PipeLine>();
            List<PipeLine> pipes = new List<PipeLine>();
            foreach (var item in PipeLines)
            {
                item.Value.IsVisit = false;
            }
            pipeLine.IsVisit = true;
            if (pipeLine.HeadConnectedObject != null)
            {
                foreach (var item in pipeLine.HeadConnectedObject.ConnectedPipes)
                {
                    if (item != pipeLine)
                    {
                        double v = GetPipeLineLength(pipeLine, item, out pipes);
                        if (v > max_head)
                        {
                            max_head = v;
                            maxLeftPipeLines = pipes;
                        }
                    }
                }
            }
            if (pipeLine.TailConnectedObject != null)
            {
                foreach (var item in pipeLine.TailConnectedObject.ConnectedPipes)
                {
                    if (item != pipeLine)
                    {
                        double v = GetPipeLineLength(pipeLine, item, out pipes);
                        if (v > max_tail)
                        {
                            max_tail = v;
                            maxRightPipeLines = pipes;
                        }
                    }
                }
            }
            maxPipelines.AddRange(maxLeftPipeLines);
            maxPipelines.Add(pipeLine);
            maxPipelines.AddRange(maxRightPipeLines);

            return max_head + max_tail + pipeLine.CalculateLength();
        }

        /// <summary>
        /// 从node节点出发不经过parent节点的最长路径
        /// </summary>
        /// <param name="parent">node的上一访问节点</param>
        /// <param name="node">node几点</param>
        /// <param name="pipes">从node节点出发不经过parent节点的最长路径的距离</param>
        /// <returns>最长路径的大小</returns>
        public double GetPipeLineLength(PipeLine parent, PipeLine node, out List<PipeLine> pipes)
        {
            pipes = new List<PipeLine>();
            if (node.HeadConnectedObject == null)
            {
                pipes.Add(node);
                return node.CalculateLength();
            }
            if (node.TailConnectedObject == null)
            {
                pipes.Add(node);
                return node.CalculateLength();
            }
            if (node.IsVisit == false)
            {
                node.IsVisit = true;
                ReadOnlyCollection<PipeLine> children = null;
                if (node.HeadConnectedObject.ConnectedPipes.Contains(parent) == true)
                {
                    children = node.TailConnectedObject.ConnectedPipes;
                }
                if (node.TailConnectedObject.ConnectedPipes.Contains(parent) == true)
                {
                    children = node.HeadConnectedObject.ConnectedPipes;
                }
                double max = 0;

                foreach (var item in children)
                {
                    if (item != node)
                    {
                        List<PipeLine> lines;
                        double v = GetPipeLineLength(node, item, out lines);
                        if (v > max)
                        {
                            pipes = lines;
                            max = v;
                        }
                    }
                }
                pipes.Add(node);
                max += node.CalculateLength();
                node.IsVisit = false;
                return max;
            }
            return 0;
        }

        public HeatProducer GetHeatProducer(ObjectId id)
        {
            if (HeatProducers.ContainsKey(id))
            {
                return HeatProducers[id];
            }
            return null;
        }

        public SubStation GetSubStation(ObjectId id)
        {
            if (SubStations.ContainsKey(id))
            {
                return SubStations[id];
            }
            return null;
        }

        /// <summary>
        /// 每一次创建管道的时候初始状态获取点的方式是从界面上选取一点
        /// 后面都是从上一次的尾点开始
        /// </summary>
        /// <returns>空或者尾点坐标，空意味着管道创建结束</returns>
        public Point3d? CreatePipeLineWithoutInitialPoint(out bool status)
        {
            status = false;
            PipeLine newPipeLine = null;
            PipeJig jig = new PipeJig(null);
            if (!jig.StartDraw(out status)) { return null; }
            newPipeLine = new PipeLine((Point3d)jig.startPoint, jig.endPoint, false, this);
            //相交测试
            //和简单接口测试相交
            return this.PipeLineCrossTest(newPipeLine);
        }
        /// <summary>
        /// 从一固定点创建管道
        /// </summary>
        /// <param name="startPoint"></param>
        /// <returns>返回结果如上</returns>
        public Point3d? CreatePipeLineConnectedPipeLine(Point3d startPoint, out bool status)
        {
            PipeLine newPipeline = null;
            PipeJig jig = new PipeJig(startPoint);
            if (!jig.StartDraw(out status)) { return null; }
            newPipeline = new PipeLine((Point3d)jig.startPoint, jig.endPoint, false, this);
            return this.PipeLineCrossTest(newPipeline);
        }



        /// <summary>
        /// 管道相交测试
        /// </summary>
        /// <param name="pipeLine">测试管道</param>
        /// <param name="isFirst">相交测试是一个递归的过程，在一层递归里面PipeLine并没有被实际创建，而是在测试结束后才创建，下面的递归
        /// 层，测试的管道都是已经被创建了的</param>
        /// <returns></returns>
        public Point3d? PipeLineCrossTest(PipeLine pipeLine)
        {
            
            Point3d? res;
            Dictionary<ObjectId, PipeLine> copyPipelines = new Dictionary<ObjectId, PipeLine>();
            foreach (var item in PipeLines)
            {
                copyPipelines.Add(item.Key, item.Value);
            }

            //和管道测试相交
            foreach (var item in copyPipelines)
            {
                PipeLine cItem = (PipeLine)item.Value;
                if (cItem == pipeLine)
                {
                    continue;
                }
                PipeLine.CrossType type = cItem.PipeLineCrossTest(pipeLine);
                if (type == PipeLine.CrossType.CrossNone)
                {
                    continue;
                }
                else if (type == PipeLine.CrossType.ParallelHead)
                {
                    Point3d p1 = pipeLine.Point(0);
                    Point3d p2 = pipeLine.Point(1);
                    Point3d p3;
                    bool result = false;
                    if ((p1 - cItem.Point(0)).Length < (p2 - cItem.Point(0)).Length)
                    {
                        result = false;
                        p3 = p2;
                    }
                    else
                    {
                        result = true;
                        p3 = p1;
                    }
                    Vector3d v = cItem.Point(0) - cItem.Point(1);
                    Vector3d v1 = p3 - cItem.Point(0);
                    Point3d p = cItem.Point(0) + v1.DotProduct(v.GetNormal()) * v.GetNormal();
                    //
                    pipeLine = new PipeLine(p, cItem.Point(1), false, this);
                    pipeLine.TailConnectedObject = cItem.TailConnectedObject;
                    if (pipeLine.TailConnectedObject != null)
                    {
                        pipeLine.TailConnectedObject.RemovePipeLine(cItem);
                        pipeLine.TailConnectedObject.AddPipeLine(pipeLine);
                    }
                    cItem.Delete();
                    //递归调用

                    res = PipeLineCrossTest(pipeLine);
                    if (result)
                    {
                        return null;
                    }
                    else
                    {
                        return res;
                    }
                }
                else if (type == PipeLine.CrossType.ParallelTail)
                {
                    Point3d p1 = pipeLine.Point(0);
                    Point3d p2 = pipeLine.Point(1);
                    Point3d p3;
                    bool result;
                    if ((p1 - cItem.Point(1)).Length < (p2 - cItem.Point(1)).Length)
                    {
                        result = false;
                        p3 = p2;
                    }
                    else
                    {
                        result = true;
                        p3 = p1;
                    }
                    Vector3d v = cItem.Point(1) - cItem.Point(0);
                    Vector3d v1 = p3 - cItem.Point(1);
                    Point3d p = cItem.Point(1) + v1.DotProduct(v.GetNormal()) * v.GetNormal();
                    //
                    pipeLine = new PipeLine(cItem.Point(0), p, false, this);
                    pipeLine.HeadConnectedObject = cItem.HeadConnectedObject;
                    if (pipeLine.HeadConnectedObject != null)
                    {
                        pipeLine.HeadConnectedObject.RemovePipeLine(cItem);
                        pipeLine.HeadConnectedObject.AddPipeLine(pipeLine);
                    }
                    cItem.Delete();
                    //递归调用
                    res = PipeLineCrossTest(pipeLine);
                    if (result)
                    {
                        return null;
                    }
                    else
                    {
                        return res;
                    }
                }
                else if (type == PipeLine.CrossType.NonParallelHead)
                {
                    Point3d p1 = pipeLine.Point(0);
                    Point3d p2 = pipeLine.Point(1);
                    Point3d p3;
                    bool result;
                    if ((p1 - cItem.Point(0)).Length < (p2 - cItem.Point(0)).Length)
                    {
                        result = false;
                        p3 = p2;
                    }
                    else
                    {
                        result = true;
                        p3 = p1;
                    }
                    SimplePipeJoint simplePipeJoint = new SimplePipeJoint(cItem.Point(1), cItem.Point(0), p3, PipeLine.GetScale() / 2, this, true);
                    cItem.UpdateStartPoint(simplePipeJoint.CalculateLastMlineEndPoint());
                    Point3d startPoint = simplePipeJoint.CalculateMlineStartPoint();
                    simplePipeJoint.AddPipeLine(cItem);
                    PipeLine newLine = new PipeLine(p3, startPoint, false, this);
                    simplePipeJoint.AddPipeLine(newLine);
                    cItem.HeadConnectedObject = simplePipeJoint;
                    newLine.TailConnectedObject = simplePipeJoint;
                    if (pipeLine.HeadConnectedObject != null)
                    {
                        pipeLine.HeadConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.HeadConnectedObject.AddPipeLine(newLine);
                        newLine.HeadConnectedObject = pipeLine.HeadConnectedObject;
                    }
                    //递归调用
                    res = PipeLineCrossTest(newLine);
                    if (result)
                    {
                        return null;
                    }
                    else
                    {
                        return res;
                    }
                }
                else if (type == PipeLine.CrossType.NonParallelTail)
                {
                    Point3d p1 = pipeLine.Point(0);
                    Point3d p2 = pipeLine.Point(1);
                    Point3d p3;
                    bool result;
                    if ((p1 - cItem.Point(1)).Length < (p2 - cItem.Point(1)).Length)
                    {
                        result = false;
                        p3 = p2;
                    }
                    else
                    {
                        result = true;
                        p3 = p1;
                    }
                    SimplePipeJoint simplePipeJoint = new SimplePipeJoint(cItem.Point(0), cItem.Point(1), p3, PipeLine.GetScale() / 2, this, true);
                    cItem.UpdateEndPoint(simplePipeJoint.CalculateLastMlineEndPoint());
                    cItem.TailConnectedObject = simplePipeJoint;
                    Point3d startPoint = simplePipeJoint.CalculateMlineStartPoint();
                    simplePipeJoint.AddPipeLine(cItem);
                    PipeLine newLine = new PipeLine(startPoint, p3, false, this);
                    simplePipeJoint.AddPipeLine(newLine);
                    newLine.HeadConnectedObject = simplePipeJoint;
                    if (pipeLine.TailConnectedObject != null)
                    {
                        pipeLine.TailConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.TailConnectedObject.AddPipeLine(newLine);
                        newLine.TailConnectedObject = pipeLine.TailConnectedObject;
                    }
                    res = PipeLineCrossTest(newLine);
                    if (result)
                    {
                        return null;
                    }
                    else
                    {
                        return res;
                    }
                }
                else if (type == PipeLine.CrossType.CrossHead)
                {
                    MultiPipeJoint mj = new MultiPipeJoint(this, true);
                    List<Point3d> ps = mj.GenerateFromCrossPipe(cItem, pipeLine.Point(0), pipeLine.Point(1), PipeLine.GetScale() / 2);
                    PipeLine p1 = new PipeLine(cItem.Point(0), ps[0], this, true);
                    PipeLine p2 = new PipeLine(ps[1], cItem.Point(1), this, true);
                    //
                    if (cItem.HeadConnectedObject != null)
                    {
                        cItem.HeadConnectedObject.RemovePipeLine(cItem);
                        cItem.HeadConnectedObject.AddPipeLine(p1);
                    }
                    if (cItem.TailConnectedObject != null)
                    {
                        cItem.TailConnectedObject.RemovePipeLine(cItem);
                        cItem.TailConnectedObject.AddPipeLine(p2);

                    }
                    p1.HeadConnectedObject = cItem.HeadConnectedObject;
                    p1.TailConnectedObject = mj;
                    p2.HeadConnectedObject = mj;
                    p2.TailConnectedObject = cItem.TailConnectedObject;

                    Point3d startPoint = ps[2];
                    cItem.Delete();

                    PipeLine pLine = new PipeLine(startPoint, pipeLine.Point(1), false, this);
                    pLine.HeadConnectedObject = mj;
                    if (pipeLine.TailConnectedObject != null)
                    {
                        pipeLine.TailConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.TailConnectedObject.AddPipeLine(pLine);
                        pLine.TailConnectedObject = pipeLine.TailConnectedObject;
                    }
                    mj.AddPipeLine(p1);
                    mj.AddPipeLine(p2);
                    mj.AddPipeLine(pLine);

                    res = PipeLineCrossTest(pLine);
                    return res;
                }
                else if (type == PipeLine.CrossType.CrossTail)
                {
                    //
                    MultiPipeJoint mj = new MultiPipeJoint(this, true);
                    List<Point3d> ps = mj.GenerateFromCrossPipe(cItem, pipeLine.Point(1), pipeLine.Point(0), PipeLine.GetScale() / 2);
                    PipeLine p1 = new PipeLine(cItem.Point(0), ps[0], this, true);
                    PipeLine p2 = new PipeLine(ps[1], cItem.Point(1), this, true);
                    //
                    if (cItem.HeadConnectedObject != null)
                    {
                        cItem.HeadConnectedObject.RemovePipeLine(cItem);
                        cItem.HeadConnectedObject.AddPipeLine(p1);
                    }
                    if (cItem.TailConnectedObject != null)
                    {
                        cItem.TailConnectedObject.RemovePipeLine(cItem);
                        cItem.TailConnectedObject.AddPipeLine(p2);
                    }
                    p1.HeadConnectedObject = cItem.HeadConnectedObject;
                    p1.TailConnectedObject = mj;
                    p2.HeadConnectedObject = mj;
                    p2.TailConnectedObject = cItem.TailConnectedObject;
                    Point3d startPoint = ps[2];
                    cItem.Delete();
                    PipeLine pLine = new PipeLine(pipeLine.Point(0), startPoint, false, this);
                    pLine.TailConnectedObject = mj;
                    if (pipeLine.HeadConnectedObject != null)
                    {
                        pipeLine.HeadConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.HeadConnectedObject.AddPipeLine(pLine);
                        pLine.HeadConnectedObject = pipeLine.HeadConnectedObject;
                    }
                    mj.AddPipeLine(p1);
                    mj.AddPipeLine(p2);
                    mj.AddPipeLine(pLine);
                    PipeLineCrossTest(pLine);
                    return mj.CenterPoint;
                }
                else if (type == PipeLine.CrossType.CrossOver)
                {
                    MultiPipeJoint mj = new MultiPipeJoint(this, true);
                    List<Point3d> ps = mj.GenerateFromCrossPipe(cItem, pipeLine.Point(1), pipeLine.Point(0), PipeLine.GetScale() / 2);
                    PipeLine p1 = new PipeLine(cItem.Point(0), ps[0], this, true);
                    PipeLine p2 = new PipeLine(ps[1], cItem.Point(1), this, true);
                    if (cItem.HeadConnectedObject != null)
                    {
                        cItem.HeadConnectedObject.RemovePipeLine(cItem);
                        cItem.HeadConnectedObject.AddPipeLine(p1);
                    }
                    if (cItem.TailConnectedObject != null)
                    {
                        cItem.TailConnectedObject.RemovePipeLine(cItem);
                        cItem.TailConnectedObject.AddPipeLine(p2);
                    }

                    p1.HeadConnectedObject = cItem.HeadConnectedObject;
                    p1.TailConnectedObject = mj;
                    p2.HeadConnectedObject = mj;
                    p2.TailConnectedObject = cItem.TailConnectedObject;

                    Point3d startPoint = ps[2];
                    cItem.Delete();

                    PipeLine pLine = new PipeLine(pipeLine.Point(0), startPoint, false, this);
                    pLine.TailConnectedObject = mj;
                    if (pipeLine.HeadConnectedObject != null)
                    {
                        pipeLine.HeadConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.HeadConnectedObject.AddPipeLine(pLine);
                        pLine.HeadConnectedObject = pipeLine.HeadConnectedObject;
                    }
                    mj.AddPipeLine(p1);
                    mj.AddPipeLine(p2);
                    mj.AddPipeLine(pLine);
                    PipeLineCrossTest(pLine);
                    //插入新的点
                    PipeLine se = mj.UpdateMultiJoint(pipeLine.Point(1));
                    se.HeadConnectedObject = mj;
                    if (pipeLine.TailConnectedObject != null)
                    {
                        pipeLine.TailConnectedObject.RemovePipeLine(pipeLine);
                        pipeLine.TailConnectedObject.AddPipeLine(se);
                        se.TailConnectedObject = pipeLine.TailConnectedObject;
                    }
                    res = PipeLineCrossTest(se);
                    return res;
                }

            }
            foreach (var item in SimplePipeJoints)
            {
                SimplePipeJoint pItem = (SimplePipeJoint)item.Value;
                if (pItem.PointInJoint(pipeLine.Point(0)) && !pItem.ConnectedPipes.Contains(pipeLine))
                {
                    MultiPipeJoint mpj = new MultiPipeJoint(this, true);
                    PipeLine ps = mpj.GenerateFromSimpleJoint(pItem, pipeLine.Point(1), PipeLine.GetScale() / 2);
                    ps.HeadConnectedObject = mpj;
                    res = PipeLineCrossTest(ps);
                    return res;
                }
                if (pItem.PointInJoint(pipeLine.Point(1)) && !pItem.ConnectedPipes.Contains(pipeLine))
                {
                    MultiPipeJoint mpj = new MultiPipeJoint(this, true);
                    PipeLine ps = mpj.GenerateFromSimpleJoint(pItem, pipeLine.Point(0), PipeLine.GetScale() / 2);
                    ps.TailConnectedObject = mpj;
                    res = PipeLineCrossTest(ps);
                    return null;
                }
            }
            foreach (var item in MultiPipeJoints)
            {
                MultiPipeJoint pItem = (MultiPipeJoint)item.Value;
                if (pItem.PointInJoint(pipeLine.Point(0)) && !pItem.ConnectedPipes.Contains(pipeLine))
                {
                    PipeLine se = pItem.UpdateMultiJoint(pipeLine.Point(1));
                    se.HeadConnectedObject = pItem;
                    res = PipeLineCrossTest(se);
                    return res;
                }
                if (pItem.PointInJoint(pipeLine.Point(1)) && !pItem.ConnectedPipes.Contains(pipeLine))
                {
                    PipeLine se = pItem.UpdateMultiJoint(pipeLine.Point(0));
                    se.HeadConnectedObject = pItem;
                    res = PipeLineCrossTest(se);
                    return null;
                }
            }
            pipeLine.SaveEntity();
            return pipeLine.Point(1);
        }

        /// <summary>
        /// 创建管道
        /// </summary>
        public void CreatePipeLine(out bool status)
        {
            status = false;
            Point3d? endPoint = this.CreatePipeLineWithoutInitialPoint(out status);
            if (endPoint == null)
            {
                return;
            }
            Point3d startPoint = (Point3d)endPoint;
            while (true)
            {
                Point3d? newLine = this.CreatePipeLineConnectedPipeLine(startPoint, out status);
                if (newLine == null)
                {
                    break;
                }
                else
                {
                    startPoint = (Point3d)newLine;
                }
            }
        }

      
        /// <summary>
        /// 将要删除复杂管道结头
        /// </summary>
        /// <param name="id"></param>
        /// <returns>返回是否查询到id</returns>
        public bool WillDeleteMultiJoints(ObjectId id)
        {
            Dictionary<ObjectId, MultiPipeJoint> copyJoints = new Dictionary<ObjectId, MultiPipeJoint>();
            foreach (var item in this.MultiPipeJoints)
            {
                copyJoints.Add(item.Key, item.Value);
            }
            foreach (var item in this.MultiPipeJoints)
            {
                MultiPipeJoint jo = item.Value;

                if (jo.IsMultiJointPart(id))
                {
                    foreach (var pipeline in jo.ConnectedPipes)
                    {
                        if (pipeline.HeadConnectedObject == jo)
                        {
                            pipeline.HeadConnectedObject = null;
                        }
                        if (pipeline.TailConnectedObject == jo)
                        {
                            pipeline.TailConnectedObject = null;
                        }
                    }
                    this.MultiPipeJoints.Remove(item.Key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 将要删除简单管道结头
        /// </summary>
        /// <param name="id"></param>
        /// <returns>同上</returns>
        public bool WillDeleteSimpleJoints(ObjectId id)
        {
            //先进行深拷贝
            Dictionary<ObjectId, SimplePipeJoint> copyJoints = new Dictionary<ObjectId, SimplePipeJoint>();
            foreach (var item in this.SimplePipeJoints)
            {
                copyJoints.Add(item.Key, item.Value);
            }
            foreach (var item in copyJoints)
            {
                SimplePipeJoint jo = item.Value;
                if (jo.IsSimpleJointPart(id))
                {
                    foreach (var pipeline in jo.ConnectedPipes)
                    {
                        if (pipeline.HeadConnectedObject == jo)
                        {
                            pipeline.HeadConnectedObject = null;
                        }
                        if (pipeline.TailConnectedObject == jo)
                        {
                            pipeline.TailConnectedObject = null;
                        }
                    }
                    this.SimplePipeJoints.Remove(item.Key);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 将要删除管道
        /// 删除管道的时候要对其相邻的结头进行处理
        /// </summary>
        /// <param name="id"></param>
        /// <returns>同上</returns>
        public bool WillDeletePipeLine(ObjectId id)
        {
            if (this.PipeLines.ContainsKey(id))
            {
                //
                
                PipeLine pLine = this.PipeLines[id];
                this.CurrentLineStyle = pLine.Style;
                this.PipeLines.Remove(id);
                if (pLine.HeadConnectedObject is SimplePipeJoint)
                {
                    SimplePipeJoint jo = (SimplePipeJoint)pLine.HeadConnectedObject;
                    foreach (var item in jo.ConnectedPipes)
                    {
                        item.RemoveJoint(jo);
                    }
                    jo.RemoveEntity();
                }
                else if (pLine.HeadConnectedObject is MultiPipeJoint)
                {
                    MultiPipeJoint jo = (MultiPipeJoint)pLine.HeadConnectedObject;
                    jo.DownGrade(pLine);
                }
                if (pLine.TailConnectedObject is SimplePipeJoint)
                {
                    SimplePipeJoint jo = (SimplePipeJoint)pLine.TailConnectedObject;
                    foreach (var item in jo.ConnectedPipes)
                    {
                        item.RemoveJoint(jo);
                    }
                    jo.RemoveEntity();
                }
                else if (pLine.TailConnectedObject is MultiPipeJoint)
                {
                    MultiPipeJoint jo = (MultiPipeJoint)pLine.TailConnectedObject;
                    jo.DownGrade(pLine);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将要删除解决方案内的元素
        /// 包括管道、管道结头、热源、热力站
        /// </summary>
        /// <param name="id"></param>
        public void TryDeleteObjectInSolution(ObjectId id)
        {
            if (HeatProducers.ContainsKey(id))
            {
                HeatProducers.Remove(id);
                return;
            }
            if (SubStations.ContainsKey(id))
            {
                SubStations.Remove(id);
                return;
            }
            if (!WillDeletePipeLine(id))
            {
                if (!WillDeleteSimpleJoints(id))
                {
                    WillDeleteMultiJoints(id);
                }
            }
        }
        public void CancelHighLightPipeLine()
        {
            //先要浅拷贝一下所有的管道
            Dictionary<ObjectId, PipeLine> pipes = new Dictionary<ObjectId, PipeLine>();
            foreach (var item in PipeLines)
            {
                pipes.Add(item.Key, item.Value);
            }
            foreach (var item in pipes)
            {
                item.Value.CancelHighlight();
            }
        }

        public void HighLight(BaseModel model)
        {
            List<PipeLine> max_pipes;
            double maxl = GetMaxPipeLengthOfModel(model, out max_pipes);
            foreach (var item in max_pipes)
            {
                item.Highlight();
            }

        }
        public double GetTotalPipeLength()
        {
            double len = 0;
            foreach (var item in PipeLines)
            {
                len += item.Value.CalculateLength();
            }
            return len;
        }
        //主管道长度
        public double GetMaxPipeLengthOfModel(BaseModel model, out List<PipeLine> lines)
        {

            if (model is SubStation || model is HeatProducer)
            {
                List<PipeLine> connectedpipes = SearchConnected(model);
                List<PipeLine> pipes = new List<PipeLine>();
                foreach (var item in connectedpipes)
                {
                    if(model is HeatProducer)
                    {
                        pipes.Add(item);
                    }
                    else if(model is SubStation)
                    {
                        if(item.Style == PipeLineStyle.AnyConnectedBuilding)
                        {
                            pipes.Add(item);
                        }
                    }
                }
                
                double maxium = 0;
                List<PipeLine> max_pipes = new List<PipeLine>();
                foreach (var item in pipes)
                {
                    List<PipeLine> item_maxlines;
                    double v = GetMainPipeLineLength(item, out item_maxlines);
                    if (v > maxium)
                    {
                        max_pipes = item_maxlines;
                        maxium = v;
                    }
                }

                lines = max_pipes;
                return maxium;
            }
            lines = new List<PipeLine>();
            return 0;
        }
        //管道总长度
        public double GetTotalPipeLengthOfModel(BaseModel model, out List<PipeLine> lines)
        {
            if (model is SubStation || model is HeatProducer)
            {
                List<PipeLine> connectedpipes = SearchConnected(model);
                List<PipeLine> pipes = new List<PipeLine>();
                foreach (var item in connectedpipes)
                {
                    if (model is HeatProducer)
                    {
                        pipes.Add(item);
                    }
                    else if (model is SubStation)
                    {
                        if (item.Style == PipeLineStyle.AnyConnectedBuilding)
                        {
                            pipes.Add(item);
                        }
                    }
                }
                double total = 0;
                Stack<PipeLine> st = new Stack<PipeLine>();
                foreach (var item in pipes)
                {
                    st.Push(item);
                }
                Dictionary<ObjectId, PipeLine> allLines = new Dictionary<ObjectId, PipeLine>();
                while (st.Count != 0)
                {
                    PipeLine pl = st.Pop();
                    if (!allLines.ContainsKey(pl.BaseObjectId))
                    {
                        allLines.Add(pl.BaseObjectId, pl);
                        if (pl.HeadConnectedObject != null)
                        {
                            foreach (var item in pl.HeadConnectedObject.ConnectedPipes)
                            {
                                if (!allLines.ContainsKey(item.BaseObjectId))
                                {
                                    st.Push(item);
                                }
                            }
                        }
                        if (pl.TailConnectedObject != null)
                        {
                            foreach (var item in pl.TailConnectedObject.ConnectedPipes)
                            {
                                if (!allLines.ContainsKey(item.BaseObjectId))
                                {
                                    st.Push(item);
                                }
                            }
                        }
                    }
                }
                List<PipeLine> ConnectedLines = new List<PipeLine>();
                foreach (var item in allLines)
                {
                    ConnectedLines.Add(item.Value);
                    total += item.Value.CalculateLength();
                }
                lines = ConnectedLines;
                //
                if (model is HeatProducer)
                {
                    HeatProducer heatProducer = (HeatProducer)model;
                    if (heatProducer.OwnSlaveDistrict != null)
                    {
                        foreach (var item in heatProducer.OwnSlaveDistrict.SubStations)
                        {
                            List<PipeLine> ls;
                            total += item.OwnSolution.GetTotalPipeLengthOfModel(item, out ls);
                            lines.AddRange(ls);
                        }
                    }
                }
                return total;
            }
            lines = new List<PipeLine>();
            return 0;
        }
        public List<PipeLine> SearchConnected(BaseModel model)
        {
            List<PipeLine> pipes = new List<PipeLine>();
            if (!model.BaseObjectId.IsNull)
            {
                try
                {
                    Database db = HostApplicationServices.WorkingDatabase;
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        DBObject obj = tr.GetObject(model.BaseObjectId, OpenMode.ForRead);
                        BlockReference block = (BlockReference)obj;

                        Extents3d bd = block.GeometryExtentsBestFit();
                        foreach (var item in PipeLines)
                        {
                            Point3d p1 = item.Value.Point(0);
                            Point3d p2 = item.Value.Point(1);
                            if (p1.X >= bd.MinPoint.X - 0.001 && p1.Y >= bd.MinPoint.Y - 0.001 && p1.X <= bd.MaxPoint.X + 0.001 && p1.Y <= bd.MaxPoint.Y + 0.001)
                            {
                                pipes.Add(item.Value);
                                continue;
                            }
                            if (p2.X >= bd.MinPoint.X - 0.001 && p2.Y >= bd.MinPoint.Y - 0.001 && p2.X <= bd.MaxPoint.X + 0.001 && p2.Y <= bd.MaxPoint.Y + 0.001)
                            {
                                pipes.Add(item.Value);
                                continue;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(e.Message);
                }
            }
            return pipes;
        }
        protected override string ModelType()
        {
            return Solution.modelType;
        }

        protected override void GetAttributes()
        {
            //
            this.attrs.Clear();
            attrs.Add("name", this.SolutionName);
            attrs.Add("sid", this.SId.ToString());
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            if (attrs.ContainsKey("name"))
            {
                SolutionName = attrs["name"];
            }
            if (attrs.ContainsKey("sid"))
            {
                string v = attrs["sid"];
                SId = int.Parse(v);
            }
        }

        //汇总数据
        public double TotalArea
        {
            get
            {
                double total = 0;
                foreach (var h in HeatProducers)
                {
                    total += h.Value.TotalArea;
                }
                return total;
            }
        }

        public double GetBoildersCost()
        {
            double cost = 0;
            foreach (var item in HeatProducers)
            {
                cost += item.Value.GetBoildersCost();
            }
            return cost;
        }
        //计算换热器造价
        public double GetHeatExchangerCost()
        {
            double cost = 0;
            foreach (var item in HeatProducers)
            {
                cost += item.Value.GetHeatExchangerCost();
            }
            return cost;
        }

        //计算管道造价
        public double GetPipeLineCost()
        {
            double cost = 0;
            foreach (var item in HeatProducers)
            {
                cost += item.Value.GetPipeLineCost();
            }
            return cost;
        }

        public double GetWaterPumpDeviceCost()
        {
            double cost = 0;
            foreach (var item in HeatProducers)
            {
                cost += item.Value.GetWaterPumpCost();
            }
            return cost;
        }
        //总造价
        public double GetTotalCost()
        {
            double cost = 0;
            foreach (var item in HeatProducers)
            {
                cost += item.Value.GetTotalCost();
            }
            return cost;
        }

        public void RemoveSelf()
        {
            HeatSourceLayoutApp.solutions.Remove(this);
            foreach (var item in new Dictionary<ObjectId, District>(Districts))
            {
                item.Value.RemoveSelf();
            }
            foreach (var item in new Dictionary<ObjectId, HeatProducer>(HeatProducers))
            {
                item.Value.RemoveSelf();
            }
            foreach(var item in new Dictionary<ObjectId, SubStation>(SubStations))
            {
                item.Value.RemoveSelf();
            }
            foreach(var item in new Dictionary<ObjectId, SimplePipeJoint>(SimplePipeJoints))
            {
                item.Value.RemoveEntity();
            }
            foreach(var item in new Dictionary<ObjectId, MultiPipeJoint>(MultiPipeJoints))
            {
                item.Value.RemoveEntity();
            }
            foreach(var item in new Dictionary<ObjectId, PipeLine>(PipeLines))
            {
                item.Value.Delete();
            }
        }


    }
}
