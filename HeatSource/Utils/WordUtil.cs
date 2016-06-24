using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System.Collections;
using HeatSource.Model;
using Autodesk.AutoCAD.DatabaseServices;
using HeatSource.View;
using Autodesk.AutoCAD.Windows;
using System.IO;
namespace HeatSource.Utils
{
    class WordUtil
    {
        public static void CreateWord()
        {
            //杀掉winword.exe进程，加快执行速度，防止打开副本等Bug
            killWinWordProcess();

            //生成每个解决方案的图片
           if(HeatSourceLayoutApp.createScreen() == false)
            {
                return;
            }

            //当前表格数目
            int currentTableNum = 0;
            var dialog = new SaveFileDialog("选择文件存储位置", "燃气热源方案比选", "docx", "SaveFile", SaveFileDialog.SaveFileDialogFlags.DefaultIsFolder);
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            object savePath = dialog.Filename;
            var wordApp = new Microsoft.Office.Interop.Word.Application();
#if DEBUG
            object templatePath = HeatSourceLayoutApp.CurrentDirectory + @"/Template.docx";
#else
            object templatePath = HeatSourceLayoutApp.CurrentDirectory + @"/Resource/Template.docx";
#endif
            //object savaPath = System.IO.Directory.GetCurrentDirectory() + @"./燃气热源方案比选.docx";
            object miss = System.Reflection.Missing.Value;
            Document doc = wordApp.Documents.Open(ref templatePath, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss);

            //解决方案数目solNum
            //每个解决方案有1 + 热源数*3 + 1 个表格
            int solNum = HeatSourceLayoutApp.solutions.Count;
            //基本信息表
            Microsoft.Office.Interop.Word.Table infoTable;
            //热源能耗表
            Microsoft.Office.Interop.Word.Table hsEnergyTable;
            //热源运行费用表
            Microsoft.Office.Interop.Word.Table hsOperationTable;
            //热源初投资（造价）表
            Microsoft.Office.Interop.Word.Table hsInvestTable;
            //工程总表
            Microsoft.Office.Interop.Word.Table totalTable;

            //根据书签定位到待插入方案的位置
            object oStart = "SolForInsert";
            Range range = doc.Bookmarks.get_Item(ref oStart).Range;
#if DEBUG
            string heatPart = HeatSourceLayoutApp.CurrentDirectory + @"\heatPart.docx";
            string startPart = HeatSourceLayoutApp.CurrentDirectory + @"\startPart.docx";
            string endPart = HeatSourceLayoutApp.CurrentDirectory +  @"\endPart.docx";
            object heatFile = HeatSourceLayoutApp.CurrentDirectory + @"\heatPart.docx";
            object startFile = HeatSourceLayoutApp.CurrentDirectory + @"\startPart.docx";
            object endFile = HeatSourceLayoutApp.CurrentDirectory + @"\endPart.docx";
#else
            string heatPart =  HeatSourceLayoutApp.CurrentDirectory + @"\Resource\heatPart.docx";
            string startPart = HeatSourceLayoutApp.CurrentDirectory + @"\Resource\startPart.docx";
            string endPart = HeatSourceLayoutApp.CurrentDirectory + @"\Resource\endPart.docx";
            object heatFile = HeatSourceLayoutApp.CurrentDirectory + @"\Resource\heatPart.docx";
            object startFile = HeatSourceLayoutApp.CurrentDirectory + @"\Resource\startPart.docx";
            object endFile = HeatSourceLayoutApp.CurrentDirectory + @"\Resource\endPart.docx";
#endif
            //首先根据模板文件Template.docx，插入表格
            //insertFile()是倒着插入
            for (int sl = solNum - 1; sl >= 0; sl--)
            {
                // insert a new line
                var pText = range.Paragraphs.Add();
                pText.Range.Text = String.Format("\n");
                pText.Range.InsertParagraphAfter();

                //插入解决方案的图片
                string imagePath =  HeatSourceLayoutApp.CurrentDirectory + "\\tmp\\" + sl + ".jpg";
                object linktofile = false;
                object savewithdocument = true;
                Microsoft.Office.Interop.Word.InlineShape shape = wordApp.ActiveDocument.InlineShapes.AddPicture(imagePath,
                    ref linktofile, ref savewithdocument, range);
                shape.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                //倒序插入，先插入末尾部分
                Document endPartFile = wordApp.Documents.Open(ref endFile, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss);
                object oEnd = "EndPartPosition";
                object endPartIndex = "EndPartIndex";
                Range endRange = endPartFile.Bookmarks.get_Item(ref oEnd).Range;
                Range endIndexRange = endPartFile.Bookmarks.get_Item(ref endPartIndex).Range;
                endIndexRange.Text = (sl + 1).ToString();
                endRange.Text = (sl + 1).ToString();
                range.InsertFile(endPart, ref miss, ref miss, ref miss);
                //不保存直接关闭
                object isSave = false;
                endPartFile.Close(ref isSave, ref miss, ref miss);
                currentTableNum++;

                //根据当前解决方案内的热源数目，插入热源部分
                int heatSourceNum = HeatSourceLayoutApp.solutions[sl].HeatProducers.Count;
                for (int h = heatSourceNum; h > 0; h--)
                {
                    Document heatPartFile = wordApp.Documents.Open(ref heatFile, ref miss, ref miss,
                                      ref miss, ref miss, ref miss, ref miss,
                                      ref miss, ref miss, ref miss, ref miss,
                                      ref miss, ref miss, ref miss, ref miss,
                                      ref miss);
                    object solutionIndex = "SolutionIndex";
                    object heatProducerIndex = "HeatProducerIndex";
                    object heatProducerPosition = "HeatProducerPosition";
                    Range solRange = heatPartFile.Bookmarks.get_Item(ref solutionIndex).Range;
                    Range heatIndexRange = heatPartFile.Bookmarks.get_Item(ref heatProducerIndex).Range;
                    Range heatPositionRange = heatPartFile.Bookmarks.get_Item(ref heatProducerPosition).Range;
                    solRange.Text = (sl + 1).ToString();
                    heatIndexRange.Text = h.ToString();
                    heatPositionRange.Text = h.ToString();
                    range.InsertFile(heatPart, ref miss, ref miss, ref miss);
                    heatPartFile.Close(ref isSave, ref miss, ref miss);
                    currentTableNum += 3;
                }

                //最后插入开头部分
                Document startPartFile = wordApp.Documents.Open(ref startFile, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss, ref miss, ref miss, ref miss,
                                       ref miss);
                object oStartPart = "StartPartPosition";
                object oStartPart2 = "StartPartPosition_2";
                Range startRange = startPartFile.Bookmarks.get_Item(ref oStartPart).Range;
                Range startRange_2 = startPartFile.Bookmarks.get_Item(ref oStartPart2).Range;
                startRange.Text = (sl + 1).ToString();
                startRange_2.Text = (sl + 1).ToString();
                range.InsertFile(startPart, ref miss, ref miss, ref miss);
                startPartFile.Close(ref isSave, ref miss, ref miss);
                currentTableNum++;

            }

            ////填数据
            ////当前解决方案的下标是sl
            for (int sl = 0; sl < solNum; sl++)
            {
                //之前解决方案内热源数目
                int lastHsNum = 0;
                for (int k = 0; k < sl; k++)
                {
                    lastHsNum += HeatSourceLayoutApp.solutions[k].HeatProducers.Count;
                }
                //之前共有表格数
                int tableNum = 3 * lastHsNum + 2 * sl;


                //第一部分，基本信息表 6*2
                //报System.Runtime.InteropServices.COMException (0x800706BA): RPC 服务器不可用。 
                infoTable = doc.Tables[tableNum + 1];
                //填充地点信息
                infoTable.Cell(1, 2).Range.Text = HeatSourceLayoutApp.globalProperty.CityName;
                //填充地区类型
                string typeString = Constants.LocationType[HeatSourceLayoutApp.globalProperty.LocationType];
                infoTable.Cell(2, 2).Range.Text = typeString;
                infoTable.Cell(2, 2).Range.Bold = 1;
                //填充气象参数
                //室外温度 t_(a.h) 室外均温 t_a 供暖天数 D
                double IndoorTemperature = HeatSourceLayoutApp.globalProperty.IndoorTemperature;
                double OutDoorTemp = HeatSourceLayoutApp.globalProperty.OutDoorTemp;
                double OutAverageTemp = HeatSourceLayoutApp.globalProperty.OutAverageTemp;
                double HeatingDays = HeatSourceLayoutApp.globalProperty.HeatingDays;
                infoTable.Cell(3, 2).Range.Text = HeatingDays.ToString();
                infoTable.Cell(4, 2).Range.Text = IndoorTemperature.ToString();
                infoTable.Cell(5, 2).Range.Text = OutDoorTemp.ToString();
                infoTable.Cell(6, 2).Range.Text = OutAverageTemp.ToString();
                infoTable.Cell(3, 2).Range.Bold = 1;
                infoTable.Cell(4, 2).Range.Bold = 1;
                infoTable.Cell(5, 2).Range.Bold = 1;
                infoTable.Cell(6, 2).Range.Bold = 1;

                //第二部分，热源对应的三个表
                //当前解决方案内热源数目
                int hsNum = HeatSourceLayoutApp.solutions[sl].HeatProducers.Count;
                //当前解决方案供热总面积 全年总耗热量 总运行费用 总造价
                double totalArea = 0;
                double totalHeat = 0;
                for (int hs = 0; hs < hsNum; hs++)
                {
                    //1 - 能耗计算
                    //根据热源包含的楼房数目添加行数
                    hsEnergyTable = doc.Tables[tableNum + 1 + 1 + 3 * hs];
                    int directBuildingNum = 0;
                    int slaveBuildingNum = 0;
                    //记录热力站中的buildings
                    List<Building> substationBuildings = new List<Building>();
                    if (HeatSourceLayoutApp.solutions[sl].HeatProducers.ElementAt(hs).Value.OwnSlaveDistrict != null)
                    {
                        foreach (var sub in HeatSourceLayoutApp.solutions[sl].HeatProducers.ElementAt(hs).Value.OwnSlaveDistrict.SubStations)
                        {
                            if (sub.OwnMasterDistrict != null)
                            {
                                foreach (var b in sub.OwnMasterDistrict.Buildings)
                                    substationBuildings.Add(b);
                            }
                        }
                    }
                    slaveBuildingNum = substationBuildings.Count;

                    //如果不含有OwnSlaveDistrict,报NullRefException
                    try
                    {
                        directBuildingNum = HeatSourceLayoutApp.solutions[sl].HeatProducers.ElementAt(hs).Value.OwnMasterDistrict.Buildings.Count;
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();

                    }
                    //填充热源中楼房信息,从第2行开始，依次为
                    //楼号-建筑类型-面积-供暖方式-采暖热指标-采暖热负荷-节能措施-直接全年耗热量
                    int totalBuildingNum = directBuildingNum + slaveBuildingNum;
                    int lineIndex = 2;
                    for (int b = 0; b < directBuildingNum; b++)
                    {
                        Building building = HeatSourceLayoutApp.solutions[sl].HeatProducers.ElementAt(hs).Value.OwnMasterDistrict.Buildings[b];
                        hsEnergyTable.Rows.Add(ref miss);
                        hsEnergyTable.Cell(lineIndex, 1).Range.Text = (b + 1).ToString();
                        //建筑类型
                        string typeStr = Constants.BuildingType[building.BuildingType];
                        hsEnergyTable.Cell(lineIndex, 2).Range.Text = typeStr;
                        hsEnergyTable.Cell(lineIndex, 3).Range.Text = building.Area.ToString();
                        //供暖方式
                        string styleStr = Constants.HeatStyle[building.HeatStyle];
                        hsEnergyTable.Cell(lineIndex, 4).Range.Text = styleStr;
                        hsEnergyTable.Cell(lineIndex, 5).Range.Text = building.HeatIndex.ToString();
                        hsEnergyTable.Cell(lineIndex, 6).Range.Text = building.HeatLoad.ToString();
                        //节能措施
                        string eneStyleStr = Constants.SaveStyle[building.EnergySavingStyle];
                        hsEnergyTable.Cell(lineIndex, 7).Range.Text = eneStyleStr;
                        hsEnergyTable.Cell(lineIndex, 8).Range.Text = building.YearHeat.ToString();

                        lineIndex++;
                    }
                    //如果热源中包含热力站，把热力站中的楼房也加进去
                    for (int b = 0; b < slaveBuildingNum; b++)
                    {
                        Building building = substationBuildings[b];
                        hsEnergyTable.Rows.Add(ref miss);
                        hsEnergyTable.Cell(lineIndex, 1).Range.Text = (b + 1).ToString();
                        //建筑类型
                        string typeStr = Constants.BuildingType[building.BuildingType];
                        hsEnergyTable.Cell(lineIndex, 2).Range.Text = typeStr;
                        hsEnergyTable.Cell(lineIndex, 3).Range.Text = building.Area.ToString();
                        //供暖方式
                        string styleStr = Constants.HeatStyle[building.HeatStyle];
                        hsEnergyTable.Cell(lineIndex, 4).Range.Text = styleStr;
                        hsEnergyTable.Cell(lineIndex, 5).Range.Text = building.HeatIndex.ToString();
                        hsEnergyTable.Cell(lineIndex, 6).Range.Text = building.HeatLoad.ToString();
                        //节能措施
                        string eneStyleStr = Constants.SaveStyle[building.EnergySavingStyle];
                        hsEnergyTable.Cell(lineIndex, 7).Range.Text = eneStyleStr;
                        hsEnergyTable.Cell(lineIndex, 8).Range.Text = building.YearHeat.ToString();

                        lineIndex++;
                    }

                    //2 - 运行费用 2*8
                    //燃料耗量-燃气单价-燃料耗费-水泵全年运行能耗-水泵运行耗电-电费单价-运行电费-总运行费用
                    hsOperationTable = doc.Tables[tableNum + 1 + 2 + 3 * hs];
                    //燃料耗量--采暖总耗热量？？？
                    HeatProducer heatProducer = HeatSourceLayoutApp.solutions[sl].HeatProducers.ElementAt(hs).Value;
                    //更新解决方案的工程总表信息
                    totalArea += heatProducer.TotalArea;
                    totalHeat += heatProducer.TotalLoad;
                    hsOperationTable.Cell(2, 1).Range.Text = heatProducer.TotalLoad.ToString();
                    hsOperationTable.Cell(2, 2).Range.Text = HeatSourceLayoutApp.globalProperty.GasPrice.ToString();
                    hsOperationTable.Cell(2, 3).Range.Text = heatProducer.GetGasHeatingCost().ToString("0.0");
                    hsOperationTable.Cell(2, 4).Range.Text = heatProducer.totalWaterPumpEnergyConsumption.ToString("0.0");
                    hsOperationTable.Cell(2, 5).Range.Text = HeatSourceLayoutApp.globalProperty.ElecPrice.ToString("0.0");
                    hsOperationTable.Cell(2, 6).Range.Text = heatProducer.GetWaterPumpOperationCost().ToString("0.0");
                    hsOperationTable.Cell(2, 7).Range.Text = (heatProducer.GetGasHeatingCost() + heatProducer.GetWaterPumpOperationCost()).ToString("0.0");

                    //3 - 初投资费用（造价）
                    //锅炉造价-换热器造价-水泵造价-管线造价-总造价
                    hsInvestTable = doc.Tables[tableNum + 1 + 3 + 3 * hs];
                    hsInvestTable.Cell(2, 1).Range.Text = heatProducer.GetBoildersCost().ToString("0.0");
                    hsInvestTable.Cell(2, 2).Range.Text = heatProducer.GetHeatExchangerCost().ToString("0.0");
                    hsInvestTable.Cell(2, 3).Range.Text = heatProducer.GetWaterPumpCost().ToString("0.0");
                    hsInvestTable.Cell(2, 4).Range.Text = heatProducer.GetPipeLineCost().ToString("0.0");
                    hsInvestTable.Cell(2, 5).Range.Text = heatProducer.GetTotalCost().ToString("0.0");

                }
                //当前方案的工程总表, 2*4 
                //建筑总面积-全年总耗热量-总运行费用-总造价
                //之前共有表格数
                int tableN = tableNum + 1 + 3 * hsNum;
                totalTable = doc.Tables[tableN + 1];
                totalTable.Cell(2, 1).Range.Text = totalArea.ToString();
                totalTable.Cell(2, 2).Range.Text = totalHeat.ToString("0.0");
                double totalFee = HeatSourceLayoutApp.solutions[sl].GetTotalFee();
                totalTable.Cell(2, 3).Range.Text = totalFee.ToString("0.0");
                //总造价
                totalTable.Cell(2, 4).Range.Text = HeatSourceLayoutApp.solutions[sl].GetTotalCost().ToString("0.0");
            }


            //最终的方案比较，根据解决方案数目动态增加列数
            //int currentTableNum = doc.Tables.Count; //不能用
            //方案比较表 8 * (solNUm + 1)
            Microsoft.Office.Interop.Word.Table solTable = doc.Tables[currentTableNum + 1];
            for (int s = 1; s <= solNum; s++)
            {
                //为每个解决方案增加一列
                solTable.Columns.Add(ref miss);

                solTable.Cell(1, s + 1).Range.Text = "方案" + s;
                //建筑总面积
                solTable.Cell(2, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].TotalArea.ToString("0.0");
                //全年耗热量
                solTable.Cell(3, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].TotalLoad.ToString("0.0");
                //燃料费用
                solTable.Cell(4, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].GetGasHeatingCost().ToString("0.0");
                //水泵全年运行能耗
                solTable.Cell(5, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].GetTotalWaterPumpEnergyConsumption().ToString("0.0");
                //运行电费
                solTable.Cell(6, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].GetWaterPumpCost().ToString("0.0");
                //总运行费用
                solTable.Cell(7, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].GetTotalFee().ToString("0.0");
                //总造价（初投资）
                solTable.Cell(8, s + 1).Range.Text = HeatSourceLayoutApp.solutions[s - 1].GetTotalCost().ToString("0.0");
            }

            //生成总方案对比图片
            int attrNum = 4;
            Excel.Application excel = new Excel.Application();
            //excel.Visible = true;
            object misValue = System.Reflection.Missing.Value;
            Excel.Workbook xlWorkBook = excel.Workbooks.Add(misValue);
            Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //fill data
            xlWorkSheet.Cells[1, 1] = "";
            for (int i = 1; i <= solNum; i++)
                xlWorkSheet.Cells[1, i+1] = "方案" + i.ToString();
            xlWorkSheet.Cells[2, 1] = "全年能耗（GJ）";
            xlWorkSheet.Cells[3, 1] = "水泵功耗（KW∙h）";
            xlWorkSheet.Cells[4, 1] = "运行费用（万元）";
            xlWorkSheet.Cells[5, 1] = "总造价（万元）";
            for (int s = 1; s <= solNum; s++)
            {
                xlWorkSheet.Cells[2, s + 1] = HeatSourceLayoutApp.solutions[s - 1].TotalLoad.ToString("0.0");
                xlWorkSheet.Cells[3, s + 1] = HeatSourceLayoutApp.solutions[s - 1].GetTotalWaterPumpEnergyConsumption().ToString("0.0");
                double totalFee = HeatSourceLayoutApp.solutions[s - 1].GetTotalFee() / 10000.0; // 万元
                xlWorkSheet.Cells[4, s + 1] = totalFee.ToString("0.0");
                double totalCost = HeatSourceLayoutApp.solutions[s - 1].GetTotalCost() / 10000.0; //   万元
                xlWorkSheet.Cells[5, s + 1] = totalCost.ToString("0.0");
            }

            // generate one chart for every attributes
            string[] imageNames = new string[4];
            for (int i = 2; i <= attrNum + 1; ++i)
            {
                // create chart
                Excel.Range chartRange;
                string r1 = "A" + (char)('0' + i);
                string r2 = new string((char)('A' + solNum), 1) + (char)('0' + i);
                chartRange = xlWorkSheet.get_Range(r1, r2);
                Excel.ChartObjects xlCharts = (Excel.ChartObjects)xlWorkSheet.ChartObjects(Type.Missing);
                Excel.ChartObject myChart = (Excel.ChartObject)xlCharts.Add(10, 80, 600, 450);
                Excel.Chart chartPage = myChart.Chart;
                chartPage.SetSourceData(chartRange, misValue);
                chartPage.ChartType = Excel.XlChartType.xlColumnClustered;

                // save chart into image
                string path = Directory.GetCurrentDirectory();
                string imageName = path + "../../../chart" + i + ".bmp";
                imageNames[i - 2] = imageName;
                chartPage.Export(imageName, "BMP", misValue);
            }

            for (int i = 0; i < attrNum; ++i)
            {
                // append chart images
                string fileName = imageNames[i];
                object linkToFile = false;
                object saveWithDocument = true;
                object oEndOfDoc = "\\endofdoc";
                Range pictureRange = doc.Bookmarks.get_Item(ref oEndOfDoc).Range;
                //pictureRange.InsertAfter("总解决方案对比图");
                doc.InlineShapes.AddPicture(fileName, linkToFile, saveWithDocument, pictureRange);
            }

            // show Word
            wordApp.Visible = true;

            // save Word
            doc.SaveAs(ref savePath, ref miss, ref miss,
                                   ref miss, ref miss, ref miss, ref miss,
                                   ref miss, ref miss, ref miss, ref miss,
                                   ref miss, ref miss, ref miss, ref miss,
                                   ref miss);
            //关闭doc,wordApp对象
            //((_Document)doc).Close();
            //((_Application)wordApp).Quit();

            // close and release
            xlWorkBook.Close(false, misValue, misValue);
            excel.Quit();

            //releaseObject(xlWorkSheet);
            //releaseObject(xlWorkBook);
            //releaseObject(excel);
            //Console.WriteLine("Excel file created , you can find the file: " + excelName);
        }
        private static void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }
        public static void killWinWordProcess()
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("WINWORD");
            foreach (System.Diagnostics.Process process in processes)
            {
                bool b = process.MainWindowTitle == "";
                if (process.MainWindowTitle == "")
                {
                    process.Kill();
                }
            }
        }
    }
}
