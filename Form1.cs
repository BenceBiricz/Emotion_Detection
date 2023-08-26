using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Structure;
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

namespace Emotion_Detection
{
    public partial class Form1 : Form
    {
        static readonly CascadeClassifier cascade_face = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        OpenFileDialog openfile;
        Bitmap bmp, bmp1, bmp_cut;

        //------------Dataset----------------------
        List<FaceData> DataSet = new List<FaceData>();
        List<FaceData> DataSetForSample = new List<FaceData>();
        List<FaceData> TrainingData;
        List<FaceData> TestingData;
        Matrix<float> x_train = null, x_test = null;
        Matrix<int> y_train = null, y_test = null;
        SVM svmModel;


        Rectangle cropArea;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //------------Open file----------------------
            openfile = new OpenFileDialog();
            openfile.FileName = "";
            openfile.Filter = "All files (*.*)|*.*";

            if (openfile.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = Image.FromFile(openfile.FileName);
                bmp = new Bitmap(pictureBox1.Image);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //------------Draw rectangle around face----------------------
            bmp1 = new Bitmap(bmp);
            Image<Bgr, byte> bgrImage = new Image<Bgr, byte>(bmp1);
            Rectangle[] rectangles = cascade_face.DetectMultiScale(bgrImage, 1.4, 1);
            foreach (Rectangle rectangle in rectangles)
            {
                Graphics graphics = Graphics.FromImage(bmp1);
                Pen pen = new Pen(Color.Red, 0);
                graphics.DrawRectangle(pen, rectangle);
                cropArea = rectangle;
            }
            pictureBox1.Image = bmp1;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //------------Cut area around rectangle----------------------
            bmp_cut = new Bitmap(bmp1);
            bmp_cut = bmp_cut.Clone(cropArea, bmp_cut.PixelFormat);
            pictureBox2.Image = bmp_cut;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            //------------Load Dataset from directorz----------------------
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;

                var files = Directory.GetFiles(dialog.SelectedPath);
                foreach (var file in files)
                {
                    var img = new Image<Gray, byte>(file).Resize(256, 256, Inter.Cubic);

                    //------------Draw rectangle around face----------------------
                    bmp1 = img.ToBitmap();
                    Bitmap tempBitmap = new Bitmap(bmp1.Width, bmp1.Height);

                    Image<Gray, byte> bgrImage = new Image<Gray, byte>(bmp1);
                    Rectangle[] rectangles = cascade_face.DetectMultiScale(bgrImage, 1.1, 10, Size.Empty, Size.Empty);
                    foreach (Rectangle rectangle in rectangles)
                    {
                        Graphics graphics = Graphics.FromImage(tempBitmap);
                        Pen pen = new Pen(Color.Red, 0);
                        graphics.DrawRectangle(pen, rectangle);
                        cropArea = rectangle;
                    }

                    //------------Cut area around rectangle----------------------
                    bmp_cut = new Bitmap(bmp1);
                    bmp_cut = bmp_cut.Clone(cropArea, bmp_cut.PixelFormat);
                    img = new Image<Gray, byte>(bmp1).Resize(256, 256, Inter.Cubic);


                    var name = Path.GetFileName(file);
                    int emotion = int.Parse(name.Substring(name.IndexOf(".") - 1, 1));
                    var index = DataSet.FindIndex(x => x.Emotion == emotion);
                    if (index > -1)
                    {
                        DataSet[index].Images.Add(img);
                        //textBox1.Text += ", " + emotion;
                    }
                    else
                    {
                        FaceData face = new FaceData();
                        face.Images = new List<Image<Gray, byte>>();
                        face.Images.Add(img);
                        face.Emotion = emotion;
                        DataSet.Add(face);
                        //textBox1.Text += ", " + emotion;
                    }
                }
                textBox1.Text = "Data loaded" + +DataSet.Count();
                Cursor = Cursors.Default;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //------------Split images to test and train----------------------
            (TrainingData, TestingData) = Helper.TestTrainSplit(DataSet);
            textBox1.Text = "Test-Train Split done.";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //------------Get HoG features----------------------
            Cursor = Cursors.WaitCursor;
            (x_train, y_train) = CalculateFeatures(TrainingData);
            textBox1.Text = "Training: Hog extracted.";
            Cursor = Cursors.Default;
        }

