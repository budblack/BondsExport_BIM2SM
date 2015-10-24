using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using SuperMap.Data;
using SuperMap.Realspace;
using SuperMap.UI;

namespace BondsExport
{
    class BE
    {
        Workspace ws = new Workspace();
        string root;

        public BE(string path)
        {
            WorkspaceConnectionInfo wscon = new WorkspaceConnectionInfo();
            wscon.Type = WorkspaceType.SMWU;
            wscon.Server = path;
            root = path.Substring(0, path.LastIndexOf('\\'));
            ws.Open(wscon);

            TypeModelProcess tmp = new TypeModelProcess();

            foreach (Datasource datasource in ws.Datasources)
            {
                foreach (Dataset dataset in datasource.Datasets)
                {
                    switch (dataset.Type)
                    {
                        case DatasetType.CAD:
                            //case DatasetType.Model:
                            if (tmp.OutPath == root + @"\BoundingBox\" + dataset.Name + "@" + datasource.Description + ".txt")
                            {
                                break;
                            }
                            else
                            {
                                tmp.Dataset = dataset as DatasetVector;
                                tmp.OutPath = root + @"\BoundingBox\" + dataset.Name + "@" + datasource.Description + ".txt";
                                new Thread(tmp.run).Start();
                                Thread.Sleep(100);
                            }
                        //addBox(tmp.Dataset, tmp.OutPath);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        void addBox(DatasetVector dv, string file)
        {
            FileStream f;
            StreamWriter sw;
            Recordset rc = dv.GetRecordset(false, CursorType.Dynamic);
            Console.WriteLine(dv.Name + "\t::\t" + dv.Type.ToString() + "\t::\t" + dv.RecordCount);

            Dictionary<int, Feature> feas = rc.GetAllFeatures();
            f = new FileStream(file, FileMode.OpenOrCreate);
            sw = new StreamWriter(f);

            foreach (Feature item in feas.Values)
            {
                Point3D lower, uper, center;

                if ((item.GetGeometry() as Geometry3D) != null)
                {
                    lower = (item.GetGeometry() as Geometry3D).BoundingBox.Lower;
                    uper = (item.GetGeometry() as Geometry3D).BoundingBox.Upper;
                    center = (item.GetGeometry() as Geometry3D).BoundingBox.Center;

                    sw.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", item.GetID(), lower.X, lower.Y, lower.Z, uper.X, uper.Y, uper.Z));
                    if (!dv.IsOpen)
                    {
                        dv.Open();
                    }

                    Dictionary<string, double> fields = new Dictionary<string, double>();
                    fields.Add("Lx", lower.X);
                    fields.Add("Ly", lower.Y);
                    fields.Add("Lz", lower.Z);
                    fields.Add("Ux", uper.X);
                    fields.Add("Uy", uper.Y);
                    fields.Add("Uz", uper.Z);


                    foreach (KeyValuePair<string, double> field in fields)
                    {
                        if (dv.FieldInfos.IndexOf(field.Key) < 0)
                        {
                            FieldInfo fieldInf = new FieldInfo(field.Key, FieldType.Double);
                            dv.FieldInfos.Add(fieldInf);
                        }

                        string fieldName = field.Key;
                        double fieldValue = field.Value;
                        try
                        {
                            rc.SeekID(item.GetID());
                            rc.Edit();
                            rc.SetFieldValue(fieldName, fieldValue);
                            rc.Update();
                        }
                        catch
                        {
                            Console.WriteLine("error!");
                        }
                        //Console.WriteLine(string.Format("{0},{1},{2}", item.GetID(), fieldName, fieldValue));
                    }
                    //Console.WriteLine("=="+item.GetID()+"==");
                }
            }
            Console.WriteLine(dv.Name + " done!");
            sw.Close();
            f.Close();
            rc.Close();
            dv.Close();
        }
    }
    class TypeModelProcess
    {
        public DatasetVector Dataset { get; set; }
        public string OutPath { get; set; }

        bool lockToken = false;
        public void run()
        {
            DatasetVector dv;
            string file;
            lock (this)
            {
                dv = this.Dataset;
                file = this.OutPath;
            }
            FileStream f;
            StreamWriter sw;
            Recordset rc = dv.GetRecordset(false, CursorType.Dynamic);
            Console.WriteLine(dv.Name + "\t::\t" + dv.Type.ToString() + "\t::\t" + dv.RecordCount);

            Dictionary<int, Feature> feas = rc.GetAllFeatures();
            try
            {
                f = new FileStream(file, FileMode.OpenOrCreate);
                sw = new StreamWriter(f);
            }
            catch
            {
                return;
            }
            foreach (Feature item in feas.Values)
            {
                Point3D lower, uper, center;

                if ((item.GetGeometry() as Geometry3D) != null)
                {
                    lower = (item.GetGeometry() as Geometry3D).BoundingBox.Lower;
                    uper = (item.GetGeometry() as Geometry3D).BoundingBox.Upper;
                    center = (item.GetGeometry() as Geometry3D).BoundingBox.Center;

                    sw.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", item.GetID(), lower.X, lower.Y, lower.Z, uper.X, uper.Y, uper.Z));
                    if (!dv.IsOpen)
                    {
                        dv.Open();
                    }

                    Dictionary<string, double> fields = new Dictionary<string, double>();
                    fields.Add("Lx", lower.X);
                    fields.Add("Ly", lower.Y);
                    fields.Add("Lz", lower.Z);
                    fields.Add("Ux", uper.X);
                    fields.Add("Uy", uper.Y);
                    fields.Add("Uz", uper.Z);


                    foreach (KeyValuePair<string, double> field in fields)
                    {
                        if (dv.FieldInfos.IndexOf(field.Key) < 0)
                        {
                            FieldInfo fieldInf = new FieldInfo(field.Key, FieldType.Double);
                            dv.FieldInfos.Add(fieldInf);
                        }

                        string fieldName = field.Key;
                        double fieldValue = field.Value;
                        try
                        {
                            rc.SeekID(item.GetID());
                            rc.Edit();
                            rc.SetFieldValue(fieldName, fieldValue);
                            rc.Update();
                        }
                        catch
                        {
                            Console.WriteLine("error!");
                        }
                        //Console.WriteLine(string.Format("{0},{1},{2}", item.GetID(), fieldName, fieldValue));
                    }
                    //Console.WriteLine("=="+item.GetID()+"==");
                }
            }
            Console.WriteLine(dv.Name + " done!");
            sw.Close();
            f.Close();
            rc.Close();
            dv.Close();
        }
    }
}
