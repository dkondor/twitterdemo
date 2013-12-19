using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;

using Elte.GeoVisualizer.Lib;

namespace DemoVisu
{
    public partial class Form1 : Form
    {
        static readonly DateTime unixtime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        Random r = new Random();
        Dictionary<string, int> regions = new Dictionary<string, int>();
        Dictionary<string, ColorVal.ColorMaps> colorScalesMention = null; // = new Dictionary<string, int>();
        Dictionary<string, ColorVal.ColorMaps> colorScalesText = null; // = new Dictionary<string, int>();
        Dictionary<string, ColorVal.ColorMaps> colorScalesTime = null; // = new Dictionary<string, int>();
        Dictionary<string, ColorVal.ColorMaps> colorScalesSentiment = null; // = new Dictionary<string, int>();

        BackgroundWorker bwdrawmapmention = new BackgroundWorker();
        BackgroundWorker bwdrawmaptext = new BackgroundWorker();
        BackgroundWorker bwdrawmapsentiment = new BackgroundWorker();
        BackgroundWorker bwsavemap = new BackgroundWorker();

        Bitmap loadingScreen = new Bitmap("pics\\bg-loading.gif");
        Bitmap stopScreen = new Bitmap("pics\\stop.png");
        //Bitmap finalImage = new Bitmap("..\\..\\pics\\Error.png");
        Bitmap finalImage = null;
        Bitmap img_textmap = new Bitmap("pics\\Error.png");
        Bitmap img_sentiment = new Bitmap("pics\\Error.png");
        Bitmap bmp2 = new Bitmap("pics\\Error.png");

        //hisztogram rajzoló (szöveges keresés, háttérképet megtartjuk)
        DrawHistogram hist_textmap;
        DrawTimeSeries times_textmap;
        const string cstr = "Data Source=FUTURE1;Initial Catalog=dkondor;Integrated Security=true";
        bool querychanged = true;
        //sentiment analysis eredmények rajzoló
        DrawHistogram hist_sentiment;

        DrawMap md;
        bool regionchanged = true;
        bool sentimentchanged = true;

        bool regionsizechanged = false;
        bool histsizechanged = false;
        bool sentimentsizechanged = false;



