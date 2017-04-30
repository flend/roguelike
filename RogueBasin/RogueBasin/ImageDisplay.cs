using System.Windows.Forms;

namespace RogueBasin
{
    public partial class ImageDisplay : Form
    {
        public ImageDisplay()
        {
            InitializeComponent();

            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        }

        public void AssignImage(string filename) {
            pictureBox1.ImageLocation = filename;
        }

    }
}
