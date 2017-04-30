using System;
using System.IO;
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
            var numberOfNodes = Convert.ToInt32(nodesBox.Text);
            var branchingRatio = Convert.ToDouble(ratioBox.Text);

            var numberOfDoors = Convert.ToInt32(noDoorsBox.Text);
            var numberOfClues = Convert.ToInt32(noCluesBox.Text);

            var randomSeed = Convert.ToInt32(seedBox.Text);
            var iterations = Convert.ToInt32(iterationBox.Text);

            var visualise = visualiseBox.Checked;

            WriteToLogfile();

            //Set seed
            Random random = new Random(randomSeed);

            if(testOptionBox.SelectedIndex == 0) {
                var graphVisualiser = new GenerateGraphAndVisualise(random);
                graphVisualiser.DoLockClueStressTest(numberOfNodes, branchingRatio, visualise);
            }
            else if(testOptionBox.SelectedIndex == 1)
            {
                var doorAndClueTest = new GenerateDoorAndClueTestAndVisualise(random);
                var unsolveableSituationFound = false;

                for (int i = 0; i < iterations; i++)
                {
                    var solvable = doorAndClueTest.DoLockClueStressTest(numberOfNodes, branchingRatio, numberOfDoors, numberOfClues, visualise);
                    if (!solvable)
                    {
                        MessageBox.Show("Map is not solvable, iteration: " + i);
                        unsolveableSituationFound = true;
                        break;
                    }
                }
                       
                if(!unsolveableSituationFound)
                    MessageBox.Show("All iterations solved");
                    
            }
            else if (testOptionBox.SelectedIndex == 2)
            {
                var doorAndClueTest = new GenerateDoorAndObjectiveTestAndVisualise(random);
                var unsolveableSituationFound = false;

                for (int i = 0; i < iterations; i++)
                {
                    var solvable = doorAndClueTest.DoLockClueStressTest(numberOfNodes, branchingRatio, numberOfDoors, numberOfClues, visualise);
                    if (!solvable)
                    {
                        MessageBox.Show("Map is not solvable, iteration: " + i);
                        unsolveableSituationFound = true;
                        break;
                    }
                }

                if (!unsolveableSituationFound)
                    MessageBox.Show("All iterations solved");

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            testOptionBox.SelectedIndex = 0;
        }

        private void WriteToLogfile()
        {
            var filename = "logfile" + LogTime(DateTime.Now) + ".txt";

            using (var stream = new StreamWriter(filename))
            {
                stream.WriteLine("random seed: " + seedBox.Text);
                stream.WriteLine("numberOfNodes: " + nodesBox.Text);
                stream.WriteLine("branchingRatio: " + ratioBox.Text);
                stream.WriteLine("numberOfDoors: " + noDoorsBox.Text);
                stream.WriteLine("numberOfClues: " + noCluesBox.Text);
            }
        }

        //Produce save dateTime string for filenames
        private string LogTime(DateTime dateTime)
        {
            string ret = dateTime.Year.ToString("0000") + "-" + dateTime.Month.ToString("00") + "-" + dateTime.Day.ToString("00") + "_" + dateTime.Hour.ToString("00") + "-" + dateTime.Minute.ToString("00") + "-" + dateTime.Second.ToString("00");
            return ret;
        }
    }
}