        public Form1()
        {
            InitializeComponent();

            this.saveFileDialog1.DefaultExt = "png";
            this.saveFileDialog1.Filter = "\"All file|*.*|PNG files|*.png\"";
            this.saveFileDialog1.FilterIndex = 2;

            splitContainer1.FixedPanel = FixedPanel.Panel1; //bal panel fix méretű
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer2.FixedPanel = FixedPanel.Panel1;
            splitContainer3.IsSplitterFixed = true;
            splitContainer2.IsSplitterFixed = true;
            splitContainer1.IsSplitterFixed = true;

            tabControl1.TabPages[0].Text = "Mentions";
            tabControl1.TabPages[1].Text = "Text search";
            tabControl1.TabPages[2].Text = "Sentiment search";
            drawmapmention.Text = "DrawMap"; //mention
            drawmaptext.Text = "DrawMap"; //text
            drawmapsentiment.Text = "DrawMap"; //sentiment

            dateTimePickerEnd.MinDate = dateTimePickerStart.Value;

            textBox_timebins.Text = trackBarTimeBins.Value.ToString();
            
            ReadRegionList(); //regiok beolvasasa a legordulo menuhoz

            //kep loader külön threadbe - background workernek
            bwdrawmapmention.WorkerReportsProgress = true;
            bwdrawmapmention.WorkerSupportsCancellation = true;
            bwdrawmapmention.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwdrawmapmention_RunWorkerCompleted);
            //bwdrawmapmention.ProgressChanged += new ProgressChangedEventHandler(bwdrawmapmention_ProgressChanged);
            bwdrawmapmention.DoWork += new DoWorkEventHandler(bwdrawmapmention_DoWork);
            //
            //histogram szál
            bwdrawmaptext.WorkerReportsProgress = true;
            bwdrawmaptext.WorkerSupportsCancellation = true;
            bwdrawmaptext.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwdrawmaptext_RunWorkerCompleted);
            //bwdrawmaptext.ProgressChanged += new ProgressChangedEventHandler(bwdrawmaptext_ProgressChanged);
            bwdrawmaptext.DoWork += new DoWorkEventHandler(bwdrawmaptext_DoWork);
            //
            //sentiment szál
            bwdrawmapsentiment.WorkerReportsProgress = true;
            bwdrawmapsentiment.WorkerSupportsCancellation = true;
            bwdrawmapsentiment.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwdrawmapsentiment_RunWorkerCompleted);
            //bwdrawmapsentiment.ProgressChanged += new ProgressChangedEventHandler(bwdrawmapsentiment_ProgressChanged);
            bwdrawmapsentiment.DoWork += new DoWorkEventHandler(bwdrawmapsentiment_DoWork);

            //képeket elmentő szál (egyelőre csak a szöveges keresésnél)
            bwsavemap.WorkerReportsProgress = true;
            bwsavemap.WorkerSupportsCancellation = true;
            bwsavemap.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwsavemap_RunWorkerCompleted);
            //bwdrawmapsentiment.ProgressChanged += new ProgressChangedEventHandler(bwdrawmapsentiment_ProgressChanged);
            bwsavemap.DoWork += new DoWorkEventHandler(bwsavemap_DoWork);

            //
            hist_textmap = null;
            md = null;
            hist_sentiment = null;
            times_textmap = null;

            trackBarTimeBins.Value = 15;

            finalImage = new Bitmap(pictureBox_mentionmap.Width, pictureBox_mentionmap.Height);
        }




        /******************************************
         * Draw a histogram map representing the  *
         * result of a sentiment analysis carried *
         * out previously for a specific hashtag  *
         ******************************************/
        private void drawmapsentiment_Click(object sender, EventArgs e) {
            if (!bwdrawmapsentiment.IsBusy) {

                savepicturesentiment.Enabled = false;
                pictureBox_sentiment.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox_sentiment.Image = loadingScreen;

                object[] arguments = new object[5];
                arguments[0] = sentimentmap_hashtag.Text; //# jel elé esetleg?
                arguments[1] = colorScalesSentiment[sentimentmap_colormap.Text];
                Projection.ProjectionView view = Projection.ProjectionView.World;
                if (radioButton14.Checked) view = Projection.ProjectionView.EU;
                else if (radioButton17.Checked) view = Projection.ProjectionView.USA;
                else if (radioButton16.Checked) view = Projection.ProjectionView.Asia;
                arguments[2] = view;
                arguments[3] = sentimentchanged;
                arguments[4] = sentimentsizechanged;
                bwdrawmapsentiment.RunWorkerAsync(arguments);
                sentimentchanged = false;
            }
            else {
                pictureBox_sentiment.Image = stopScreen;
                pictureBox_sentiment.Refresh();
                Thread.Sleep(500);
                pictureBox_sentiment.Image = loadingScreen;
                pictureBox_sentiment.Refresh();
            }
        }


        void bwdrawmapsentiment_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            object[] arguments = (object[])e.Argument;

            lock (img_sentiment) 
            {
                if (hist_sentiment == null) {
                    hist_sentiment = new DrawHistogram(cstr);
                    hist_sentiment.DrawColorBar = true;
                    hist_sentiment.Size = new Size(pictureBox_sentiment.Width, pictureBox_sentiment.Height);
                    hist_sentiment.DoubleLogScale = false;
                    hist_sentiment.LogScale = true;
                    hist_sentiment.DrawMaps();
                }
                else if ((bool)arguments[4] == true) {
                    hist_sentiment.Size = new Size(pictureBox_sentiment.Width, pictureBox_sentiment.Height);
                    hist_sentiment.DrawMaps();
                }

                Projection.ProjectionView view = (Projection.ProjectionView)arguments[2];
                hist_sentiment.MapView = (uint)view;

                bool querychanged2 = (bool)arguments[3];
                hist_sentiment.Col3 = true;
                hist_sentiment.StdevSat = 0.0;
                hist_sentiment.Col3Min = -1;
                hist_sentiment.Col3Max = 1;

                ColorVal cv = new ColorVal((ColorVal.ColorMaps)arguments[1]);
                string query = (string)arguments[0];
                if (querychanged2) {
                    hist_sentiment.LoadSentimentQuery(query, 0);
                }
                hist_sentiment.ColorScale3 = cv;
                img_sentiment = hist_sentiment.Draw();
            }
        }


        void bwdrawmapsentiment_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox_sentiment.Image = img_sentiment;
            pictureBox_sentiment.Refresh();
            savepicturesentiment.Enabled = true;
        }


        /*******************************************************************
         * Draw a histogram map based on the occurances of a word, or the
         * results of some SQL Query (it should return coordinates and
         * possibly timestamps)
         *******************************************************************/

        //struct to hold the parameters
        private class DrawMapTextArguments
        {
            public string querytext;
            public bool customsqlquery;
            public bool hashtagquery;
            public bool querychanged;
            public bool histsizechanged;
            public Projection.ProjectionView view;
            public bool timecolor;
            public ColorVal.ColorMaps cv;
            public bool timescaleauto;
            public int timemin;
            public int timemax;
            public int timebins;
        }

        //handle the button click
        private void drawmaptext_Click(object sender, EventArgs e)
        {
            if (querytext.Text != "" && querytext.Text != "#")
            {
                if (!bwdrawmaptext.IsBusy)
                {

                    savepicturetext.Enabled = false;
                    pictureBox_textmap.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox_textmap.Image = loadingScreen;

                    DrawMapTextArguments args = new DrawMapTextArguments();

                    args.querytext = querytext.Text;
                    if (querytext.Text[0] == '#') args.hashtagquery = true;
                    else args.hashtagquery = false;
                    args.querychanged = querychanged;
                    args.histsizechanged = histsizechanged;

                    Projection.ProjectionView view = Projection.ProjectionView.World;
                    if (textmap_view_eu.Checked) view = Projection.ProjectionView.EU;
                    else if (textmap_view_usa.Checked) view = Projection.ProjectionView.USA;
                    else if (textmap_view_asia.Checked) view = Projection.ProjectionView.Asia;
                    args.view = view;

                    args.timecolor = maptimecolor.Checked;

                    if (args.timecolor) args.cv = colorScalesTime[textmap_colormap.Text];
                    else args.cv = colorScalesText[textmap_colormap.Text];

                    if (querychanged) textmap_date_manual.Checked = false;


                    if (textmap_date_manual.Checked)
                    {
                        DateTime d1 = dateTimePickerStart.Value;
                        DateTime d2 = dateTimePickerEnd.Value;
                        TimeSpan t1 = d1.Subtract(unixtime);
                        TimeSpan t2 = d2.Subtract(unixtime);
                        args.timemin = (int)Math.Floor(t1.TotalSeconds);
                        args.timemax = (int)Math.Floor(t2.TotalSeconds);
                        args.timescaleauto = false;
                    }
                    else args.timescaleauto = true;
                    args.timebins = trackBarTimeBins.Value;

                    args.customsqlquery = customquery.Checked;

                    bwdrawmaptext.RunWorkerAsync(args);
                    querychanged = false;
                    histsizechanged = false;

                }
                else
                {
                    pictureBox_textmap.Image = stopScreen;
                    pictureBox_textmap.Refresh();
                    Thread.Sleep(500);
                    pictureBox_textmap.Image = loadingScreen;
                    pictureBox_textmap.Refresh();
                }
            }
        }

        //draw the map
        void bwdrawmaptext_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            DrawMapTextArguments args = (DrawMapTextArguments)e.Argument;

            bool querychanged2 = args.querychanged;
            bool timecolor = args.timecolor;
            
            lock (img_textmap) {
                if (hist_textmap == null) {
                    hist_textmap = new DrawHistogram(cstr);
                    hist_textmap.Size = new Size(pictureBox_textmap.Width, pictureBox_textmap.Height);
                    hist_textmap.DoubleLogScale = false;
                    hist_textmap.LogScale = true;
                    hist_textmap.DrawMaps();
                }
                else if (args.histsizechanged == true) {
                    hist_textmap.Size = new Size(pictureBox_textmap.Width, pictureBox_textmap.Height);
                    int ks = (int)Math.Round(hist_textmap.Size.Width / 200.0);
                    if (ks == 0) ks = 1;
                    hist_textmap.KernelSize = ks;
                    hist_textmap.DrawMaps();
                }

                Projection.ProjectionView view = (Projection.ProjectionView)args.view;
                hist_textmap.MapView = view;

                hist_textmap.Col3 = timecolor;
                if (args.timescaleauto == false) {
                    hist_textmap.Col3Min = args.timemin;
                    hist_textmap.Col3Max = args.timemax;
                    hist_textmap.Col3Auto = false;
                    hist_textmap.TimeBins = args.timebins;
                }
                else hist_textmap.Col3Auto = true;

                bool hashtag = args.hashtagquery;
                ColorVal cv = new ColorVal(args.cv);
                string query = args.querytext;
                if (args.customsqlquery) {
                    if (querychanged2) hist_textmap.LoadCustomQuery(query, true);
                }
                else {
                    if (querychanged2) {
                        if (hashtag) {
                            string query2;
                            if (query[0] == '#') query2 = query.Substring(1);
                            else query2 = query;
                            hist_textmap.LoadHashtagQuery(query2, 0); //!! TODO: query stringet visszaadja, el lehetne tenni valahova
                        }
                        else {
                            hist_textmap.LoadTextQuery(query, 0);
                        }
                    }
                }
                if (timecolor) hist_textmap.ColorScale3 = cv;
                else hist_textmap.ColorScale = cv;
                img_textmap = hist_textmap.Draw();
            }
            lock (bmp2) {
                //idősor alulra
                int w1 = pictureBox_textmap_time.Width;
                int h1 = pictureBox_textmap_time.Height;
                //TODO: rendes méret számolás !!
                if (w1 > 3 * h1) w1 = 3 * h1;
                if (times_textmap == null) times_textmap = new DrawTimeSeries();
                times_textmap.Size = new Size(w1, h1);
                times_textmap.ticsfont = new Font(FontFamily.GenericSansSerif, 12.0F);
                times_textmap.colorx = timecolor;
                bmp2 = hist_textmap.DrawTimeSeries(times_textmap);
           //     bmp2.Save("test1.png", ImageFormat.Png);
            }
            
        }


        void bwdrawmaptext_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            pictureBox_textmap_time.Image = bmp2;
            pictureBox_textmap_time.Refresh();
            pictureBox_textmap.Image = img_textmap;
            pictureBox_textmap.Refresh();
            savepicturetext.Enabled = true;
        }

        /*******************************************************************
         * Draw the colored region map based on the mention (or possibly
         *  follower or other) links between ther regions
         *******************************************************************/
        private class DrawMapMentionArgs {
            public int region;
            public ColorVal.ColorMaps cv;
            public bool regionchanged;
            public bool regionsizechanged;
            public Projection.ProjectionView view;
            public bool outgoing;
        }

        //button click
        private void drawmapmention_Click(object sender, EventArgs e)
        {
            if (!bwdrawmapmention.IsBusy)
            {
                DrawMapMentionArgs args = new DrawMapMentionArgs();
                savepicturemention.Enabled = false;
                pictureBox_mentionmap.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox_mentionmap.Image = loadingScreen;

                args.region = regions[regionlist.Text];
                args.cv = colorScalesMention[mentionmap_colormap.Text];
                args.regionchanged = regionchanged;
                args.regionsizechanged = regionsizechanged;
                Projection.ProjectionView view = Projection.ProjectionView.World;
                if (mentionmap_view_eu.Checked) view = Projection.ProjectionView.EU;
                else if (mentionmap_view_usa.Checked) view = Projection.ProjectionView.USA;
                else if (mentionmap_view_asia.Checked) view = Projection.ProjectionView.Asia;
                args.view = view;
                args.outgoing = mentionmap_links_outgoing.Checked;

                bwdrawmapmention.RunWorkerAsync(args);
                regionsizechanged = false;
                regionchanged = false;
            }
            else
            {
                pictureBox_mentionmap.Image = stopScreen;
                pictureBox_mentionmap.Refresh();
                Thread.Sleep(500);
                pictureBox_mentionmap.Image = loadingScreen;
                pictureBox_mentionmap.Refresh();
            }

        }

        //draw the map
        void bwdrawmapmention_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;
            DrawMapMentionArgs args = (DrawMapMentionArgs)e.Argument;
            if (md == null) {
                md = new DrawMap();
                md.cstr = cstr;
                md.bgcolor = Color.FromArgb(255, 21, 23, 62);
                md.pen = new Pen(Color.Black);
                md.DrawColorBar = true;

                Elte.GeoVisualizer.Lib.DataSources.GeomArray ga1 = new Elte.GeoVisualizer.Lib.DataSources.GeomArray();
                ga1.LoadGeom("select ID,Geom from dkondor.dbo.region03 order by ID", cstr);
                md.ga = ga1;
                md.SizeChanged(new Size(pictureBox_mentionmap.Width, pictureBox_mentionmap.Height));
            }
            else if (args.regionsizechanged) {
                Elte.GeoVisualizer.Lib.DataSources.GeomArray ga1 = new Elte.GeoVisualizer.Lib.DataSources.GeomArray();
                ga1.LoadGeom("select ID,Geom from dkondor.dbo.region03 order by ID", cstr);
                md.SizeChanged(new Size(pictureBox_mentionmap.Width, pictureBox_mentionmap.Height));
            }

            Projection.ProjectionView view = (Projection.ProjectionView)args.view;
            md.MapView = view;
            bool outlinks = args.outgoing;
            bool regionchanged2 = args.regionchanged;
            if (outlinks != md.OutLinks) regionchanged2 = true;
            md.OutLinks = outlinks;


            if (regionchanged2) md.LoadData(args.region);

            md.ColorScale = new ColorVal((ColorVal.ColorMaps)args.cv);
            md.LogScale = mentionmap_logscale.Checked;
            md.Dest = mentionmap_includeregion.Checked;

            if (mentionmap_norm_none.Checked) md.Normalization = DrawMap.NormType.None;
            else if (mentionmap_norm_usercount.Checked) md.Normalization = DrawMap.NormType.UserCount;
            else if (mentionmap_norm_mentioncnt.Checked) md.Normalization = DrawMap.NormType.MentionCount;
            else md.Normalization = DrawMap.NormType.MentionCount;

            lock (finalImage) {
                finalImage = md.DrawMentionMap();
            }


        }

        void bwdrawmapmention_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox_mentionmap.Image = finalImage;
            pictureBox_mentionmap.Refresh();
            savepicturemention.Enabled = true;
        }

        
        /****************************
         * other (helper) functions *
         ****************************/
        private void Form1_Load(object sender, EventArgs e)
        {

            
        }

        private void ReadRegionList() {
            TextReader tr1 = new StreamReader("info\\regionID2.txt");
            string temp = "";

            while ((temp = tr1.ReadLine()) != null) {
                regions.Add(temp.Split('\t')[1], Int32.Parse(temp.Split('\t')[0]));
            }

            colorScalesMention = ColorVal.GetColorMapsMention();
            colorScalesSentiment = ColorVal.GetColorMapsSentiment();
            colorScalesText = ColorVal.GetColorMapsIntensity();
            colorScalesTime = ColorVal.GetColorMapsTime();

            tr1.Close();

            foreach (string s in regions.Keys) {
                regionlist.Items.Add(s);
            }

            foreach (string s in colorScalesMention.Keys) {
                mentionmap_colormap.Items.Add(s);
            }

            foreach (string s in colorScalesSentiment.Keys)
            {
                sentimentmap_colormap.Items.Add(s);
            }

            foreach (string s in colorScalesText.Keys)
            {
                textmap_colormap.Items.Add(s);
            }


            regionlist.SelectedItem = regions.First().Key;
            mentionmap_colormap.SelectedItem = colorScalesMention.First().Key;
            textmap_colormap.SelectedItem = colorScalesText.First().Key;
            sentimentmap_colormap.SelectedItem = colorScalesSentiment.First().Key;


            //sentiment analysis eredmény hashtagek betöltése
            {
                Elte.GeoVisualizer.Lib.DataSources.SqlQuery sq = new Elte.GeoVisualizer.Lib.DataSources.SqlQuery();
                sq.ConnectionString = cstr;
                sq.Command = new System.Data.SqlClient.SqlCommand("select tag from dkondor.dbo.sentiment1 where sentiment != 0 group by tag having count(*) > 2000");
                sq.Open();
                object[] o1 = new object[1];
                while (sq.ReadNext(o1)) {
                    sentimentmap_hashtag.Items.Add((string)o1[0]);
                }
                sq.Close();
            }
        }

        private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }


        private void regionlist_SelectedIndexChanged(object sender, EventArgs e) {
            regionchanged = true;
        }

        private void splitContainer4_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void textmap_date_manual_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Enabled = !panel2.Enabled;
        }

        private void maptimecolor_CheckedChanged(object sender, EventArgs e)
        {
            if (!maptimecolor.Font.Bold)
            {
                maptimecolor.Font = new Font(maptimecolor.Font, FontStyle.Bold);
                textmap_colormap.Items.Clear();
                foreach (string s in colorScalesTime.Keys) {
                    textmap_colormap.Items.Add(s);
                }
                textmap_colormap.SelectedIndex = 0;
            }
            else
            {
                maptimecolor.Font = new Font(maptimecolor.Font, FontStyle.Regular);
            }
        }

        private void textmap_intensity_CheckedChanged(object sender, EventArgs e)
        {
            if (!textmap_intensity.Font.Bold)
            {
                textmap_intensity.Font = new Font(textmap_intensity.Font, FontStyle.Bold);
                textmap_colormap.Items.Clear();
                foreach (string s in colorScalesText.Keys)
                {
                    textmap_colormap.Items.Add(s);
                }
                textmap_colormap.SelectedIndex = 0;
            }
            else
            {
                textmap_intensity.Font = new Font(textmap_intensity.Font, FontStyle.Regular);
            }
        }

        private void querytext_TextChanged(object sender, EventArgs e) {
            querychanged = true;
        }

        private void pictureBox_textmap_SizeChanged(object sender, EventArgs e) {
            histsizechanged = true;
        }

        private void pictureBox_mentionmap_SizeChanged(object sender, EventArgs e) {
            regionsizechanged = true;
        }

        private void sentimentmap_hashtag_SelectedValueChanged(object sender, EventArgs e) {
            sentimentchanged = true;
        }

        private void trackBarTimeBins_ValueChanged(object sender, EventArgs e)
        {
            textBox_timebins.Text = trackBarTimeBins.Value.ToString();
        }

        private void dateTimePickerStart_ValueChanged(object sender, EventArgs e)
        {
            //dateTimePicker2.MinDate = dateTimePicker1.Value;
        }


        /*******************
         * save the images *
         *******************/
        private void savepicturemention_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Png Image|*.png|Bitmap Image|*.bmp";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox_mentionmap.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        pictureBox_mentionmap.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Png);
                        break;

                    case 3:
                        pictureBox_mentionmap.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }

                fs.Close();
            }
        }

        /**********************
         * save the text map *
         *********************/
        private struct SaveFileArgs {
            public string fname;
            public int ftype;
        }

        private void savepicturetext_Click(object sender, EventArgs e)
        {
            if (!bwsavemap.IsBusy) {
                savepicturetext.Enabled = false;
                drawmaptext.Enabled = false;
                saveFileDialog1.Filter = "JPeg Image|*.jpg|Png Image|*.png|Bitmap Image|*.bmp";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName == "") {
                    savepicturetext.Enabled = true;
                }
                else {
                    SaveFileArgs args;
                    args.fname = saveFileDialog1.FileName;
                    args.ftype = saveFileDialog1.FilterIndex;
                    bwsavemap.RunWorkerAsync(args);
                }
                drawmaptext.Enabled = true;
            }
        }

        private void bwsavemap_DoWork(object sender, DoWorkEventArgs e) {
            DrawHistogram hist2 = null;
            SaveFileArgs args = (SaveFileArgs)e.Argument;
            int savewidth = 4000; //!! TODO: make this adjustable by the user !!
            int saveheight = 2000;
            lock (hist_textmap) {
                hist2 = hist_textmap.Clone(savewidth, saveheight);
                savewidth = hist2.Size.Width;
                saveheight = hist2.Size.Height;
            }

            int ks = (int)Math.Round(savewidth / 200.0);
            if(ks == 0) ks = 1;
            hist2.KernelSize = ks;
            Bitmap bmp1 = hist2.Draw();

            int w1 = savewidth;
            int h1 = (int)(savewidth * 0.3);
            DrawTimeSeries times2 = new DrawTimeSeries();
            times2.Size = new Size(w1, h1);
            float fs = 12.0F * ((float)w1) / 800.0F;
            times2.ticsfont = new Font(FontFamily.GenericSansSerif, fs);
            times2.colorx = maptimecolor.Checked;
            Bitmap bmp2 = null;
            try {
                bmp2 = hist2.DrawTimeSeries(times2);
            }
            catch (Exception ex) {
                Console.Error.WriteLine(ex);
                bmp2 = null;
            }

            Bitmap bmp3 = null;
            if (bmp2 != null) {
                bmp3 = new Bitmap(savewidth, saveheight + h1);
                Graphics g1 = Graphics.FromImage(bmp3);
                g1.DrawImage(bmp1, new Point(0, 0));
                g1.DrawImage(bmp2, new Point(0, saveheight));
                g1.Dispose();
            }
            else bmp3 = bmp1;

            
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (args.ftype) {
                    case 1:
                        bmp3.Save(args.fname,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        bmp3.Save(args.fname,
                           System.Drawing.Imaging.ImageFormat.Png);
                        break;

                    case 3:
                        bmp3.Save(args.fname,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }

        }

        private void bwsavemap_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (!bwdrawmaptext.IsBusy) {
                savepicturetext.Enabled = true;
            }
        }

        private void savepicturesentiment_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "JPeg Image|*.jpg|Png Image|*.png|Bitmap Image|*.bmp";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox_sentiment.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;

                    case 2:
                        pictureBox_sentiment.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Png);
                        break;

                    case 3:
                        pictureBox_sentiment.Image.Save(fs,
                           System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                }

                fs.Close();
            }
        }

    }
}
