﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Xml.Schema;

namespace CannyEdgeDetectionCSharp
{
    public partial class Mainform : Form
    {
        public Mainform()
        {
            InitializeComponent();
        }

        Canny _cannyData;
        List<double[]> _reducingList;

        private int _n;
        private OpenFileDialog _ofd;
        private int LengthBetweenObjects;
        private int LengthBetweenPoints;
        //private double CorrelationLimit = 0.3;
        private bool _endObj;
        private double[] _nextPoint = new double[3];
        private double[] _beginPoint = new double[2];
        private int _simpleI;
        private List<List<double[]>> _objectsMassive;
        public List<List<List<double[]>>> AllApproximObjects;
        private Bitmap _approxBit;
        private Bitmap _inputImage;
        private String _fileName;
        //public int CountForFoundObjects=0;
        //public Bitmap OneSmallObject;
        public List<string[]> Names;
        public string pathForComparsion;
        public string pathForComparsion2;
        public string pathForComparsion3;
        public string comparsionImg;
        public double pathForComparsionTxt;
        public double pathForComparsionTxt2;
        public double pathForComparsionTxt3;
        public Bitmap bitForComparsion;
        public double NearestP;//how much points find in begining
        public int I;
        public int linesCount;
        public List<List<double[]>> TempArray = new List<List<double[]>>();
        public string pathBmp;
        public double _correlationConst = 0.5;

        public List<double> libar1;
        public List<double> libar2;
        public List<double> inpAr; 

        public int iterat = 0;


        public List<double[]> resultArray;
        //private bool _flagForSteps=true;

        private readonly List<double> _correlationList = new List<double>();
        private readonly List<double[]> _regressionList = new List<double[]>();


        ///////////////////////////////////////////
        public string inputFileName;
        public string pathForCircuit="C:\\1\\kontur.bmp";
        

        //private List<double[]> _approxNPoints;
        //private List<List<double[]>> _approxObject;

