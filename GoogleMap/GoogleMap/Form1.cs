using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Threading;

namespace GoogleMap
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class Form1 : Form
    {
        float[,] dis;
        int[] ans;
        int div_count;
        int interscape;
        string[] latLng;
        public Form1()
        {
            InitializeComponent();
            div_count = 0;
            interscape = 0;
            dis = new float[20, 20];
            latLng = new string[20];
            ans = new int[20];
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.AllowWebBrowserDrop = false;
            webBrowser1.IsWebBrowserContextMenuEnabled = false;
            webBrowser1.WebBrowserShortcutsEnabled = false;
            webBrowser1.ObjectForScripting = this;
            webBrowser1.Url = new Uri(@"C:\Users\Administrator\Desktop\Test6.htm");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int n = (int)webBrowser1.Document.InvokeScript("getPosition");
            double count = System.Math.Ceiling((double)n * n / 100);
            interscape = 100 / n;
            int m = 100 / n;
            int var1 = div_count*m;
            int var2 = (div_count+1)*m;
            object[] args = new object[2];
            if (var2 < n)
            {
                args[0]=var1;
                args[1]=var2;
                webBrowser1.Document.InvokeScript("calcMatrixDistance", args);
                div_count++;
                var1 = div_count * m;
                var2 = (div_count + 1) * m;
            }
            else
            {
                args[0] = var1;
                args[1] = n;
                div_count++;
                webBrowser1.Document.InvokeScript("calcMatrixDistance", args);
            }
            Thread.Sleep(2000);
            //label1.Text = "还需要获取" + (count - div_count).ToString() + "次距离数据";
            
        }


        public double myTest(double i, double j)
        {
            return i + j;
        }

        public void setLatAndLng(int i, string str)
        {
            latLng[i] = str;
        }

        public void fillInMatrix(int i, int j, int k)
        {
            int var1 = (div_count-1)* interscape + i;
            dis[var1, j] = k;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //webBrowser1.Document.InvokeScript("test");
            int n = (int)webBrowser1.Document.InvokeScript("getPosition");
            ans = pso_tsp(dis, n);
            object[] args = new object[2];
            for (int i = 1; i < n; i++)
            {
                args[0] = ans[i - 1];
                args[1] = ans[i];
                webBrowser1.Document.InvokeScript("calcRoute", args);
                Thread.Sleep(1000);
            }
            args[0] = ans[n - 1];
            args[1] = ans[0];
            webBrowser1.Document.InvokeScript("calcRoute", args);
            string str = "";
            for (int i = 0; i < n; i++)
            {
                str += latLng[i] + "\n";
            }
            textBox1.Text = str;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string address = textBox2.Text.Trim();
            object[] args = new object[1];
            args[0] = address;
            webBrowser1.Document.InvokeScript("getAddress", args);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("increaseZoom");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("decreaseZoom");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int num = comboBox1.SelectedIndex;
            object[] args = new object[1];
            args[0] = num;
            webBrowser1.Document.InvokeScript("changeMapType", args);
        }

        private int[] pso_tsp(float[,] Dist, int cityNum)
        {
            //粒子个数
            int indiNum = 1000;
            //迭代次数
            int nMax = 100;
            //计算城市间距离
            /*
            float[,] Dist = new float[cityNum, cityNum];
            for (int i = 0; i < cityNum; i++){
                for (int j = i + 1; j < cityNum; j++){
                    Dist[i, j] = (float)Math.Sqrt((City[i, 0] - City[j, 0]) * (City[i, 0] - City[j, 0])
                        + (City[i, 1] - City[j, 1]) * (City[i, 1] - City[j, 1]));
                    Dist[j, i] = Dist[i, j];
                }
            }*/
            //初始化粒子
            int[,] tourPbest = new int[indiNum, cityNum];
            for (int i = 0; i < cityNum; i++)
                for (int j = 0; j < indiNum; j++)
                    tourPbest[j, i] = i;
            Random rand = new Random();
            for (int i = 0; i < indiNum; i++)
                for (int j = 0; j < cityNum; j++)
                {
                    int pos = rand.Next(cityNum - j);
                    int temp = tourPbest[i, pos];
                    tourPbest[i, pos] = tourPbest[i, cityNum - j - 1];
                    tourPbest[i, cityNum - j - 1] = temp;
                }
            //计算适应度，记录全体和局部最优
            float[] indiFit = new float[indiNum];
            int[] tourGbest = new int[cityNum];
            float[] recordPbest = new float[indiNum];
            float recordGbest = float.PositiveInfinity;
            int[,] indis = new int[indiNum, cityNum];

            int min_tag = 0;
            float min = float.PositiveInfinity;
            for (int i = 0; i < indiNum; i++)
            {
                float dist = 0.0f;
                for (int j = 0; j < cityNum; j++)
                    dist += Dist[tourPbest[i, j], tourPbest[i, (j + 1) % (cityNum)]];
                recordPbest[i] = dist;
                if (dist < min)
                {
                    min = dist;
                    min_tag = i;
                    recordGbest = min;// 本轮中全局最优粒子
                }
            }

            for (int i = 0; i < indiNum; i++)
                for (int j = 0; j < cityNum; j++)
                    indis[i, j] = tourPbest[i, j];
            for (int j = 0; j < cityNum; j++)
                tourGbest[j] = tourPbest[min_tag, j];// 初始化 tourGbest

            // 循环寻找最优路径
            for (int iter = 0; iter < nMax; iter++)
            {
                ////////////////////////
                //对每个粒子进行交叉操作///
                ////////////////////////
                for (int i = 0; i < indiNum; i++)
                {
                    for (int Case = 1; Case <= 2; Case++)
                    {
                        //产生交叉染色体 chb
                        int c1, c2, chb1, chb2, length;
                        int[] chb;
                        do
                        {
                            c1 = rand.Next(cityNum);
                            c2 = rand.Next(cityNum);
                        } while (c1 == c2);
                        if (c1 < c2)
                        {
                            chb1 = c1; chb2 = c2;
                        }
                        else
                        {
                            chb1 = c2; chb2 = c1;
                        }
                        length = chb2 - chb1 + 1;
                        chb = new int[length];
                        for (int t = 0; t < length; t++)
                        {
                            if (Case == 1)
                                chb[t] = tourPbest[i, t + chb1];//第一次：与当前粒子历史最优解组合
                            if (Case == 2)
                                chb[t] = tourGbest[t + chb1];//第二次：与全局粒子最优解组合
                        }
                        /////////////////////////
                        /////////////////////////  output
                        //Console.Write("iter: {0} indi: {1} case: {2} - ",iter,i,Case);
                        //for (int f = 0; f < length; f++)
                        //Console.Write("{0} ", chb[f]);
                        //Console.WriteLine();
                        //////////////////////////  output
                        //////////////////////////
                        //与当前迭代同一粒子交叉
                        for (int k = 0; k < length; k++)
                        {
                            for (int j = 0; j < cityNum; j++)
                            {
                                if (indis[i, j] == chb[k])
                                {
                                    for (int t = j; t < cityNum - 1; t++)
                                        indis[i, t] = indis[i, t + 1];
                                    indis[i, cityNum - 1] = -1;
                                }
                            }
                        }
                        /////////////////////////
                        /////////////////////////  output
                        //for (int f = 0;f < cityNum; f++)
                        //    Console.Write("{0} ", indis[i,f]);
                        //Console.WriteLine();
                        //////////////////////////  output
                        //////////////////////////
                        for (int t = 0; t < length; t++)
                            indis[i, cityNum - length + t] = chb[t];
                        /////////////////////////
                        /////////////////////////  output
                        //for (int f = 0; f < cityNum; f++)
                        //    Console.Write("{0} ", indis[i, f]);
                        //Console.WriteLine();
                        //////////////////////////  output
                        //////////////////////////
                        //新路径变短则接受
                        float dist = 0.0f;
                        for (int j = 0; j < cityNum; j++)
                            dist += Dist[indis[i, j], indis[i, (j + 1) % (cityNum)]];
                        if (dist < recordPbest[i])
                        {
                            for (int j = 0; j < cityNum; j++)
                                tourPbest[i, j] = indis[i, j];//更新路径
                            recordPbest[i] = dist;//更新最小值
                        }
                    }
                }
                ////////////////////////
                //对每个粒子进行变异操作///
                ////////////////////////
                for (int i = 0; i < indiNum; i++)
                {
                    int c1, c2, temp;
                    do
                    {
                        c1 = rand.Next(cityNum);
                        c2 = rand.Next(cityNum);
                    } while (c1 == c2);
                    temp = indis[i, c1];
                    indis[i, c1] = indis[i, c2];
                    indis[i, c2] = temp;
                    //路径长度变短则接受
                    float dist = 0.0f;
                    for (int j = 0; j < cityNum; j++)
                        dist += Dist[indis[i, j], indis[i, (j + 1) % (cityNum)]];
                    if (dist < recordPbest[i])
                    {
                        for (int j = 0; j < cityNum; j++)
                            tourPbest[i, j] = indis[i, j];
                        recordPbest[i] = dist;
                    }
                }

                // 更新 全局最优
                min = float.PositiveInfinity;
                for (int i = 0; i < indiNum; i++)
                {
                    if (recordPbest[i] < min)
                    {
                        min_tag = i;
                        min = recordPbest[i];
                    }
                }
                recordGbest = min;
                for (int j = 0; j < cityNum; j++)
                    tourGbest[j] = tourPbest[min_tag, j];
                Console.WriteLine("iter: {0} - {1}", iter, recordGbest);
            }// 循环寻找最优路径结束
            Console.WriteLine("Algorithm over ! Result is : {0}", recordGbest);
            for (int j = 0; j < cityNum; j++)
            {
                Console.Write(tourGbest[j]);
                Console.Write(" ");
            }
            return tourGbest;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            webBrowser1.Document.InvokeScript("clean");
            div_count = 0;
            interscape = 0;
            dis = new float[20, 20];
            latLng = new string[20];
            ans = new int[20];
        }//// pso_tsp 函数结束

    }
}
