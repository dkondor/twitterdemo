using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Elte.GeoVisualizer.Lib
{
    public class Map
    {
        private Projection projection;
        private List<Layer> layers;
        private List<DataSource> dataSources;

        public Projection Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public List<Layer> Layers
        {
            get { return layers; }
        }

        public List<DataSource> DataSources
        {
            get { return dataSources; }
        }

        public Map()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.projection = null;
            this.layers = new List<Layer>();
            this.dataSources = new List<DataSource>();
        }


        public void Render(Bitmap bmp, Rectangle bounds)
        {
            // TODO
            var context = new RenderingContext()
            {
                Width = bounds.Width,
                Height = bounds.Height,
                Projection = this.projection,
                Random = new Random()
            };

            var dss = CollectDataSources();

            foreach (var ds in dss.Keys)
            {
                var ls = dss[ds].ToArray();

                ds.Open();

                var names = ds.GetColumnNames();
                var values = new object[names.Length];

                for (int i = 0; i < ls.Length; i++)
                {
                    ls[i].OnBeginRender(context);
                }

                while (ds.ReadNext(values))
                {
                    for (int i = 0; i < ls.Length; i++)
                    {
                        ls[i].OnRender(context, values);
                    }
                }

                ds.Close();
            }

            using (var g = Graphics.FromImage(bmp))
            {

                foreach (var l in ((IEnumerable<Layer>)layers).Reverse())
                {
                    var lbmp = l.OnEndRender();

                    g.DrawImage(lbmp, bounds);
                }

            }
        }

        private Dictionary<DataSource, List<Layer>> CollectDataSources()
        {
            var res = new Dictionary<DataSource, List<Layer>>();

            foreach (var l in layers)
            {
                if (!res.ContainsKey(l.DataSource))
                {
                    res.Add(l.DataSource, new List<Layer>());
                }

                res[l.DataSource].Add(l);
            }

            return res;
        }

    }
}
