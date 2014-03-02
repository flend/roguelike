using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphMapStressTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            var lockClueStressTest = new GenerateGraphAndVisualise();

            var numberOfNodes = Convert.ToInt32(nodesBox.Text);
            var branchingRatio = Convert.ToDouble(ratioBox.Text);

            lockClueStressTest.DoLockClueStressTest(numberOfNodes, branchingRatio);
        }
    }
}
