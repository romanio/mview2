using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace mview2
{
    public class EclipseProject
    {
        public string FILENAME;
        public string ROOT;
        public string PATH;
        public Dictionary<string, string> FILES;
        public Summary SUMMARY = null;

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

            if (FILES["SMSPEC"] != null)
            {
                SUMMARY = new Summary(FILES["SMSPEC"]);
                ProceedSUMMARY();
            }
        }

        void ProceedSUMMARY()
        {

        }
    }
}
