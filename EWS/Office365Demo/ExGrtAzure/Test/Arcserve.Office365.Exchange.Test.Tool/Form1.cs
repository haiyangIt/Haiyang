using Arcserve.Exchange.FastTransferUtil.CompoundFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arcserve.Office365.Exchange.Test.Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnChooseBin_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtBinPath.Text = openFileDialog1.FileName;
            }
        }

        private void btnChooseMsg_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                txtMsgPath.Text = fbd.SelectedPath;
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtBinPath.Text))
                {
                    OpenFileDialog openFileDialog1 = new OpenFileDialog();

                    openFileDialog1.InitialDirectory = "c:\\";
                    openFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
                    openFileDialog1.FilterIndex = 2;
                    openFileDialog1.RestoreDirectory = true;

                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        txtBinPath.Text = openFileDialog1.FileName;
                    }
                }

                if (string.IsNullOrEmpty(txtMsgPath.Text))
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    DialogResult dr = fbd.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        txtMsgPath.Text = fbd.SelectedPath;
                    }
                }

                if (File.Exists(txtBinPath.Text))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        byte[] buffer = new byte[1024];
                        using (FileStream fileStream = new FileStream(txtBinPath.Text, FileMode.Open))
                        {
                            int count = fileStream.Read(buffer, 0, 1024);
                            while (count > 0)
                            {
                                stream.Write(buffer, 0, count);
                                count = fileStream.Read(buffer, 0, 1024);
                            }
                        }
                        string msgName = Path.GetFileNameWithoutExtension(txtBinPath.Text);
                        string msgPath = Path.Combine(txtMsgPath.Text, string.Format("{0}_{1}.msg", msgName, DateTime.Now.Ticks));
                        using (FileStream fileStream = new FileStream(msgPath, FileMode.CreateNew))
                        {
                            CompoundFileUtil.Instance.ConvertBinToMsg(stream, fileStream);
                            MessageBox.Show("Generate Finished.");
                        }

                    }


                }
                else
                {
                    MessageBox.Show(string.Format("{0} is not exist.", txtBinPath.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnBinToTxt_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtBinPath.Text))
                {
                    OpenFileDialog openFileDialog1 = new OpenFileDialog();

                    openFileDialog1.InitialDirectory = "c:\\";
                    openFileDialog1.Filter = "bin files (*.bin)|*.bin|All files (*.*)|*.*";
                    openFileDialog1.FilterIndex = 2;
                    openFileDialog1.RestoreDirectory = true;

                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        txtBinPath.Text = openFileDialog1.FileName;
                    }
                }

                if (string.IsNullOrEmpty(txtMsgPath.Text))
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    DialogResult dr = fbd.ShowDialog();

                    if (dr == DialogResult.OK)
                    {
                        txtMsgPath.Text = fbd.SelectedPath;
                    }
                }

                if (File.Exists(txtBinPath.Text))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        string txtName = Path.GetFileNameWithoutExtension(txtBinPath.Text);
                        string txtPath = Path.Combine(txtMsgPath.Text, string.Format("{0}_{1}.txt", txtName, DateTime.Now.Ticks));
                        string split = "\t";
                        int readIndex = 0;

                        byte[] buffer = new byte[1024];
                        using (StreamWriter writer = new StreamWriter(txtPath))
                        {
                            writer.Write("Num");
                            writer.Write(split);
                            writer.Write("Hex");
                            writer.Write(split);
                            writer.Write("Dec");
                            writer.Write(split);
                            writer.Write("Ch");
                            writer.WriteLine();

                            using (FileStream fileStream = new FileStream(txtBinPath.Text, FileMode.Open))
                            {

                                int count = fileStream.Read(buffer, 0, 1024);
                                while (count > 0)
                                {
                                    stream.Write(buffer, 0, count);
                                    count = fileStream.Read(buffer, 0, 1024);
                                    for (int index = 0; index < count; index++)
                                    {
                                        var b = buffer[index];
                                        writer.Write(readIndex++);
                                        writer.Write(split);
                                        writer.Write(b.ToString("X2"));
                                        writer.Write(split);
                                        writer.Write((int)b);
                                        writer.Write(split);
                                        writer.Write((char)b);
                                        writer.WriteLine();
                                    }
                                }
                            }
                        }
                    }
                    MessageBox.Show("Convert Success.");
                }
                else
                {
                    MessageBox.Show(string.Format("{0} is not exist.", txtBinPath.Text));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace));
            }
        }
    }
}
