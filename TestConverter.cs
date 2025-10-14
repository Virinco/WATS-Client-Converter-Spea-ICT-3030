using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Virinco.WATS.Interface;

namespace Spea
{
    [TestClass]
    public class TestConverter : TDM
    {
        [TestMethod]
        public void SetupClient()
        {
            SetupAPI(null, "City, Country", "Demo", true);
            RegisterClient("https://example.wats.com", "", "token");
            InitializeAPI(true);
        }

        [TestMethod]
        public void TestICT3030Converter()
        {
            InitializeAPI(true);
            string fn = @"Data\testdata_SN9871234012";
            Dictionary<string, string> arguments = new ICT3030Converter().ConverterParameters;
            arguments.Add("partNumber", "PN1");
            ICT3030Converter converter = new ICT3030Converter(arguments);
            using (FileStream file = new FileStream(fn, FileMode.Open))
            {
                SetConversionSource(new FileInfo(fn), converter.ConverterParameters, null);
                Report uut = converter.ImportReport(this, file);
            }
            SubmitPendingReports();
        }

        [TestMethod]
        public void TestICT3030ConverterFolder()
        {
            InitializeAPI(true);
            
            var folderPath = @"Data";
            var files = Directory.GetFiles(folderPath);
            Dictionary<string, string> arguments = new ICT3030Converter().ConverterParameters;
            arguments.Add("partNumber", "PN1");

            foreach (string fn in files)
            {
                ICT3030Converter converter = new ICT3030Converter(arguments);
                using (FileStream file = new FileStream(fn, FileMode.Open))
                {
                    SetConversionSource(new FileInfo(fn), converter.ConverterParameters, null);
                    Report uut = converter.ImportReport(this, file);
                    SubmitPendingReports();
                }
            }
        }
    }
}