        private void ClickOpen(object sender, EventArgs e)
        {
            _ofd = new OpenFileDialog
            {
                Filter =
                    @"PNG files (*.png)|*.png| Bitmap files (*.bmp)|*.bmp|TIFF files (*.tif)|*tif|JPEG files (*.jpg)|*.jpg",
                FilterIndex = 4,
                RestoreDirectory = true
            };

            if (_ofd.ShowDialog() != DialogResult.OK) return;
            try
            {
                _inputImage = new Bitmap(_ofd.FileName);
                pictureBox1.Image = _inputImage;
                _fileName = Path.GetFileNameWithoutExtension(_ofd.FileName);

            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Step1Canny(object sender, EventArgs e)
        {
            LengthBetweenObjects = Convert.ToInt32(textBox2.Text);
            LengthBetweenPoints = Convert.ToInt32(textBox3.Text);

            _reducingList = new List<double[]>();
            var th = float.Parse(TxtTH.Text);
            var tl = float.Parse(TxtTL.Text);
            _n = int.Parse(textBox1.Text);

            const int maskSize = 5;
            const float sigma = 1;
            try
            {
                _cannyData = new Canny(_inputImage, th, tl, maskSize, sigma);
                CannyEdges.Image = _cannyData.DisplayImage(_cannyData.EdgeMap);
                new Bitmap(CannyEdges.Image).Save(pathForCircuit);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(@"Выберите изображение");
                return;
            }

            for (var i = 0; i < _cannyData.EdgeMap.GetLength(0); i++)
                for (var j = 0; j < _cannyData.EdgeMap.GetLength(1); j++)
                {
                    if (_cannyData.EdgeMap[i, j] > 0)
                        _reducingList.Add(new[] { i, (double)j });
                }

            CheckCreateDirectories();
            _beginPoint = GetMaxLength(_reducingList, ShapeCenter());
        }


        
        private void Step2Circuit(object sender, EventArgs e)
        {
            AllApproximObjects = new List<List<List<double[]>>>();
            _objectsMassive = new List<List<double[]>>();

            try
            {
                while (true)
                {
                    if (_reducingList.Count >= _n)
                    {
                        resultArray = new List<double[]>();
                        _nextPoint = _beginPoint;
                        AllApproximObjects.Add(GetOneObject());
                    }
                    else
                    {
                        MessageBox.Show(@"Сканирование завершено");
                        break;
                    }
                }
            }
            catch (NullReferenceException)
            {
                if (_inputImage == null)
                {
                    MessageBox.Show(@"Выберите изображение");
                    return;
                }
                if (CannyEdges.Image == null)
                {
                    MessageBox.Show(@"Обработайте изображение");
                }
            }
        }


        private void Step3Identify(object sender, EventArgs e)
        {
            if (_inputImage == null)
            {
                MessageBox.Show(@"Выберите изображение");
                return;
            }
            if (CannyEdges.Image == null)
            {
                MessageBox.Show(@"Обработайте изображение");
                return;
            }
            var identify = new identifyForm { Owner = this };
            identify.ShowDialog();

        }



        private void Step4Comparsion(object sender, EventArgs e)
        {
            var result = new List<Tuple<double, double, double, string>>();
            inpAr = new List<double>();
            if (iterat >= Names.Count)
            {
                MessageBox.Show("Элементы кончились");
                return;
            }
            var q = new StreamReader(Names[iterat][4]);
            comparsionImg = Names[iterat][0];
            string doubles;
            while ((doubles = q.ReadLine()) != null)
            {
                inpAr.Add(Convert.ToDouble(doubles));
            }

            var directories = Directory.GetDirectories(@"2\");
            foreach (var name in directories)
            {
                var files = Directory.GetFiles(name + "\\", "*.txt");
                foreach (var file in files)
                {
                    // MessageBox.Show(file);
                    var mins = new List<double>();
                    libar1 = new List<double>();
                    libar2 = new List<double>();
                    var t = new StreamReader(file);
                    string dbls;
                    while ((dbls = t.ReadLine()) != null)
                    {
                        libar1.Add(Convert.ToDouble(dbls));
                        libar2.Add(Convert.ToDouble(dbls));
                    }
                    for (var i = 2; i < inpAr.Count; i++)
                    {
                        mins.Add(equalTwoArrays(new[] { inpAr[i - 2], inpAr[i - 1], inpAr[i] }));
                    }
                    var total = mins.Sum();

                    mins = new List<double>();
                    for (var i = 2; i < libar2.Count; i++)
                    {
                        mins.Add(equalTwoArraysRevert(new[] { libar2[i - 2], libar2[i - 1], libar2[i] }));
                    }
                    var total2 = mins.Sum();
                    t.Close();
                    result.Add(new Tuple<double, double, double, string>(total + total2, total, total2, file));
                }
            }

            if (result.Count == 0)
            {
                MessageBox.Show(@"В библиотеке нет элементов");
                return;
            }

            result.Sort();
            var txtFile = new StreamWriter(@"1\" + _fileName + "\\" + "outCopmare" + iterat + ".txt");
            pathForComparsion = result[1].Item4;
            pathForComparsion = pathForComparsion.Substring(0, pathForComparsion.Length - 4);
            pathForComparsion += ".bmp";
            pathForComparsionTxt = result[1].Item1;

            pathForComparsion2 = result[2].Item4;
            pathForComparsion2 = pathForComparsion2.Substring(0, pathForComparsion2.Length - 4);
            pathForComparsion2 += ".bmp";
            pathForComparsionTxt2 = result[2].Item1;

            pathForComparsion3 = result[3].Item4;
            pathForComparsion3 = pathForComparsion3.Substring(0, pathForComparsion3.Length - 4);
            pathForComparsion3 += ".bmp";
            pathForComparsionTxt3 = result[3].Item1;

            draw();

            foreach (var line in result)
            {
                txtFile.WriteLine(line.Item1 + @" " + line.Item2 + @" " + line.Item3 + @" " + line.Item4);
            }
            txtFile.Close();
            iterat++;



            var comparsion = new ComparsionForm() { Owner = this };
            comparsion.ShowDialog();
        }


        private List<double[]> getNNearestPoints(double[] point)
        {
            var npoints = new List<double[]>();
            var pointLength = new List<double[]>();
            pointLength.AddRange(_reducingList.Select(t1 => new[] { t1[0], t1[1], get_length(point[0], t1[0], point[1], t1[1]) }));
            pointLength.Sort((x, y) => x[2].CompareTo(y[2]));
            for (var i = 1; i < NearestP+1; i++)
            {
                npoints.Add(pointLength[i]);
            }
            return npoints;
        }


        private List<List<double[]>> GetOneObject()
        {
            while (true)
            {
                while(resultArray.Count<_n)
                {
                    var pointsLength = new List<double[]>();
                    foreach (var elem in _reducingList)
                    {
                        pointsLength.Add(new[]
                        {
                            elem[0], elem[1], get_length(_nextPoint[0], elem[0], _nextPoint[1], elem[1])
                        });
                    }
                    pointsLength.Sort((x, y) => x[2].CompareTo(y[2]));
                    FuckingEquals(new []{pointsLength[0][0], pointsLength[0][1]});

                    if (_reducingList.Count < _n)
                    {
                        return _objectsMassive.Count <= linesCount ? null : _objectsMassive;
                    }


                    TempArray.Add(new List<double[]>());
                    for (var j = 1; j < _n + 1; j++)
                    {
                        TempArray[TempArray.Count-1].Add(pointsLength[j]);
                    }

                    var min = new[] {0, 0, Double.PositiveInfinity};
                    foreach (var minArray in TempArray)
                    {
                        foreach (var elem in minArray)
                        {
                            if (elem[2] < min[2])
                            {
                                min = elem;
                                break;
                            }
                        }
                    }
                    FuckingEquals(min, TempArray);
                    FuckingEquals1(new[] { min[0], min[1] });

                    if (min[2] < LengthBetweenPoints)
                    {
                        _nextPoint = min;
                        resultArray.Add(min);
                    }
                    else if (min[2] >= LengthBetweenPoints && min[2] <= LengthBetweenObjects)
                    {
                        TempArray.Clear();
                        _nextPoint = min;
                    }
                    else if (min[2] >= LengthBetweenObjects && !_endObj)
                    {
                        TempArray.Clear();
                        _endObj = true;
                        _nextPoint = _beginPoint;
                    }
                    else if (min[2] >= LengthBetweenObjects && _endObj)
                    {
                        TempArray.Clear();
                        _endObj = false;
                        _beginPoint = min;
                        if (_objectsMassive.Count <= linesCount)
                        {
                            _objectsMassive.Clear();
                            _nextPoint = min;
                            resultArray.Clear();
                            continue;
                        }
                        DrawApproxBit();
                        pathBmp = "C:\\1\\"+ I+".bmp";
                        _approxBit.Save(pathBmp);
                        I++;
                        return _objectsMassive;
                    }
                }
                _objectsMassive.Add(new List<double[]>());
                foreach (double[] t in resultArray)
                {
                    _objectsMassive[_objectsMassive.Count - 1].Add(t);
                }
                //_objectsMassive.Add(resultArray);
                _nextPoint = new[] { resultArray[resultArray.Count - 1][0], resultArray[resultArray.Count - 1][1] };
                resultArray.Clear();
            }
        }


        private double[] ShapeCenter()
        {
            try
            {
                double sumx = 0, sumy = 0;
                foreach (var t in _reducingList)
                {
                    sumx += t[0];
                    sumy += t[1];
                }
                return new[] {sumx/_reducingList.Count, sumy/_reducingList.Count};
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(@"массив точек пуст");
                return null;
            }
        }



        private static double get_length(double x1, double x2, double y1, double y2)
        {
            return Math.Sqrt(Math.Abs(Math.Pow(x1-x2,2)+Math.Pow(y1-y2,2)));
        }



        private static double[] GetMaxLength(IReadOnlyList<double[]> cloud, IList<double> point)
        {
            var max = new double[] { 0, 0, 0 };
            for (var i = 1; i < cloud.Count; i++)
            {
                if (get_length(cloud[i][0], point[0], cloud[i][1], point[1]) > max[2])
                {
                    max[0] = cloud[i][0];
                    max[1] = cloud[i][1];
                    max[2] = get_length(cloud[i][0], point[0], cloud[i][1], point[1]);
                }
            }
            return new[] { max[0], max[1] };
        }

        private void FuckingEquals(double[] point)
        {
            for (var i = 0; i < _reducingList.Count; i++)
            {
                if (point.SequenceEqual(_reducingList[i]))
                {
                    _reducingList.Remove(_reducingList[i]);
                }
            }
        }


        private void FuckingEquals1(double[] point)
        {
            foreach (var elem in _reducingList.Where(elem => point[0] == elem[0] && point[1] == elem[1]))
            {
                _reducingList.Remove(elem);
                break;
            }
        }


        private static void FuckingEquals(IList<double> point, IEnumerable<List<double[]>> cloud)
        {
            foreach (var t in cloud)
                for (var j = 0; j < t.Count; j++)
                {
                    if (point[0]==t[j][0] && point[1]==t[j][1])
                    {
                        t.Remove(t[j]);
                    }
                }
        }

        private double[] get_regression(IReadOnlyCollection<double[]> cloud)
        {
            double meanXy = 0, meanX = 0, meanY = 0, meanSqrX = 0;
            foreach (double[] t in cloud)
            {
                meanXy += t[0]*t[1];
                meanX += t[0];
                meanY += t[1];
                meanSqrX += Math.Pow(t[0], 2);
            }
            meanSqrX /= cloud.Count;
            meanX /= cloud.Count;
            meanY /= cloud.Count;
            meanXy /= cloud.Count;


            
            var a = Math.Round((meanXy - meanX*meanY)/(meanSqrX - Math.Pow(meanX, 2)),3);
            var b = Math.Round(meanY - (a * meanX), 3);

            if (double.IsNaN(a))
            {
                a = double.PositiveInfinity;
            }
            if (double.IsNaN(b))
            {
                b = double.PositiveInfinity;
            }

            return new[]{a,b};
        }


        private double Correlation(IReadOnlyList<double[]> cloud)
        {
            double meanX = 0, meanY = 0, upCor=0, downCor1=0, downCor2=0;
            for (var i = 0; i < _n; i++)
            {
                meanX += cloud[i][0];
                meanY += cloud[i][1];
            }
            meanX /= _n;
            meanY /= _n;

            for (var i = 0; i < _n; i++)
            {
                upCor += (cloud[i][0] - meanX)*(cloud[i][1] - meanY);
                downCor1 += Math.Pow(cloud[i][0] - meanX,2);
                downCor2 += Math.Pow(cloud[i][1] - meanY, 2);
            }

            return upCor == 0 ? 1 : Math.Round(upCor/Math.Sqrt(downCor1*downCor2), 3);
        }


        private static List<int[]> GetMinMax(IReadOnlyList<double[]> cloud)
        {
            double minx = cloud[0][0], miny = cloud[0][1], maxx = cloud[0][0], maxy = cloud[0][1];
            var result= new List<int[]>();
            foreach (double[] t in cloud)
            {
                if (minx > t[0])
                    minx = t[0];
                else if (maxx < t[0])
                    maxx = t[0];

                if (miny > t[1])
                    miny = t[1];
                else if (maxy < t[1])
                    maxy = t[1];
            }
            result.Add(new[]{(int)minx, (int)miny});
            result.Add(new[]{(int)maxx, (int)maxy});

            return result;
        }


        private void DrawApproxBit()
        {
            _approxBit = new Bitmap(_inputImage);
            foreach (var cloud in _objectsMassive)
            {
                if (Correlation(cloud) >= _correlationConst)
                {
                    var xmin = GetMinMax(cloud)[0][0];
                    var xmax = GetMinMax(cloud)[1][0];
                    var ymin = GetMinMax(cloud)[0][1];
                    var ymax = GetMinMax(cloud)[1][1];

                    var a = get_regression(cloud)[0];
                    var b = get_regression(cloud)[1];

                    if (Math.Abs(a) <= 1)
                    {
                        for (var i = xmin; i < xmax; i++)
                        {
                            var number = Convert.ToInt32(i*a + b);
                            if (number >= _inputImage.Height)
                            {
                                number = _inputImage.Height - 1;
                            }
                            _approxBit.SetPixel(i, number, Color.Red);
                        }
                    }
                    else if (Math.Abs(a) > 1 && Math.Abs(a) < 10)
                    {
                        for (int i = ymin; i <= ymax; i++)
                        {
                            int number = Convert.ToInt32(-b/a + i/a);
                            if (number >= _inputImage.Width)
                            {
                                number = _inputImage.Width - 1;
                            }
                            _approxBit.SetPixel(number, i, Color.Red);
                        }
                    }
                    else
                    {
                        for (int i = ymin; i < ymax; i++)
                        {
                            _approxBit.SetPixel(xmin, i, Color.Red);
                        }
                    }
                }
            }
        }




        private List<double[]> GetNearestNPoints(double[] currPoint)
        {
            try
            {
                var superMassiveArray = new List<List<double[]>>();
                var pointLength = new List<double[]>();
                var nminLengthPoints = new List<double[]>();
                var previousPoints = new List<double[]>();
                Names = new List<string[]>();
                for (int i = 0; i < _n; i++)
                {
                    superMassiveArray.Add(new List<double[]>());
                    pointLength.AddRange(_reducingList.Select(t1 => new[]{t1[0], t1[1], get_length(currPoint[0], t1[0], currPoint[1], t1[1])}));
                    pointLength.Sort((x, y) => x[2].CompareTo(y[2]));
                    for (var k = 1; k < _n; k++)
                    {
                        superMassiveArray[i].Add(pointLength[k]);
                    }
                    double[] t = GetTotalMin(superMassiveArray);
                    if (t[2] <= LengthBetweenPoints)
                    {
                        nminLengthPoints.Add(t);
                        FuckingEquals(t, superMassiveArray);
                        FuckingEquals(new[] {t[0], t[1]});
                        previousPoints.Add(t);
                        currPoint = t;
                        pointLength.Clear();
                    }
                    if (t[2] >= LengthBetweenPoints && t[2] <= LengthBetweenObjects)
                    {
                        FuckingEquals(new[] {t[0], t[1]});
                        return GetNearestNPoints(t);
                    }
                    if (t[2] >= LengthBetweenObjects && !_endObj)
                    {
                        _endObj = true;
                        FuckingEquals(new[] {t[0], t[1]});
                        return GetNearestNPoints(_beginPoint);
                    }
                    if (!(t[2] >= LengthBetweenObjects) || !_endObj) continue;
                    _endObj = false;
                    _beginPoint = t;
                    FuckingEquals(new[] {t[0], t[1]});
                    if (_objectsMassive.Count > 5)
                    {

                        AllApproximObjects.Add(_objectsMassive);
                        DrawApproxBit();    
                        string pathBmp = @"1\"+_fileName+"\\" + _fileName + "." + _simpleI + ".bmp";
                        string pathBmp2 = _fileName + "." + _simpleI + ".bmp";
                        _approxBit.Save(pathBmp);
                        string pathTxt = @"1\" + _fileName+"\\"+ _fileName + "." + _simpleI + ".txt";
                        string pathTxt2 = _fileName + "." + _simpleI + ".txt";
                        string pathTxt3 = @"1\" + _fileName + "\\" + _simpleI+".txt";
                        var pathTxt4 = _simpleI + ".txt";
                        var file2 = new StreamWriter(pathTxt3);
                        for (var k = 1; k < _correlationList.Count; k++)
                        {
                            file2.WriteLine(Math.Round(Math.Atan(_regressionList[k][0]) - Math.Atan(_regressionList[k-1][0]),3));
                        }
                        file2.Close();
                        _correlationList.Clear();
                        _regressionList.Clear();

                        Names.Add(new []{pathBmp, pathTxt, pathBmp2, pathTxt2, pathTxt3, pathTxt4});
                        _simpleI++;

                    }
                    _objectsMassive = new List<List<double[]>>();
                    return GetNearestNPoints(t);
                }
                _objectsMassive.Add(nminLengthPoints);

                return nminLengthPoints;
            }
            catch (Exception)
            {
                if (_objectsMassive.Count <= 5) return null;
                DrawApproxBit();
                _approxBit.Save(@"1\"+_fileName +"\\"+_fileName+"."+ _simpleI + ".bmp");
                string pathBmp = @"1\" + _fileName + "\\" + _fileName + "." + _simpleI + ".bmp";
                string pathBmp2 = _fileName + "." + _simpleI + ".bmp";
                string pathTxt = @"1\" + _fileName + "\\" + _fileName + "." + _simpleI + ".txt";
                string pathTxt2 = _fileName + "." + _simpleI + ".txt";
                string pathTxt3 = @"1\" + _fileName + "\\" + _simpleI + ".txt";
                string pathTxt4 = _simpleI + ".txt";
                var file2 = new StreamWriter(pathTxt3);
                //for (var j = 0; j < _correlationList.Count; j++)
                //{
                //    var s = @"корреляция= " + _correlationList[j] + " a= " + _regressionList[j][0] + " b= " +
                //               _regressionList[j][1];
                //    file.WriteLine(s);
                //}
                for (var k = 1; k < _correlationList.Count; k++)
                {
                    file2.WriteLine(Math.Atan(_regressionList[k][0]) - Math.Atan(_regressionList[k-1][0]));
                }
                file2.Close();
                _correlationList.Clear();
                _regressionList.Clear();

                Names.Add(new[] { pathBmp, pathTxt, pathBmp2, pathTxt2, pathTxt3, pathTxt4});

                _simpleI++;
                return null;
            }
        }





        private static double[] GetTotalMin(IEnumerable<List<double[]>> globalCloud)
        {
            double[] min= {0,0, double.PositiveInfinity};
            foreach (var t in globalCloud)
            {
                for (var j = 0; j < t.Count; j++)
                {
                    if (t[j][2] < min[2])
                        min = t[j];
                    else j = t.Count;
                }
            }
            return min;
        }


    






        private void CheckCreateDirectories()
        {
            if (Directory.Exists(@"1\" + _fileName))
            {
                MessageBox.Show(@"Папка с таким названием уже существует. Прекратите выполнение программы и измените название входного файла, иначе выходные файлы будут перезаписаны.");
            }
            else
            Directory.CreateDirectory(@"1\" + _fileName);
        }


    



        public void draw()
        {
            List<List<double[]>> currObject = AllApproximObjects[iterat];
            var a = GetMinMax(currObject);
            Bitmap bit = new Bitmap(a[1][0] - a[0][0] + 2, a[1][1] - a[0][1] + 2);
            foreach (var nPoints in currObject)
            {
                foreach (var point in nPoints)
                {
                    //тут падает 
                    bit.SetPixel((int)point[0] - a[0][0], (int)point[1] - a[0][1], Color.Black);
                }
            }
            bitForComparsion = bit;
        }



        private List<int[]> GetMinMax(IEnumerable<List<double[]>> cloud)
        {

            double minx = double.PositiveInfinity, miny = double.PositiveInfinity, maxx = double.NegativeInfinity, maxy = double.NegativeInfinity;
            foreach (var nPoints in cloud)
            {
                foreach (var point in nPoints)
                {
                    if (minx > point[0])
                        minx = point[0];
                    if (maxx < point[0])
                        maxx = point[0];

                    if (miny > point[1])
                        miny = point[1];
                    if (maxy < point[1])
                        maxy = point[1];
                }
            }

            return new List<int[]> { new[] { (int)minx, (int)miny }, new[] { (int)maxx, (int)maxy } };
        }

        

        private double equalTwoArrays(double[] three)
        {
            var min = double.PositiveInfinity;
            for (int i = 2; i < libar1.Count; i++)
            {
                var number = Math.Abs(three[0] - libar1[i - 2]) + Math.Abs(three[1] - libar1[i - 1]) + Math.Abs(three[2] - libar1[i]);
                if (min > number)
                {
                    min = number;
                }
            }
            return min;
        }

        private double equalTwoArraysRevert(double[] three)
        {
            var min = double.PositiveInfinity;
            for (int i = 2; i < inpAr.Count; i++)
            {
                var number = Math.Abs(three[0] - inpAr[i - 2]) + Math.Abs(three[1] - inpAr[i - 1]) + Math.Abs(three[2] - inpAr[i]);
                if (min > number)
                {
                    min = number;
                }
            }
            return min;
        }







    }
}