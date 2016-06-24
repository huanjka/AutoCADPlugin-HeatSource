using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using HeatSource.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Model
{
    /// <summary>
    /// 区域的实体
    /// 表明热源与热力站/热力站与楼房/热源与楼房的关系
    /// case 1: 热源与热力站 1 对 多 HeatProducers的长度为 1, SubStations的长度大于0， Buildings 的长度为0
    /// case 2: 热源与楼房   1 对 多 HeatProducers的长度为 1, Buildings 的长度大于0， Substations 的长度为0
    /// case 3: 热力站与楼房 1 对 多 Heatproducers的长度为 0， Substations 的长度为1， Buildings的长度大于0
    /// </summary>
    public class District:BaseModel
    {
        public static string modelType = "district";
        public List<Building> Buildings = new List<Building>();
        public List<SubStation> SubStations = new List<SubStation>();
        public List<HeatProducer> HeatProducers = new List<HeatProducer>();
        public Solution OwnSolution { set; get; }
        public District(Solution s):base(true)
        {
            OwnSolution = s;
        }
        public District() : base(false)
        {
        }
        protected override string ModelType()
        {
            return modelType;
        }

        protected override void GetAttributes()
        {
            string str1 = "";
            if(Buildings.Count > 0)
            {
                foreach(var item in Buildings)
                {
                    str1 += ModelIdManager.toString(item.BaseModelId) + " ";
                }
                str1 = str1.Substring(0, str1.Length - 1);
                attrs.Add("buildings", str1);
            }
            if(SubStations.Count > 0)
            {
                string str2 = "";
                foreach(var item in SubStations)
                {
                    str2 += ModelIdManager.toString(item.BaseModelId) + " ";
                }
                str2 = str2.Substring(0, str2.Length - 1);
                attrs.Add("substations", str2);
            }
            if(HeatProducers.Count > 0)
            {
                string str3 = "";
                foreach(var item in HeatProducers)
                {
                    str3 += ModelIdManager.toString(item.BaseModelId) + " ";
                }
                str3 = str3.Substring(0, str3.Length - 1);
                attrs.Add("heatproducers", str3);
            }
            if (this.OwnSolution != null)
            {
                attrs.Add("solution", Utils.ModelIdManager.toString(this.OwnSolution.BaseModelId));
            }
        }

        protected override void _SetAttributes(Dictionary<string, string> _attrs)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            foreach (var item in _attrs)
            {
                attrs[item.Key] = item.Value;
            }
            Buildings.Clear();
            HeatProducers.Clear();
            SubStations.Clear();
            if(attrs.ContainsKey("buildings"))
            {
                string[] strs = attrs["buildings"].Split(' ');
                foreach(var item in strs)
                {
                    int id = ModelIdManager.Parse(item);
                    Buildings.Add((Building)BaseModel.GetModelById(id));
                }
            }
            if (attrs.ContainsKey("substations"))
            {
                string[] strs = attrs["substations"].Split(' ');
                foreach(var item in strs)
                {
                    int id = ModelIdManager.Parse(item);
                    SubStations.Add((SubStation)BaseModel.GetModelById(id));
                }
            }
            if(attrs.ContainsKey("heatproducers"))
            {
                string[] strs = attrs["heatproducers"].Split(' ');
                foreach(var item in strs)
                {
                    int id = ModelIdManager.Parse(item);
                    HeatProducers.Add((HeatProducer)BaseModel.GetModelById(id));
                }
            }
            if (attrs.ContainsKey("solution"))
            {
                this.OwnSolution = (Solution)BaseModel.GetModelById(Utils.ModelIdManager.Parse(attrs["solution"]));
                if (this.OwnSolution != null)
                {
                    this.OwnSolution.Districts.Add(this.BaseObjectId, this);
                }
            }
        }

        public List<Building> GetBuildingsFromCloud()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            var lists = new List<Building>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(BaseObjectId, OpenMode.ForRead);
                
                foreach (var item in HeatSourceLayoutApp.buildings)
                {
                    if (item.Value.isInPolyline((Polyline)obj))
                    {
                        lists.Add(item.Value);
                    }
                }
                tr.Dispose();
            }
            return lists;
        }

        public List<HeatProducer> GetHeatProducersFromCloud()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            var lists = new List<HeatProducer>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(BaseObjectId, OpenMode.ForRead);

                foreach (var item in HeatSourceLayoutApp.currentSolution.HeatProducers)
                {
                    if (item.Value.isInpolyline((Polyline)obj))
                    {
                        lists.Add(item.Value);
                    }
                }
                tr.Dispose();
            }
            return lists;
        }

        public List<SubStation> GetSubStationsFromCloud()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            var lists = new List<SubStation>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(BaseObjectId, OpenMode.ForRead);
                foreach (var item in HeatSourceLayoutApp.currentSolution.SubStations)
                {
                    if (item.Value.isInpolyline((Polyline)obj))
                    {
                        lists.Add(item.Value);
                    }
                }
                tr.Dispose();
            }
            return lists;
        }


        /// <summary>
        /// 检查是否满足约束，如果满足则保存
        /// </summary>
        /// <returns></returns>
        public Error CheckAndSave(int type)
        {
            Buildings = this.GetBuildingsFromCloud();
            SubStations = this.GetSubStationsFromCloud();
            HeatProducers = this.GetHeatProducersFromCloud();
            switch(type)
            {
                case 1:
                    Buildings.Clear();
                    break;
                case 2:
                    SubStations.Clear();
                    break;
                case 3:
                    HeatProducers.Clear();
                    break;
            }
            Error e = new Error();
            foreach(var item in Buildings)
            {
                if(item.HasDistrict(OwnSolution))
                {
                    e.Message = "存在楼房已被圈选";
                    e.Err = Error.ErrorCode.ConflictErr;
                    return e;
                }
            }
            foreach(var item in HeatProducers)
            {
                if(item.OwnMasterDistrict != null && Buildings.Count != 0)
                {
                    e.Message = "存在热源已被圈选且圈选中有新的楼房";
                    e.Err = Error.ErrorCode.ConflictErr;
                    return e;
                }
                if(item.OwnSlaveDistrict != null && SubStations.Count != 0)
                {
                    e.Message = "存在热源已被圈选且圈选中有新的热力站";
                    e.Err = Error.ErrorCode.ConflictErr;
                    return e;
                }
            }
            foreach(var item in SubStations)
            {
                if(item.OwnMasterDistrict != null && Buildings.Count != 0)
                {
                    e.Message = "存在热力站已被圈选且圈选中有新的楼房";
                    e.Err = Error.ErrorCode.ConflictErr;
                    return e;
                }
                if(item.OwnSlaveDistrict != null && HeatProducers.Count != 0)
                {
                    e.Message = "存在热力站已被圈选且圈选中有新的热源";
                    e.Err = Error.ErrorCode.ConflictErr;
                    return e;
                }
            }
            bool pass = false;
            if (Buildings.Count != 0 && SubStations.Count == 1 && HeatProducers.Count == 0 && type == 3)
            {
                pass = true;
                foreach (var item in Buildings)
                {
                    item.AddDistrict(OwnSolution, this);
                }
                foreach (var item in SubStations)
                {
                    item.OwnMasterDistrict = this;
                }
            }
            else if(Buildings.Count != 0 && SubStations.Count == 0 && HeatProducers.Count == 1 && type == 2)
            { 
                pass = true;
                foreach (var item in Buildings)
                {
                    item.AddDistrict(OwnSolution, this);
                }
                foreach (var item in HeatProducers)
                {
                    item.OwnMasterDistrict = this;
                }

            }
            else if(Buildings.Count == 0 && SubStations.Count != 0 && HeatProducers.Count == 1 && type == 1)
            {
                pass = true;
                foreach(var item in SubStations)
                {
                    item.OwnSlaveDistrict = this;
                }
                foreach (var item in HeatProducers)
                {
                    item.OwnSlaveDistrict = this;
                }
            }
            if (pass == true)
            {
                e.Err = Error.ErrorCode.NoErr;
                this.Save();
                return e;
            }
            else
            {
                e.Message = "圈选逻辑错误，请检查是否圈了多个热源或者热力站";
                e.Err = Error.ErrorCode.LogicalErr;
                return e;
            }
        }
        /// <summary>
        /// 仅仅删除类的逻辑
        /// </summary>
        public void Delete()
        {
            foreach (var item in Buildings)
            {
                item.RemoveDistrict(OwnSolution);
            }
            foreach (var item in HeatProducers)
            {
                if(item.OwnMasterDistrict == this)
                {
                    item.OwnMasterDistrict = null;
                }
                if(item.OwnSlaveDistrict == this)
                {
                    item.OwnSlaveDistrict = null;
                }
            }
            foreach (var item in SubStations)
            {
                if(item.OwnMasterDistrict== this)
                {
                    item.OwnMasterDistrict = null;
                }
                if(item.OwnSlaveDistrict == this)
                {
                    item.OwnSlaveDistrict = null;
                }
            }
            this.OwnSolution.Districts.Remove(this.BaseObjectId);
        }

       /// <summary>
       /// 删除类的逻辑和实体
       /// </summary>
        public void RemoveSelf()
        {
            //remove self
            Delete();
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    tr.GetObject(BaseObjectId, OpenMode.ForWrite).Erase();
                    tr.Commit();
                    tr.Dispose();
                }
            }
        }

        public double TotalArea
        {
            get
            {
                if (this.Buildings.Count > 0)
                {
                    double total = 0;
                    foreach (var b in Buildings)
                    {
                        total += b.Area;
                    }
                    return total;
                }
                if(this.SubStations.Count > 0)
                {
                    double total = 0;
                    foreach(var s in SubStations)
                    {
                        total += s.TotalArea;
                    }
                    return total;
                }
                return 0;
            }
        }

       
    }
}
