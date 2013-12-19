using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;


namespace Elte.GeoVisualizer.Lib.DataSources {
    /// <summary>
    /// store an array of coordinates, and possibly timestamps / sentiment to be displayed as a histogram
    /// </summary>
    public class CoordArray : DataSource {
        /// <summary>
        /// store triplets of data
        /// </summary>
        public struct crecord {
            public double lat;
            public double lon;
            public int t;
        };

        private List<crecord> data;
        private int records;
        private int counter;
        private bool col3;

        private bool nocache;
        private bool opened;
        private string query1;
        private string cstr1;
        private SqlQuery sq;

        /// <summary>
        /// do not cache the data retrieved from the server (behave like the SqlQuery datasource)
        /// useful if very large amount of data is needed
        /// </summary>
        public bool NoCache {
            get { return nocache; }
            set {
                if (opened) throw new Exception("CoordArray: Cannot change parameters while running the query!");
                nocache = value;
                if (nocache == true) {
                    data = null;
                    records = 0;
                    counter = 0;
                }
            }
        }

        /// <summary>
        /// set everything to zero
        /// </summary>
        public CoordArray() {
            data = null;
            records = 0;
            counter = 0;
            col3 = false;
            nocache = false;
            opened = false;
            query1 = null;
            sq = null;
        }

        /// <summary>
        /// Get all the stored data as an array
        /// </summary>
        /// <returns></returns>
        public crecord[] GetData() {
            if (nocache) return null;
            else return data.ToArray();
        }

        /// <summary>
        /// load the data from the database
        /// </summary>
        /// <param name="query">An sql query, which returns the coordinates (lon, lat, in that order) and possibly a third, integer column with additional data.</param>
        /// <param name="cstr">Connection string to use</param>
        /// <param name="col3q">Set to true if the query returns three columns.</param>
        public void LoadData(string query, string cstr, bool col3q) {
            if (data != null) {
                data = null;
            }
            if (nocache) {
                query1 = query;
                cstr1 = cstr;
                col3 = col3q;
                return;
            }

            data = new List<crecord>();
            records = 0;
            col3 = col3q;
            SqlConnection connection = new SqlConnection(cstr);
            connection.Open();
            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataReader sr = cmd.ExecuteReader();

            Object[] data1 = new Object[3];
            double[] c1 = new double[2];
            while (sr.Read()) {
                crecord data2 = new crecord();
                int cnt = sr.GetValues(data1);
                if (cnt < 2) {
                    throw new Exception("CoordArray.LoadData(): error executing query!\n");
                }
                if (col3 && cnt < 3) {
                    throw new Exception("CoordArray.LoadData(): error executing query (got only 2 columns instead of 3)!\n");
                }
                for (int i = 0; i < 2; i++) {
                    Type t = data1[i].GetType();
                    if (t.Equals(typeof(System.Double))) {
                        c1[i] = (double)data1[i];
                    }
                    else if (t.Equals(typeof(System.Single))) {
                        c1[i] = (double)(float)data1[i];
                    }
                    else if (t.Equals(typeof(System.Int32))) {
                        c1[i] = (double)(int)data1[i];
                    }
                    else {
                        IConvertible d1i = data1[i] as IConvertible;
                        if (d1i == null)
                            throw new InvalidCastException("CoordArray.LoadData(): invalid data!\n");
                        c1[i] = d1i.ToDouble(null);
                    }
                }
                data2.lat = c1[1];
                data2.lon = c1[0];

                if (col3) {
                    Type t = data1[2].GetType();
                    if (t.Equals(typeof(System.Int32))) {
                        data2.t = (int)data1[2];
                    }
                    else if(t.Equals(typeof(System.Int16))) {
                        data2.t = (int)(Int16)data1[2];
                    }
                    else {
                        IConvertible d1i = data1[2] as IConvertible;
                        if (d1i == null)
                            throw new InvalidCastException("CoordArray.LoadData(): invalid data!\n");
                        data2.t = d1i.ToInt32(null);
                    }
                }

                data.Add(data2);

                records++;
            }
            connection.Close();
        }

        public override string[] GetColumnNames() {
            if (col3) return new string[] { "lon", "lat", "t" };
            else return new string[] { "lon", "lat" };
        }

        public override void Open() {
            if (nocache) {
                if (query1 == null || cstr1 == null) {
                    throw new Exception("CoordArray.Open(): no query or connection string specified!\n");
                }
                sq = new SqlQuery();
                sq.ConnectionString = cstr1;
                sq.Command = new SqlCommand(query1);
                sq.Open();
            }
            else {
                if (data == null) {
                    throw new Exception("CoordArray.Open(): no data loaded!\n");
                }
                counter = 0;
            }
        }

        public override void Close() {
            if (nocache) {
                sq.Close();
                sq = null;
            }
            else counter = 0;
        }

        public override bool ReadNext(object[] values1) {
            if (nocache) return sq.ReadNext(values1);
            if (data == null) {
                throw new Exception("CoordArrayReadNext(): no data loaded!\n");
            }
            if (counter == records) return false;
            values1[0] = data[counter].lon;
            values1[1] = data[counter].lat;
            if (col3) values1[2] = data[counter].t;
            counter++;
            return true;
        }


        public int GetDataNum() {
            return records;
        }
    }
}
