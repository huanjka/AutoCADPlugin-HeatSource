using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace HeatSource.Model
{
    /// <summary>
    /// BaseModel 是所有Model的基类
    /// 每一个model 都具有baseobjectId 属性和basemodelId属性
    /// baseobjectid 是其在autocad中对象的引用
    /// basemodelid 是该对象的引用
    /// basemodelid 唯一的表示了该元素， baseobjectid 可能在运行中发生变化
    /// </summary>
    public abstract class BaseModel
    {
        public Autodesk.AutoCAD.DatabaseServices.ObjectId BaseObjectId { set; get; }
        

        public int BaseModelId { set; get; }

        public static Dictionary<int, BaseModel> ApplicationBaseModels = new Dictionary<int, BaseModel>();

        protected Dictionary<String, String> attrs = new Dictionary<string,string>();

        private bool WriteLock = false; //禁止在SetAttributes 的时候调用set 属性中的Save函数
        protected abstract String ModelType();
        protected abstract void GetAttributes();
        protected abstract void _SetAttributes(Dictionary<String, String> attrs);

        protected BaseModel(bool needId)
        {
            if (needId)
            {
                this.BaseModelId = Utils.ModelIdManager.nextModelId();
                if (ApplicationBaseModels.ContainsKey(this.BaseModelId))
                {
                    ApplicationBaseModels[this.BaseModelId] = this;
                }
                else
                {
                    ApplicationBaseModels.Add(this.BaseModelId, this);
                }
            }
        }

        private void SetAttributes(Dictionary<String, String> attrs, ObjectId objId)
        {
            WriteLock = true;
            if (attrs.ContainsKey("modelid"))
            {
                this.BaseModelId = Utils.ModelIdManager.Parse(attrs["modelid"]);
                if (ApplicationBaseModels.ContainsKey(this.BaseModelId))
                {
                    ApplicationBaseModels[this.BaseModelId] = this;
                }
                else
                {
                    ApplicationBaseModels.Add(this.BaseModelId, this);
                }
            }
            this.BaseObjectId = objId;
            this._SetAttributes(attrs);
            WriteLock = false;
        }

        public void ResetAttributes(Dictionary<String, String> _attrs, ObjectId objId)
        {
            attrs.Clear();
            if (_attrs.ContainsKey("modelid"))
            {
                this.BaseModelId = Utils.ModelIdManager.Parse(_attrs["modelid"]);
                if (ApplicationBaseModels.ContainsKey(this.BaseModelId))
                {
                    ApplicationBaseModels[this.BaseModelId] = this;
                }
                else
                {
                    ApplicationBaseModels.Add(this.BaseModelId, this);
                }
            }
            this.BaseObjectId = objId;
            foreach(var item in _attrs)
            {
                attrs.Add(item.Key, item.Value);
            }
        }

        public void RestoreAttributes()
        {
            WriteLock = true;
            this._SetAttributes(attrs);
            WriteLock = false;
        }

        protected static BaseModel GetModelById(int id)
        {
            if(ApplicationBaseModels.ContainsKey(id))
            {
                return ApplicationBaseModels[id];
            }
            else
            {
                Utils.Logging.WriteMessage("BaseModel GetModelById: cannot find model id");
                return null;
            }
        }

        public void RequireLock()
        {
            WriteLock = true;
        }

        public void ReleaseLock()
        {
            WriteLock = false;
        }


        public void Save()
        {
            if(WriteLock == true)
            {
                return;
            }
            if(BaseObjectId == ObjectId.Null)
            {
                return;
            }
            using (DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument())
            {
                attrs.Clear();
                this.GetAttributes();
                if (attrs.ContainsKey("type"))
                {
                    attrs["type"] = this.ModelType();
                }
                else
                {
                    attrs.Add("type", this.ModelType());
                }
                if(attrs.ContainsKey("modelid"))
                {
                    attrs["modelid"] = Utils.ModelIdManager.toString(this.BaseModelId);
                }
                else
                {
                    attrs.Add("modelid", Utils.ModelIdManager.toString(this.BaseModelId));
                }
                Dictionary<String, String> pairs = attrs;
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
                try
                {
                    DBObject ent = trans.GetObject(BaseObjectId, OpenMode.ForRead);
                    

                    if (ent.ExtensionDictionary.IsNull)
                    {
                        ent.UpgradeOpen();
                        ent.CreateExtensionDictionary();
                    }

                    DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                    foreach (KeyValuePair<string, string> item in pairs)
                    {
                        String key = item.Key;
                        String value = item.Value;
                        Xrecord myXrecord;
                        if (extensionDict.Contains(key))
                        {
                            ObjectId entryId = extensionDict.GetAt(key);
                            myXrecord = default(Xrecord);
                            extensionDict.UpgradeOpen();
                            myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForWrite);
                            ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, value));
                            myXrecord.Data = data;
                        }
                        else
                        {
                            extensionDict.UpgradeOpen();
                            myXrecord = new Xrecord();
                            ResultBuffer data = new ResultBuffer(new TypedValue((int)DxfCode.Text, value));
                            myXrecord.Data = data;
                            extensionDict.SetAt(key, myXrecord);
                            trans.AddNewlyCreatedDBObject(myXrecord, true);
                        }
                    }
                    trans.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("a problem occured because " + ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
            
        }

        public static Dictionary<String,String> Load(ObjectId id)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = ed.Document.Database.TransactionManager.StartTransaction();
            
            try
            {
                Dictionary<String, String> pairs = new Dictionary<string,string>();
                DBObject ent = trans.GetObject(id, OpenMode.ForRead);
                if (ent != null && ent.ExtensionDictionary == ObjectId.Null)
                {
                    return pairs;
                }
                DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);
                DBObject dbObj;
                foreach (System.Collections.DictionaryEntry dEntry in extensionDict)
                {
                    dbObj = (DBObject)trans.GetObject((ObjectId)dEntry.Value, OpenMode.ForRead, false);
                    if (dbObj is DBDictionary)
                    {
                        
                    }
                    else if (dbObj is Xrecord)
                    {
                        foreach (TypedValue value in (Xrecord)dbObj)
                        {
                            String key =(String)dEntry.Key;
                            String svalue = value.ToString();
                            pairs.Add(key, svalue.Substring(3, svalue.Length - 4));
                        }
                    }
                }
                if (pairs.ContainsKey("type"))
                {
                    return pairs;

                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception e)
            {
                ed.WriteMessage("\n" + e.Message);
            }
            finally
            {
                trans.Dispose();
            }
            
            return null;
        }
     }
}
