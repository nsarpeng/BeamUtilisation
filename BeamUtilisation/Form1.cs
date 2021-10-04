using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace BeamUtilisation
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            EC3calcs EC3 = new EC3calcs();
            EC3.UBs(out string[] ubname, out double[] h, out double[] b, out double[] tw, out double[] tf, out double[] r, out double[] Iyy, out double[] Izz, out double[] Wyel, out double[] Wzel, out double[] Wypl, out double[] Wzpl, out double[] U, out double[] X, out double[] Iw, out double[] It, out double[] A);

            comboBoxSections.Items.AddRange(ubname);
            comboBoxSections.SelectedIndex = 0;

            comboBoxGrade.Items.Add("S275");
            comboBoxGrade.Items.Add("S355");
            comboBoxGrade.SelectedIndex = 0;

            comboBoxFamily.Items.Add("UB");
            comboBoxFamily.SelectedIndex = 0;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Click on the link below to continue learning how to build a desktop app using WinForms!
            System.Diagnostics.Process.Start("http://aka.ms/dotnet-get-started-desktop");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            test();
        }

        private void test()
        {

            EC3calcs EC3 = new EC3calcs();
            int idx = comboBoxSections.SelectedIndex;
            EC3.UBs(out string[] ubnames, out double[] hs, out double[] bs, out double[] tws, out double[] tfs, out double[] rs, out double[] Iys, out double[] Izs, out double[] Wyels, out double[] Wzels, out double[] Wypls, out double[] Wzpls, out double[] Us, out double[] Xs, out double[] Iws, out double[] Its, out double[] As);

            Double.TryParse(textBoxL.Text, out double L);
            Double.TryParse(textBoxk.Text, out double k);

            L *= 1000; // convert m to mm

            //string section = "1016x305x584";
            //double h = 1056;//mm
            //double b = 315;//mm
            //double tw = 36;//mm
            //double tf = 64;//mm
            //double r = 30;//mm
            //double d = 898.1; //mm
            //double Iy = 1246000; //cm4
            //double Iz = 33400; // cm4
            //double Wyel = 23600; //cm3
            //double Wzel = 2130; //cm3
            //double Wypl = 28000; //cm3
            //double Wzpl = 3480; //cm3
            //double U = 0.869;
            //double X = 18;
            //double Iw = 81.2; //dm6
            //double It = 7150; //cm4
            //double A = 744; //cm2

            string section = ubnames[idx];
            double h = hs[idx];//mm
            double b = bs[idx];//mm
            double tw = tws[idx];//mm
            double tf = tfs[idx];//mm
            double r = rs[idx];//mm
            double Iy = Iys[idx]; //cm4
            double Iz = Izs[idx]; // cm4
            double Wyel = Wyels[idx]; //cm3
            double Wzel = Wzels[idx]; //cm3
            double Wypl = Wypls[idx]; //cm3
            double Wzpl = Wzpls[idx]; //cm3
            double U = Us[idx];
            double X = Xs[idx];
            double Iw = Iws[idx]; //dm6
            double It = Its[idx]; //cm4
            double A = As[idx]; //cm2



            string grade = comboBoxGrade.SelectedItem.ToString(); // "S275";

            int secclass = Math.Max(EC3.SectionClassOutstandFlangeCompression(b/2-tw/2-r,tf,EC3.Fy(grade,tf)), EC3.SectionClassWebBending(h-2*tf-2*r,tw,EC3.Fy(grade,tw)));

            double py = Math.Min(EC3.Fy(grade, tf), EC3.Fy(grade, tw));

            
            //double L = 10000; //mm

            // UDL values
            double C1 = 1.127;
            double C2 = 0.454;
            double E = 210000; // MPa, EC3 3.2.6(1)
            double v = 0.3; // EC3 3.2.6(1)
            double G = E/(2*(1+v)); //MPa, EC2 3.2.6(1)

            double zg = 0;
            double kw = 1;

            //double k = 1;

            double McRd;
            double Wyy;
            double gammaM0 = 1;
            double gammaM1 = 1;

            if (secclass == 1 || secclass == 2)
            {
                McRd = Wypl*Math.Pow(10,3) * py / gammaM0;
                Wyy = Wyel;
            }
            else if (secclass == 3)
            {
                McRd = Wyel * Math.Pow(10, 3)* py / gammaM0;
                Wyy = Wypl;
            }
            else
            {
                McRd = 1;
                Wyy = 1;
            }

            double Mcr = EC3.Mcr(C1, C2, E, G, Iy * Math.Pow(10, 4), Iz * Math.Pow(10, 4), Iw * Math.Pow(10, 12), It * Math.Pow(10, 4), k, kw, L, zg);
            labelMcr.Text =  (Mcr/Math.Pow(10,6)).ToString("F0") + "kNm";
            labelMcRd.Text = (McRd/Math.Pow(10,6)).ToString("F0") + "kNm";

            string curve = EC3.BucklingCurveTable6pt5UKNA_DoubleSymmetricIHandHollow(h, b);
            double alphaLT = EC3.AlphaLT(curve);
            double lambdaLT = EC3.LambdaLT(Wyy * Math.Pow(10, 3), py , Mcr);
            double lambdaLT0 = 0.4; // UK NA.2.17(a) - rolled sections and hollow sections only
            double beta = 0.75; // UK NA.2.17(a) - rolled sections and hollow sections only
                
            double phiLT = EC3.PhiLT(alphaLT, lambdaLT, lambdaLT0, beta);
            double chiLT = EC3.ChiLT(phiLT, beta, lambdaLT);

            double MbRd = chiLT * Wyy * Math.Pow(10, 3) * py / gammaM1;

            labelIyy.Text = Iy.ToString("F0") + "cm4";
            labelIzz.Text = Iz.ToString("F0") + "cm4";
            labelWyy.Text = Wyy.ToString("F0") + "cm3";
            labelCurve.Text = curve;
            labelalphaLT.Text = alphaLT.ToString("F2");
            labelClass.Text = secclass.ToString("F0");
            labelfy.Text = py.ToString("F0") + " MPa";
            labelMbRd.Text = (MbRd / Math.Pow(10, 6)).ToString("F0") + "kNm";
        }

        private void LabelMcr_Click(object sender, EventArgs e)
        {

        }

        private void LabelMcRd_Click(object sender, EventArgs e)
        {

        }

        private void LabelMbRd_Click(object sender, EventArgs e)
        {

        }

        private void Label5_Click(object sender, EventArgs e)
        {

        }

        private void Label13_Click(object sender, EventArgs e)
        {

        }
    }
}
