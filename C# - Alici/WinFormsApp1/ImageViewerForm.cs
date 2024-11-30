using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class ImageViewerForm : Form
    {
        private PictureBox pictureBox;
        private string serverIp = " alici ip";
        private int serverPort = 8000;

        public ImageViewerForm()
        {
           
            this.Text = "Canlı Görüntü";
            this.Size = new Size(500, 500);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            
            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(pictureBox);

            
            System.Threading.Thread receiveThread = new System.Threading.Thread(ReceiveImages)
            {
                IsBackground = true
            };
            receiveThread.Start();
        }

        private void ReceiveImages()
        {
            try
            {
                using (TcpClient client = new TcpClient(serverIp, serverPort))
                {
                    Console.WriteLine("Server'a baglandi.");
                    NetworkStream stream = client.GetStream();

                    while (!this.IsDisposed)
                    {
                       
                        byte[] sizeBuffer = new byte[4];
                        int readBytes = stream.Read(sizeBuffer, 0, sizeBuffer.Length);
                        if (readBytes == 0) break;

                        int imageSize = BitConverter.ToInt32(sizeBuffer, 0);

                        if (imageSize < 0 || imageSize > 10 * 1024 * 1024)
                        {
                            Console.WriteLine("Geçersiz image alindi, atlaniyor...");
                            continue;
                        }

                        
                        byte[] imageBuffer = new byte[imageSize];
                        int bytesRead = 0;

                        while (bytesRead < imageSize)
                        {
                            int read = stream.Read(imageBuffer, bytesRead, imageSize - bytesRead);
                            if (read == 0) break;
                            bytesRead += read;
                        }

                        try
                        {
                            using (MemoryStream ms = new MemoryStream(imageBuffer))
                            {
                                Image receivedImage = Image.FromStream(ms);
                                UpdatePictureBox(receivedImage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"image islenirken hata olustu: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"baglanti hatasi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePictureBox(Image image)
        {
            if (this.IsDisposed) return;

            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action<Image>(UpdatePictureBox), image);
                return;
            }

           
            if (pictureBox.Image != null)
            {
                var oldImage = pictureBox.Image;
                pictureBox.Image = null;
                oldImage.Dispose();
            }

            pictureBox.Image = image;
        }
    }
}