        private (Matrix<float>, Matrix<int>) CalculateFeatures(List<FaceData> data)
        {
            //------------HoG features----------------------
            HOGDescriptor hog = new HOGDescriptor(new Size(256, 256), new Size(32, 32), new Size(16, 16), new Size(8, 8));

            List<float[]> hogfeatures = new List<float[]>();
            List<int> emotions = new List<int>();

            foreach (var item in data)
            {
                foreach (var img in item.Images)
                {
                    //------------Compute HoG features----------------------
                    var features = hog.Compute(img);
                    hogfeatures.Add(features);
                    emotions.Add(item.Emotion);
                }
            }
            //------------Save features----------------------
            var xtrain = new Matrix<float>(Helper.To2D<float>(hogfeatures.ToArray()));
            var ytrain = new Matrix<int>(emotions.ToArray());
            return (xtrain, ytrain);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //------------Train SVM model----------------------

            Cursor = Cursors.WaitCursor;
            svmModel = new SVM();
            if (File.Exists("face_svm"))
            {
                svmModel.Load("face_svm");
                textBox1.Text = "Trained Model Loaded.";
            }
            else
            {
                svmModel.SetKernel(SVM.SvmKernelType.Rbf);
                svmModel.Type = SVM.SvmType.CSvc;
                svmModel.TermCriteria = new MCvTermCriteria(10000, 0.000001);
                svmModel.C = 250;
                svmModel.Gamma = 0.001;

                TrainData traindata = new TrainData(x_train, DataLayoutType.RowSample, y_train);

                if (svmModel.Train(traindata))
                {
                    svmModel.Save("face_svm");
                    textBox1.Text = "Model Trained & Saved.";
                }

            }

            Cursor = Cursors.Default;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //------------Test the picture emotion----------------------
            List<FaceData> SampleData;

            List<FaceData> sample = new List<FaceData>();
            FaceData sample1 = new FaceData();
            sample1.Images = new List<Image<Gray, byte>>();
            Image<Gray, byte> image = new Image<Gray, byte>(bmp_cut).Resize(256, 256, Inter.Cubic);
            sample1.Images.Add(image);
            sample1.Emotion = int.Parse(openfile.FileName.Substring(openfile.FileName.IndexOf(".") - 1, 1));
            sample.Add(sample1);
            SampleData = Helper.TestSample(sample);
            (x_test, y_test) = CalculateFeatures(SampleData);


            int prediction = (int)svmModel.Predict(x_test.GetRow(0));

            //------------Predicted emotion----------------------
            if (prediction == 1)
            {
                textBox1.Text = "Predicted: Happy";
            }
            else if (prediction == 2)
            {
                textBox1.Text = "Predicted: Normal";
            }
            else if (prediction == 3)
            {
                textBox1.Text = "Predicted: Sad";
            }
            else if (prediction == 4)
            {
                textBox1.Text = "Predicted: Sleepy";
            }
            else if (prediction == 5)
            {
                textBox1.Text = "Predicted: Suprised";
            }

            //------------Actual emotion----------------------
            if (sample1.Emotion == 1)
            {
                textBox1.Text += "\r\nActual: Happy";
            }
            else if (sample1.Emotion == 2)
            {
                textBox1.Text += "\r\nActual: Normal";
            }
            else if (sample1.Emotion == 3)
            {
                textBox1.Text += "\r\nActual: Sad";
            }
            else if (sample1.Emotion == 4)
            {
                textBox1.Text += "\r\nActual: Sleepy";
            }
            else if (sample1.Emotion == 5)
            {
                textBox1.Text += "\r\nActual: Suprised";
            }

        }
    }
}
