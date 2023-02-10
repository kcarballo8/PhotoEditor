using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditorSimple
{
    public partial class Transform : Form
    {
        public bool cancelButtonClicked = false;
        private CancellationTokenSource cancellationTokenSource;


        public Transform()
        {
            
            InitializeComponent();
            
        }

        public ProgressBar progressBarAccess
        {
            get { return this.progressBar1; }
            set { this.progressBar1 = value; }
        }

        public void CancelButton_Click(object sender, EventArgs e)
        {
            cancelButtonClicked = true;
            this.Close();
        }

        public void progressBar1_Click(object sender, EventArgs e)
        {
       
        }

        private void Transform_Load(object sender, EventArgs e)
        {
            cancelButtonClicked = false;
        }

        private void CancelButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.F4)
            {
                cancelButtonClicked = true;
                this.Close();
            }
        }

        private void Transform_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Alt && e.KeyCode == Keys.F4)
            {
                cancelButtonClicked = true;
                this.Close();
            }

        }
    }
}
