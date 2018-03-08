using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace WindowsFormsApp1
{
    public partial class PictureLoader : Form
    {
        //Set Azure account key
        const string SubscriptionKey = "7b2c54a81f31425e92bb9c807b3a2953";
        public PictureLoader()
        {
            InitializeComponent();
            //Clear PictureBox
            pbImage.Image.Dispose();
            pbImage.Image = null;
            //Load new image from file
            pbImage.Image = Image.FromFile(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\puppy.jpg");
            //PictureBox size setting = adjust size of image
            pbImage.SizeMode = PictureBoxSizeMode.AutoSize;
            //Form size setting = adjust to pbImage size
            this.ClientSize = new Size(pbImage.Image.Width, pbImage.Image.Height + toolStripMenuItem1.Height + textBox1.Height);
            //Position Image to top left corner of form
            pbImage.Location = new System.Drawing.Point(0, toolStripMenuItem1.Height);
            //Position TextBox right below ImageBox
            textBox1.Location = new System.Drawing.Point(0, pbImage.Image.Height + toolStripMenuItem1.Height);
            //Readjust width of TextBox
            textBox1.Width = pbImage.Width;
        }

        private void PictureLoader_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
        }
        
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //String array of file paths dropped
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                //Pass first file path to isImage
                isImage(filePaths[0]);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                isImage(fileName);
            }
        }
        //Loads file if it is an image
        //Args: file path in a string
        private void isImage(string fileName)   
        {
            textBox1.Text = "";
            //Check if file type is an image
            if (fileName.Contains(".bmp") || fileName.Contains(".tif") || fileName.Contains(".jpg") || fileName.Contains(".gif") || fileName.Contains(".png"))
            {
                //Clear PictureBox prior to loading new image
                if(pbImage.Image != null)
                {
                    pbImage.Image.Dispose();
                    pbImage.Image = null;
                }
                //Load new file from path String fileName
                pbImage.Image = Image.FromFile(fileName);
                //Resize form size to picture size
                this.ClientSize = new Size(pbImage.Image.Width, pbImage.Image.Height + toolStripMenuItem1.Height + textBox1.Height);
                textBox1.Location = new System.Drawing.Point(0, pbImage.Image.Height + toolStripMenuItem1.Height);
                textBox1.Width = pbImage.Width;

                //Test
                Uri uri = new Uri(fileName, UriKind.Absolute);
                var done = DoWork(uri, true);
            } else {
                MessageBox.Show("File not an image of type:\n.bmp\n.tif\n.jpg\n.gif\n.png", "File Error", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            }
        }
        /// <summary>
        /// Uploads the image to Project Oxford and performs analysis
        /// </summary>
        /// <param name="imageFilePath">The image file path.</param>
        /// <returns></returns>
        private async Task<AnalysisResult> UploadAndAnalyzeImage(string imageFilePath)
        {
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Vision API Service client
            //
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");

            using (Stream imageFileStream = File.OpenRead(imageFilePath))
            {
                //
                // Analyze the image for all visual features
                //
                VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
                AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageFileStream, visualFeatures);
                return analysisResult;
            }

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------
        }

        /// <summary>
        /// Sends a url to Project Oxford and performs analysis
        /// </summary>
        /// <param name="imageUrl">The url of the image to analyze</param>
        /// <returns></returns>
        private async Task<AnalysisResult> AnalyzeUrl(string imageUrl)
        {
            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE STARTS HERE
            // -----------------------------------------------------------------------

            //
            // Create Project Oxford Vision API Service client
            //
            VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey);

            //
            // Analyze the url for all visual features
            //
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.ImageType, VisualFeature.Tags };
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(imageUrl, visualFeatures);
            return analysisResult;

            // -----------------------------------------------------------------------
            // KEY SAMPLE CODE ENDS HERE
            // -----------------------------------------------------------------------
        }
        /// <summary>
        /// Perform the work for this scenario
        /// </summary>
        /// <param name="imageUri">The URI of the image to run against the scenario</param>
        /// <param name="upload">Upload the image to Project Oxford if [true]; submit the Uri as a remote url if [false];</param>
        /// <returns></returns>
        protected async Task<int> DoWork(Uri imageUri, bool upload)
        {
            //
            // Either upload an image, or supply a url
            //
            AnalysisResult analysisResult;
            if (upload)
            {
                analysisResult = await UploadAndAnalyzeImage(imageUri.LocalPath);
            }
            else
            {
                analysisResult = await AnalyzeUrl(imageUri.AbsoluteUri);
            }

            //Show result of Computer Vision API on TextBow
            //MessageBox.Show("Some text");
            textBox1.Text = "";
            textBox1.Text = analysisResult.Color.DominantColors.ToString();
            //textBox1.Text = imageUri.ToString();
            //textBox1.Text = analysisResult.Description.ToString();

            return 1;
        }
    }
}
