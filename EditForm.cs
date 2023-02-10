using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace PhotoEditorSimple
{
    public partial class EditForm : Form
    {
        private string path;
        Transform transform;
        

        public EditForm(string imagePath)
        {
            InitializeComponent();
            pictureBox1.Image = LoadImage(imagePath);
            path = imagePath;
            SaveButton.Enabled = false;
        }

        private Image LoadImage(string filename)
        {
            // Use this method to load images so the image files do not remain locked
            byte[] bytes = File.ReadAllBytes(filename);
            MemoryStream ms = new MemoryStream(bytes);
            return Image.FromStream(ms);
        }

        private async Task transformImage(string option)
        {
            transform = new Transform();
     

            Bitmap transformedBitmap = new Bitmap(pictureBox1.Image);
            if (option == "Invert")
            {
                transform.Show();
                this.Enabled = false;
                await Task.Run(() =>
                {
                    InvertColors(transformedBitmap);
                });
            }
            else if (option == "Color")
            {

                ColorDialog colorDia = new ColorDialog();
               

                if (colorDia.ShowDialog() == DialogResult.OK) {
                    Color colorChoosen = colorDia.Color;
                transform.Show();
                this.Enabled = false;

                await Task.Run(() =>
                {
                    AlterColors(transformedBitmap, colorChoosen);
                });
            }
            }
            else if (option == "Brightness")
            {
                int brightnessv = Brightness.Value;

                transform.Show();
                this.Enabled = false;
                await Task.Run(() =>
                {
                    ChangeBrightness(transformedBitmap, brightnessv);
                });
            }

            transform.Close();            //close the progress dialog box
            this.Enabled = true;//enable the edit form and bring it to the front
            this.BringToFront();
            if (transform.cancelButtonClicked == false)//if the task wasn't cancelled then pictureBox's image=transformedBitmap
            {
                pictureBox1.Image = transformedBitmap;
                SaveButton.Enabled = true;
            }

        }

       
        private async void Invert_Click(object sender, EventArgs e)
        {
            /* //transform.CancelButton = ;
             Bitmap transformedBitmap = new Bitmap(pictureBox1.Image);
             var transform = new Transform();
             transform.Show();
             await InvertColors(transformedBitmap);*/
            await transformImage("Invert");
        }

        private void InvertColors(Bitmap transformedBitmap)
        {
            // int num = 0;
            for (int y = 0; y < transformedBitmap.Height; y++)
            {
                for (int x = 0; x < transformedBitmap.Width; x++)
                {
                    var color = transformedBitmap.GetPixel(x, y);
                    int newRed = Math.Abs(color.R - 255);
                    int newGreen = Math.Abs(color.G - 255);
                    int newBlue = Math.Abs(color.B - 255);
                    Color newColor = Color.FromArgb(newRed, newGreen, newBlue);
                    transformedBitmap.SetPixel(x, y, newColor);
                }

                if (y % (transformedBitmap.Height / 100) == 0)
                {
                    Invoke((Action)delegate ()
                    {
                        if (transform.progressBarAccess.Value != 100)
                        {
                            transform.progressBarAccess.Value += 1;
                        }
                    });
                }
                // num++;

            }
        }

        private async void ColorChangeButton_Click(object sender, EventArgs e)
        {
            /* this.Enabled = false;
             var imageB = new Bitmap(pictureBox1.Image);
             var transform = new Transform();
             if ( colorDialog1.ShowDialog() == DialogResult.OK)
             {
                 await AlterColors(imageB, colorDialog1.Color);
             }
             this.Enabled = true; */
            await transformImage("Color");
        }

        // Tints image with chosen color
        private void AlterColors(Bitmap transformedBitmap, Color chosenColor)
        {    
            for (int y = 0; y < transformedBitmap.Height; y++)
            {
                for (int x = 0; x < transformedBitmap.Width; x++)
                {
                    var color = transformedBitmap.GetPixel(x, y);
                    int ave = (color.R + color.G + color.B) / 3;
                    double percent = ave / 255.0;
                    int newRed = Convert.ToInt32(chosenColor.R * percent);
                    int newGreen = Convert.ToInt32(chosenColor.G * percent);
                    int newBlue = Convert.ToInt32(chosenColor.B * percent);
                    var newColor = Color.FromArgb(newRed, newGreen, newBlue);
                    transformedBitmap.SetPixel(x, y, newColor);
                }

                if (y % (transformedBitmap.Height / 100) == 0)
                {
                    Invoke((Action)delegate ()
                    {
                        if (transform.progressBarAccess.Value != 100)
                        {
                            transform.progressBarAccess.Value += 1;
                        }
                    });
                }
            }
        }
    

        private  void ChangeBrightness(Bitmap transformedBitmap, int brightness)
        {
            int amount = Convert.ToInt32(2 * (50 - brightness) * 0.01 * 255);

            // Calculate amount to change RGB

            for (int y = 0; y < transformedBitmap.Height; y++)
            {
                for (int x = 0; x < transformedBitmap.Width; x++)
                {
                    var color = transformedBitmap.GetPixel(x, y);
                    int newRed = Math.Max(0, Math.Min(color.R - amount, 255));
                    int newGreen = Math.Max(0, Math.Min(color.G - amount, 255));
                    int newBlue = Math.Max(0, Math.Min(color.B - amount, 255));
                    var newColor = Color.FromArgb(newRed, newGreen, newBlue);
                    transformedBitmap.SetPixel(x, y, newColor);
                }

                if (y % (transformedBitmap.Height / 100) == 0)
                {
                    Invoke((Action)delegate ()
                    {
                        if (transform.progressBarAccess.Value != 100)
                        {
                            transform.progressBarAccess.Value += 1;
                        }
                    });
                }
                
            }
        }

        private async void Brightness_MouseUp(object sender, MouseEventArgs e)
        {
            /* this.Enabled = false;
             var imageB = new Bitmap(pictureBox1.Image);
             var transform = new Transform();
             await Task.Run(() =>
             {
                 ChangeBrightness(imageB, Brightness.Value);
             });
             this.Enabled = true;*/
            await transformImage("Brightness");
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

     
        private void SaveImage_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save(path, ImageFormat.Jpeg);
            Close();
        }
        private void SaveAs_Click(object sender, EventArgs e)
        {
            //code from https://www.c-sharpcorner.com/uploadfile/mahesh/savefiledialog-in-C-Sharp/
            SaveFileDialog SaveAs = new SaveFileDialog();
            SaveAs.InitialDirectory = path;
            SaveAs.Filter = "JPeg Image | *.jpg";
            SaveAs.Title = "Save Image as";
            if (SaveAs.ShowDialog() == DialogResult.OK)
            {
                path = SaveAs.FileName;
                pictureBox1.Image.Save(path, ImageFormat.Jpeg);
            }
            Close();
        }

    }
}
