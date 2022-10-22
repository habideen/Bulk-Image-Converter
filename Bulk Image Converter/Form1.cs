using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Bulk_Image_Converter
{
    public partial class Form1 : Form
    {
        private float progress_shown = 0;
        private float progress_shown_next = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void selectSourceBtn_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                inputLocationTxt.Text = dialog.FileName;
            }
            else
            {
                inputLocationTxt.Text = "";
            }
        }

        private void outputLocationBtn_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                outputLocationTxt.Text = dialog.FileName;
            }
            else
            {
                outputLocationTxt.Text = "";
            }
        }

        private async void countInputBtn_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@inputLocationTxt.Text))
            {
                MessageBox.Show("Source folder is invalid!", "Error", MessageBoxButtons.OK);
                return;
            }

            if (inputTypeCmb.Text == String.Empty)
            {
                MessageBox.Show("Please select input file format!", "Error", MessageBoxButtons.OK);
                inputTypeCmb.Focus();
                return;
            }

            countInputBtn.Enabled = false;
            await Task.Run(() => countFiles(@inputLocationTxt.Text));
            countInputBtn.Enabled = true;

        } //countInputBtn_Click


        private void countFiles(String folder)
        {
            Invoke(new Action(() =>
            {
                inputCountLbl.Text = Directory.GetFiles(folder, "*."+inputTypeCmb.Text, SearchOption.TopDirectoryOnly).Length.ToString();
            }));
        } //countFiles



        private async void convertBtn_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@inputLocationTxt.Text))
            {
                MessageBox.Show("Source folder is invalid!", "Error", MessageBoxButtons.OK);
                return;
            }

            if (inputTypeCmb.Text == String.Empty)
            {
                MessageBox.Show("Please select input file format!", "Error", MessageBoxButtons.OK);
                inputTypeCmb.Focus();
                return;
            }

            //check output criteria
            if (!Directory.Exists(@outputLocationTxt.Text))
            {
                MessageBox.Show("Output folder is invalid!", "Error", MessageBoxButtons.OK);
                return;
            }

            if (outputTypeCmb.Text == String.Empty)
            {
                MessageBox.Show("Please select output file format!", "Error", MessageBoxButtons.OK);
                outputTypeCmb.Focus();
                return;
            }


            await Task.Run(() => countFiles(@inputLocationTxt.Text));



            if (Int64.Parse(inputCountLbl.Text) == 0)
            {
                MessageBox.Show("There are no files in the source folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            String[] files = Directory.GetFiles(@inputLocationTxt.Text, "*." + inputTypeCmb.Text, SearchOption.TopDirectoryOnly);
            
            outputCountLbl.Text = "0";

            
            progress_shown = 100 / Int64.Parse(inputCountLbl.Text);
            progress_shown_next = 0;


            progressBar1.Value = 0;
            progressBar1.Visible = true;


            convertBtn.Enabled = false;

            foreach (String file in files)
            {
                if (File.Exists(file))
                {
                    progress_shown_next += progress_shown; //increase progressbar value

                    await Task.Run(() => convertFile(@file, @outputLocationTxt.Text));
                }                
            }

            progressBar1.Visible = false;

            convertBtn.Enabled = true;

            GC.Collect();
            GC.WaitForPendingFinalizers();

        } //convertBtn_Click


        private void convertFile(String file, String outputLocationTxt)
        {
            Invoke(new Action(() =>
            {
                Image img = Image.FromFile(file);

                string newName = System.IO.Path.GetFileNameWithoutExtension(file);
                newName = "10" + newName + "." + outputTypeCmb.Text; // append 10 to the account names
                newName = outputLocationTxt + "\\" + newName;

                
                if (outputTypeCmb.Text == "bmp")
                {
                    img.Save(newName, ImageFormat.Bmp);
                }
                else if (outputTypeCmb.Text == "jpg")
                {
                    img.Save(newName, ImageFormat.Jpeg);
                } 
                else if (outputTypeCmb.Text == "png")
                {
                    img.Save(newName, ImageFormat.Png);
                }


                outputCountLbl.Text = (Int64.Parse(outputCountLbl.Text) + 1).ToString();
                progressBar1.Value = (int) progress_shown_next;

                System.Threading.Thread.Sleep(200);
            }));
        }//convertFiles
    }
}
