using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UECap_Feature_Parser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            getBandCombiantionList();
        }
        public bool getSrsCs(int a)
        {
            var start = textBox1.Text.IndexOf("supportedBandCombinationList-v1540");
            var stop = textBox1.Text.IndexOf("srs-SwitchingTimeRequested");
            try
            {
                String result = textBox1.Text.Substring(start, stop - start);
                var array = result.Split("bandList-v1540");
                try { if (array[a].IndexOf("srs-CarrierSwitch") != -1) return true; }
                catch { return false; }
            }
            catch { return false; }
            return false;
        }
        public  void getBandCombiantionList()
        {
            var nr_start = textBox1.Text.IndexOf("supportedBandListNR");
            var nr_result = textBox1.Text.Substring(nr_start, textBox1.TextLength - nr_start);
            var start = nr_result.IndexOf("supportedBandCombinationList");
            var stop = nr_result.IndexOf("appliedFreqBandListFilter");
            String result = nr_result.Substring(start, stop-start);
            var array = result.Split("[0] -> nr");
            int bandIndex = 0;
            List<List<OneBandCombinationFeature>> AllTable = new List<List<OneBandCombinationFeature>>();
            foreach (var item in array) // 这里是每一个聚合的列表
            {
                List<OneBandCombinationFeature> oneBandCombinationFeature = new List<OneBandCombinationFeature>();
                var bandCombiantion = item.Split("bandNR : ");
                if (bandCombiantion[0] != item)
                {
                    foreach (var item2 in bandCombiantion)  // item2已经是核心内容乐，通常情况下只会存在一个ca-BandwidthClassDL-NR，此时逐行扫描即可
                    {
                        //if (item2 == "")
                        try
                        {
                            OneBandCombinationFeature bandInfo = new OneBandCombinationFeature();
                            var resultArray = item2.Split("\r\n");
                            bandInfo.bandNum = Convert.ToInt32(resultArray[0]);

                            foreach (var item3 in resultArray)
                            {
                                if (item3.IndexOf("ca-BandwidthClassDL-NR") != -1) bandInfo.DL = item3.Split(":")[1];
                                if (item3.IndexOf("ca-BandwidthClassUL-NR") != -1) bandInfo.UL = item3.Split(":")[1];
                            }
                            bandInfo.bandIndex = bandIndex;
                            oneBandCombinationFeature.Add(bandInfo);
                        }
                        catch { }
                    }
                    
                }
                //这里oneBandCombinationFeature已经收集完这个组合的每一个band信息，接下来就是汇总
                AllTable.Add(oneBandCombinationFeature);
                bandIndex++;
            }
            String lastResult = "";
            var bandCaIndex = 0;
            foreach (var item in AllTable)
            {
                foreach(var item2 in item)
                {
                    if (item2.bandNum == 0) break;
                    lastResult += $"{item2.bandNum}{item2.DL}{item2.UL} + ";
                    
                }
                if (lastResult.Length != 0) lastResult += ("SRS CS:"+getSrsCs(bandCaIndex) + "\r\n");
                bandCaIndex++;
            }
            textBox2.Text = lastResult.Replace(" ","");
        }
    }
    public class OneBandCombinationFeature
    {
        public int bandIndex = 0;
        public int bandNum = 0;
        public string UL = "";
        public string DL = "";
    }

}
