using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeamUtilisation
{
    public partial class FormDetailedOutput : Form
    {
        public FormDetailedOutput(string html)
        {
            InitializeComponent();
            webBrowser1.DocumentText = html;
        }
    }
}
