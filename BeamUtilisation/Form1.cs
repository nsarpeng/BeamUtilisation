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
        string verboseOutput;

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

            verboseOutput = "<!DOCTYPE html><html><head>";
            verboseOutput += "<meta http-equiv=\"x-ua-compatible\" content=\"IE=11\">";
            verboseOutput += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">";
            //verboseOutput += "<style>";
            //verboseOutput += "table.table1 {border-collapse: collapse;}";
            //verboseOutput += "table.table1, td.td1, th.th1 {border: 1px solid black;}";
            //verboseOutput += "td.td1 {text-align:center;}";
            //verboseOutput += "th.th1 {text-align:center;}";
            //verboseOutput += "th.th1 {background-color: #04AA6D;color: white;}";
            //verboseOutput += "tr.tr1:nth-child(even) {background-color: #f2f2f2;}";
            //verboseOutput += "</style>";
            verboseOutput += "</head><body>";

            EC3calcs EC3 = new EC3calcs();
            int idx = comboBoxSections.SelectedIndex;
            EC3.UBs(out string[] ubnames, out double[] hs, out double[] bs, out double[] tws, out double[] tfs, out double[] rs, out double[] Iys, out double[] Izs, out double[] Wyels, out double[] Wzels, out double[] Wypls, out double[] Wzpls, out double[] Us, out double[] Xs, out double[] Iws, out double[] Its, out double[] As);

            Double.TryParse(textBoxL.Text, out double L);
            Double.TryParse(textBoxk.Text, out double k);
            Double.TryParse(textBoxVEdz.Text, out double VEd);

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

            verboseOutput += SectionOutput(section, h, b, tw, tf, r, Iy, Iz, Wyel, Wzel, Wypl, Wzpl, U, X, Iw, It, A);

            string grade = comboBoxGrade.SelectedItem.ToString(); // "S275";

            double py = Math.Min(EC3.Fy(grade, tf), EC3.Fy(grade, tw));
            double epsilon = EC3.Epsilon(py);

            int secclass = Math.Max(EC3.SectionClassOutstandFlangeCompression(b/2-tw/2-r,tf, epsilon), EC3.SectionClassWebBending(h-2*tf-2*r,tw, epsilon));

            
            
            //double L = 10000; //mm

            // UDL values
            double C1 = 1.127;
            double C2 = 0.454;
            double E = 210000; // MPa, EC3 3.2.6(1)
            double v = 0.3; // EC3 3.2.6(1)
            double G = EC3.G(E,v); //MPa, EC2 3.2.6(1)

            double zg = 0;
            double kw = 1;

            double gammaM0 = 1;
            double gammaM1 = 1;

            // cross section shear resistance
            double Av = EC3.AvIHparallel(A, h, b, tf, tw, r);
            double VplRd = EC3.VplRd(Av, py, gammaM0);
            Boolean shearbuckling = EC3.Shearbuckling(epsilon, 1, h - 2 * tf, tw);
            Boolean shearinteraction = EC3.ShearInteraction(VEd*1000, VplRd);

            double rho;
            double pymod;
            if (shearinteraction == true)
            {
                 rho = EC3.Rho(VEd * 1000, VplRd);
                 pymod = EC3.Reducedfy(py, rho);
            }
            else
            {
                 rho = 0;
                 pymod = py;
            }

            verboseOutput += ShearOutput(Av, VplRd, shearbuckling, shearinteraction, rho, pymod);

            // cross section bending resistance
            double McRd = EC3.McRd(secclass, Wypl*1000, Wyel*1000, pymod, gammaM0, out double Wyy);


            // member buckling reistance to LTB
            double Mcr = EC3.Mcr(C1, C2, E, G, Iy * Math.Pow(10, 4), Iz * Math.Pow(10, 4), Iw * Math.Pow(10, 12), It * Math.Pow(10, 4), k, kw, L, zg);
            string curve = EC3.BucklingCurveTable6pt5UKNA_DoubleSymmetricIHandHollow(h, b);
            double alphaLT = EC3.AlphaLT(curve);
            double lambdaLT = EC3.LambdaLT(Wyy, py , Mcr);
            double lambdaLT0 = 0.4; // UK NA.2.17(a) - rolled sections and hollow sections only
            double beta = 0.75; // UK NA.2.17(a) - rolled sections and hollow sections only
            double phiLT = EC3.PhiLT(alphaLT, lambdaLT, lambdaLT0, beta);
            double chiLT = EC3.ChiLT(phiLT, beta, lambdaLT);

            double kc = EC3.kcUKNA(C1);
            double f = EC3.F(kc, lambdaLT);
            double chiLTmod = EC3.ChiLTmod(chiLT, f, lambdaLT);
            double MbRd = EC3.MbRd(chiLTmod , Wyy , pymod , gammaM1);

            verboseOutput += LTBoutput(C1, C2, E, G, curve, alphaLT, lambdaLT, lambdaLT0, beta, phiLT, chiLT, kc, f, chiLTmod, MbRd);

            // also calculate using the general method
            string curveG = EC3.BucklingCurveTable6pt4RolledI(h, b);
            double alphaLTG = EC3.AlphaLT6322(curveG);
            double phiLTG = EC3.PhiLT6322(alphaLTG, lambdaLT);
            double chiLTG = EC3.ChiLT6322(phiLTG, lambdaLT);
            double MbRdG = EC3.MbRd(chiLTG, Wyy, pymod, gammaM1);

            // general output
            labelIyy.Text = Iy.ToString("F0") + " cm4";
            labelIzz.Text = Iz.ToString("F0") + " cm4";
            labelWyy.Text = (Wyy/1000).ToString("F0") + " cm3";
            
            labelClass.Text = secclass.ToString("F0");
            labelfy.Text = py.ToString("F0") + " MPa";
            labelEpsilon.Text = epsilon.ToString("F2");

            // shear output
            labelAv.Text = (Av/10/10).ToString("F0") + " cm2";
            labelShearbuckling.Text = shearbuckling.ToString();
            labelShearinteraction.Text = shearinteraction.ToString();
            labelRho.Text = rho.ToString("F2");


            labelVplRd.Text = (VplRd / 1000).ToString("F0") + " kN";
            labelFymod.Text = pymod.ToString("F0") + " MPa";


            // LTB output
            labelCurve.Text = curve;
            labelalphaLT.Text = alphaLT.ToString("F2");
            labelF.Text = f.ToString("F2");
            labelMcr.Text = (Mcr / Math.Pow(10, 6)).ToString("F0") + " kNm";
            labelMcRd.Text = (McRd / Math.Pow(10, 6)).ToString("F0") + " kNm";
            labelMbRd.Text = (MbRd / Math.Pow(10, 6)).ToString("F0") + " kNm";
            labelMbRdgeneral.Text = (MbRdG / Math.Pow(10, 6)).ToString("F0") + " kNm";
            if (shearbuckling == true)
            {
                labelVplRd.Text = "-";
                labelFymod.Text = "-";
                labelMcRd.Text = "-"; ;
                labelMbRd.Text = "-"; ;
            }

            verboseOutput += "</body></html>";
            //PrintOutput(verboseOutput);
        }

        public void PrintOutput(string html)
        {
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == "FormDetailedOutput")
                {
                    frm.Close();
                    break;
                }
            }
            FormDetailedOutput f2 = new FormDetailedOutput(html);
            f2.Show();
        }

        public string ShearOutput(double Av, double VplRd, Boolean shearbuckling, Boolean shearinteraction, double rho, double pymod)
        {
            string tmp = "<h2>Shear capacity</h2>";
            tmp += "<table>";
            tmp += "<tr><td>A<sub>v</sub></td><td>=</td><td>" + Av.ToString("F0") + " mm<sup>2</sup></td></tr>";
            tmp += "<tr><td>V<sub>pl,Rd</sub></td><td>=</td><td>" + (VplRd/1000).ToString("F0") + " kN</td></tr>";
            tmp += "<tr><td>Shear buckling check req'd?</td><td>=</td><td>" + shearbuckling.ToString() + "</td></tr>";
            tmp += "<tr><td>Shear interaction?</td><td>=</td><td>" + shearinteraction.ToString() + "</td></tr>";
            tmp += "<tr><td>&#961;</td><td>=</td><td>" + rho.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>f<sub>y,mod</sub<</td><td>=</td><td>" + pymod.ToString("F0") + " MPa</td></tr>";
            tmp += "</table>";
            return tmp;
        }

        public string LTBoutput(double C1, double C2, double E, double G,string curve, double alphaLT, double lambdaLT, double lambdaLT0, double beta, double phiLT, double chiLT, double kc, double f, double chiLTmod, double MbRd)
        {
            string tmp = "<h2>Lateral torsional buckling to EN1993-1-1 Cl.6.3.2.3</h2>";
            tmp += "<table>";
            tmp += "<tr><td>C<sub>1</sub></td><td>=</td><td>" + C1.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>C<sub>2</sub></td><td>=</td><td>" + C2.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>E</td><td>=</td><td>" + E.ToString("F0") + " MPa</td></tr>";
            tmp += "<tr><td>G</td><td>=</td><td>" + G.ToString("F0") + " MPa</td></tr>";
            tmp += "<tr><td>Curve</td><td>=</td><td>" + curve + "</td></tr>";
            tmp += "<tr><td>&#945;<sub>LT</sub></td><td>=</td><td>" + alphaLT.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#955;<sub>LT</sub></td><td>=</td><td>" + lambdaLT.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#955;<sub>LT,0</sub></td><td>=</td><td>" + lambdaLT0.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#946;</td><td>=</td><td>" + beta.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#934;<sub>LT</sub></td><td>=</td><td>" + phiLT.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#967;<sub>LT</sub></td><td>=</td><td>" + chiLT.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>k<sub>c</sub></td><td>=</td><td>" + kc.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>f</td><td>=</td><td>" + f.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>&#967;<sub>LT,mod</sub></td><td>=</td><td>" + chiLTmod.ToString("F3") + "</td></tr>";
            tmp += "<tr><td>M<sub>b,Rd</sub></td><td>=</td><td>" + (MbRd / Math.Pow(10, 6)).ToString("F0") + " kNm</td></tr>";
            tmp += "</table>";
            return tmp;
        }

        public string SectionOutput(string section, double h, double b, double tw, double tf, double r, double Iy, double Iz, double Wyel, double Wzel, double Wypl, double Wzpl, double U, double X, double Iw, double It, double A)
        {
            string tmp = "<h2>Section properties</h2>";
            tmp += "<table>";
            tmp += "<tr><td>Section</td><td>=</td><td>" + section + "</td></tr>";
            tmp += "<tr><td>h</td><td>=</td><td>" + h.ToString("F1") + " mm</td></tr>";
            tmp += "<tr><td>b</td><td>=</td><td>" + b.ToString("F1") + " mm</td></tr>";
            tmp += "<tr><td>t<sub>w</sub></td><td>=</td><td>" + tw.ToString("F2") + " mm</td></tr>";
            tmp += "<tr><td>t<sub>f</sub></td><td>=</td><td>" + tf.ToString("F2") + " mm</td></tr>";
            tmp += "<tr><td>r</td><td>=</td><td>" + r.ToString("F2") + " mm</td></tr>";
            tmp += "<tr><td>I<sub>yy</sub></td><td>=</td><td>" + Iy.ToString("F0") + " cm<sup>4</sup></td></tr>";
            tmp += "<tr><td>I<sub>zz</sub></td><td>=</td><td>" + Iz.ToString("F0") + " cm<sup>4</sup></td></tr>";
            tmp += "<tr><td>W<sub>y,el</sub></td><td>=</td><td>" + Wyel.ToString("F0") + " cm<sup>3</sup></td></tr>";
            tmp += "<tr><td>W<sub>z,el</sub></td><td>=</td><td>" + Wzel.ToString("F0") + " cm<sup>3</sup></td></tr>";
            tmp += "<tr><td>W<sub>y,pl</sub></td><td>=</td><td>" + Wypl.ToString("F0") + " cm<sup>3</sup></td></tr>";
            tmp += "<tr><td>W<sub>z,pl</sub></td><td>=</td><td>" + Wzpl.ToString("F0") + " cm<sup>3</sup></td></tr>";
            tmp += "<tr><td>U</td><td>=</td><td>" + U.ToString("F2") + "</td></tr>";
            tmp += "<tr><td>X</td><td>=</td><td>" + X.ToString("F2") + "</td></tr>";
            tmp += "<tr><td>I<sub>w</sub></td><td>=</td><td>" + Iw.ToString("F0") + " dm<sup>6</sup></td></tr>";
            tmp += "<tr><td>I<sub>t</sub></td><td>=</td><td>" + It.ToString("F0") + " cm<sup>4</sup></td></tr>";
            tmp += "<tr><td>A</sub></td><td>=</td><td>" + A.ToString("F0") + " cm<sup>2</sup></td></tr>";
            tmp += "</table>";
            return tmp;
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

        private void Button2_Click(object sender, EventArgs e)
        {
            test();
            PrintOutput(verboseOutput);
        }
    }
}
