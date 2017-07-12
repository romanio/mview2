using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace mview2
{
    public enum NameOptions
    {
        Aquifer,
        Block,
        Completion,
        Field,
        Group,
        LGBlock,
        LGCompletion,
        LGWell,
        Network,
        Region,
        RegionFlows,
        RegionComponent,
        WellSegment,
        Well,
        Other
    }
    public struct VectorData
    {
        public int index;
        public string keyword;
        public string measurement;
        public string unit;
    }

    public class Vector
    {
        public NameOptions Type;
        public string Name;
        public int Num;
        public List<VectorData> Data = new List<VectorData>();
    }


    public class EclipseProject
    {
        public string FILENAME;
        public string ROOT;
        public string PATH;
        public Dictionary<string, string> FILES;
        public List<Vector> VECTORS = null;
        public SMSPEC SUMMARY = null;
        public RSSPEC RESTART = null;
        public EGRID EGRID = null;


        public void OpenData(string filename)
        {
            // Следует разобраться со структурой файлов в директории

            FILENAME = filename;
            ROOT = Path.GetFileNameWithoutExtension(FILENAME).ToUpper();
            PATH = Path.GetDirectoryName(FILENAME).ToUpper();
            FILES = new Dictionary<string, string>();

            var files = Directory.GetFiles(PATH).OrderBy(f => f).ToArray();
            string extension;
            string rootname;

            foreach (string item in files)
            {
                extension = Path.GetExtension(item);
                rootname = Path.GetFileNameWithoutExtension(item);

                if (rootname == ROOT)
                {
                    if (extension == ".SMSPEC") FILES.Add("SMSPEC", item);
                    if (extension == ".RSSPEC") FILES.Add("RSSPEC", item);
                    if (extension == ".INSPEC") FILES.Add("INSPEC", item);
                    if (extension == ".EGRID") FILES.Add("EGRID", item);
                    if (extension == ".INIT") FILES.Add("INIT", item);
                    if (Regex.IsMatch(extension, "^.S+[0-9]{4}")) FILES.Add(extension, item);
                    if (Regex.IsMatch(extension, "^.X+[0-9]{4}")) FILES.Add(extension, item);
                    if (extension == ".UNSMRY") FILES.Add("UNSMRY", item);
                    if (extension == ".UNRST") FILES.Add("UNRST", item);
                }
            }

            if (FILES.ContainsKey("SMSPEC"))
            {
                SUMMARY = new SMSPEC(FILES["SMSPEC"]);
                ProceedSUMMARY();
            }

            if (FILES.ContainsKey("RSSPEC"))
            {
                RESTART = new RSSPEC(FILES["RSSPEC"]);
            }

        }

        void ProceedSUMMARY()
        {
            var temp = new List<Vector>();

            for (int iw = 0; iw < SUMMARY.KEYWORDS.Length; ++iw)
            {
                if (SUMMARY.KEYWORDS[iw].StartsWith("F"))
                    temp.Add(new Vector
                    {
                        Type = NameOptions.Field,
                        Name = SUMMARY.WGNAMES[iw],
                        Data = new List<VectorData>()
                        {
                            new VectorData
                            {
                                index = iw,
                                keyword = SUMMARY.KEYWORDS[iw],
                                unit = SUMMARY.WGUNITS[iw]??"",
                                measurement = SUMMARY.MEASRMNT[iw]??""
                            }
                        }
                    });

                if (SUMMARY.KEYWORDS[iw].StartsWith("W"))
                    temp.Add(new Vector
                    {
                        Type = NameOptions.Well,
                        Name = SUMMARY.WGNAMES[iw],
                        Data = new List<VectorData>()
                        {
                            new VectorData
                            {
                                index = iw,
                                keyword = SUMMARY.KEYWORDS[iw],
                                unit = SUMMARY.WGUNITS[iw]??"",
                                measurement = SUMMARY.MEASRMNT[iw]??""
                            }
                        }
                    });

                if (SUMMARY.KEYWORDS[iw].StartsWith("G"))
                    temp.Add(new Vector
                    {
                        Type = NameOptions.Group,
                        Name = SUMMARY.WGNAMES[iw],
                        Data = new List<VectorData>()
                        {
                            new VectorData
                            {
                                index = iw,
                                keyword = SUMMARY.KEYWORDS[iw],
                                unit = SUMMARY.WGUNITS[iw]??"",
                                measurement = SUMMARY.MEASRMNT[iw]??""
                            }
                        }
                    });

                if (SUMMARY.KEYWORDS[iw].StartsWith("A"))
                    temp.Add(new Vector
                    {
                        Type = NameOptions.Aquifer,
                        Name = "A" + SUMMARY.NUMS[iw].ToString(),
                        Data = new List<VectorData>()
                        {
                            new VectorData
                            {
                                index = iw,
                                keyword = SUMMARY.KEYWORDS[iw],
                                unit = SUMMARY.WGUNITS[iw]??"",
                                measurement = SUMMARY.MEASRMNT[iw]??""
                            }
                        }
                    });

                if (SUMMARY.KEYWORDS[iw].StartsWith("R"))
                    temp.Add(new Vector
                    {
                        Type = NameOptions.Region,
                        Name = "R" + SUMMARY.NUMS[iw].ToString(),
                        Data = new List<VectorData>()
                        {
                            new VectorData
                            {
                                index = iw,
                                keyword = SUMMARY.KEYWORDS[iw],
                                unit = SUMMARY.WGUNITS[iw]??"",
                                measurement = SUMMARY.MEASRMNT[iw]??""
                            }
                        }
                    });
            }

            VECTORS = new List<Vector>();

            foreach (var item in temp.GroupBy(c => new { c.Name, c.Type, c.Num }))
            {
                VECTORS.Add(
                    new Vector
                    {
                        Name = item.Key.Name,
                        Type = item.Key.Type,
                        Num = item.Key.Num,
                        Data = item.Select(c => c.Data[0]).ToList()
                    });
            }
        }
    }
}
