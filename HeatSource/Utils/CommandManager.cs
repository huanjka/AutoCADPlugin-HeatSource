using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatSource.Utils
{
    public class CommandManager
    {
        public enum ToolCommand
        {
            DrawBuildingPoly,
            DrawBuildingRect,
            DrawSubStation,
            DrawHeatProducer,
            DrawPipeLine,
            DrawHeatProducerBuildingCloud,
            DrawHeatProducerSubStationCloud,
            DrawSubStationBuildingCloud,
            DrawPipeLineBuilding,
            ImportBackgroundImage,
            AdjustImageScale,
            GenerateDocument,
        }
        public List<ToolCommand> CommandsQueue = new List<ToolCommand>();
        private bool Lock = false;
        public void AddCommand(ToolCommand cmd)
        {
            CommandsQueue.Add(cmd);
        }
        public void TriggerCommand()
        {
            if(CommandsQueue.Count > 0)
            {
                ToolCommand cmd = CommandsQueue[0];
                CommandsQueue.Clear();
                switch(cmd)
                {
                    case ToolCommand.DrawBuildingPoly:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(1);
                        HeatSourceLayoutApp.tooPanel.drawBuildingBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawBuildingRect:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(2);
                        HeatSourceLayoutApp.tooPanel.drawRectBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawPipeLine:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(3);
                        HeatSourceLayoutApp.tooPanel.drawPiplineBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawHeatProducer:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(4);
                        HeatSourceLayoutApp.tooPanel.addSourceBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawSubStation:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(5);
                        HeatSourceLayoutApp.tooPanel.addStationBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawHeatProducerSubStationCloud:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(6);
                        HeatSourceLayoutApp.tooPanel.connectHSBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawHeatProducerBuildingCloud:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(7);
                        HeatSourceLayoutApp.tooPanel.connectHBBtn_Click(null, null);
                        break;

                    case ToolCommand.DrawSubStationBuildingCloud:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(8);
                        HeatSourceLayoutApp.tooPanel.connectSBBtn_Click(null, null);
                        break;
                    case ToolCommand.DrawPipeLineBuilding:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(10);
                        HeatSourceLayoutApp.tooPanel.drawPiplineBuildingBtn_Click(null, null);
                        break;
                    case ToolCommand.ImportBackgroundImage:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(11);
                        HeatSourceLayoutApp.tooPanel.importImageBtn_Click(null, null);
                        break;
                    case ToolCommand.AdjustImageScale:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(12);
                        HeatSourceLayoutApp.tooPanel.ajustPropBtn_Click(null, null);
                        break;
                    case ToolCommand.GenerateDocument:
                        HeatSourceLayoutApp.tooPanel.changeBtnStyle(9);
                        HeatSourceLayoutApp.tooPanel.generateIntroBtn_Click(null, null);
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if require lock success, false if not.</returns>
        public bool RequireLock()
        {
            if(this.Lock == false)
            {
                this.Lock = true;
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public void ReleaseLock()
        {
            this.Lock = false;
            HeatSourceLayoutApp.tooPanel.changeBtnStyle(-1);
        }

        public bool Status()
        {
            return CommandsQueue.Count != 0;
        }
               
    }
}
