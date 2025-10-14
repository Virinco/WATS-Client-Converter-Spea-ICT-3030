using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Virinco.WATS.Integration.TextConverter;
using Virinco.WATS.Interface;

namespace Spea
{
    public class ICT3030Converter : TextConverterBase
    {
        public ICT3030Converter() : base()
        {
            converterArguments.Add("partNumber", "PN1");
        }
        public ICT3030Converter(IDictionary<string, string> args) : base(args)
        {
            this.currentCulture = CultureInfo.InvariantCulture;
            this.searchFields.culture = CultureInfo.InvariantCulture;

            const string headerRegEx = @"^START;(?<Prog>[^;]*);(?<Seq>[^;]*);(?<SeqVer>[^;]*);(?<Seq2>[^;]*);(?<Oper>[^;]*);(?<StartDate>[^;]*);(?<StartTime>[^;]*)";
            SearchFields.RegExpSearchField regExField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, headerRegEx, null, typeof(string), ReportReadState.InTest);
            regExField.AddSubField("Seq", typeof(string), null, UUTField.SequenceName);
            regExField.AddSubField("SeqVer", typeof(string), null, UUTField.SequenceVersion);
            regExField.AddSubField("Oper", typeof(string), null, UUTField.Operator);
            regExField.AddSubField("StartDate", typeof(DateTime), "MM/dd/yyyy");
            regExField.AddSubField("StartTime", typeof(TimeSpan), "HH:mm:ss");

            const string measureRegEx = @"^ANL;(?<Nr1>[^;]*);(?<StepRef>[^;]*);(?<Nr2>[^;]*);(?<Nr3>[^;]*);(?<StepName>[^;]*);(?<F1>[^;]*);(?<Result>[^;]*);(?<Meas>[^;]*);(?<LoLim>[^;]*);(?<HiLim>[^;]*);(?<Unit>[^;]*);(?<F2>[^;]*);(?<Nrr4>[^;]*)";
            regExField = searchFields.AddRegExpField("Measure", ReportReadState.InTest, measureRegEx, null, typeof(string));
            regExField.AddSubField("StepRef", typeof(string));
            regExField.AddSubField("StepName", typeof(string));
            regExField.AddSubField("Result", typeof(string));
            regExField.AddSubField("Meas", typeof(double));
            regExField.AddSubField("LoLim", typeof(double));
            regExField.AddSubField("HiLim", typeof(double));
            regExField.AddSubField("Unit", typeof(string));

            searchFields.AddExactField(UUTField.SerialNumber, ReportReadState.InTest, "SN;", null, typeof(string));

            const string footerRegEx = @"^END;(?<Result>[^;]*);(?<Date>[^;]*);(?<Time>[^;]*)";
            regExField = searchFields.AddRegExpField("Footer", ReportReadState.InTest, footerRegEx, null, typeof(string), ReportReadState.InHeader);
            regExField.AddSubField("Result", typeof(UUTStatusType));
            regExField.AddSubField("Date", typeof(DateTime), "MM/dd/yyyy");
            regExField.AddSubField("Time", typeof(TimeSpan), "HH:mm:ss");
        }

        protected override string PreProcessLine(string line)
        {
            return line.Replace("Automatic", "0").Replace(";-NAN;", ";NaN;");
        }

        string prevStepRef;
        protected override bool ProcessMatchedLine(SearchFields.SearchMatch match, ref ReportReadState readState)
        {
            if (match == null)
                return true;

            if (match.matchField.fieldName == null && match.completeLine.StartsWith("START"))
            {
                apiRef.TestMode = TestModeType.Import;
                currentUUT.StartDateTime = (DateTime)match.GetSubField("StartDate") + (TimeSpan)match.GetSubField("StartTime");
                currentUUT.StartDateTimeUTC = currentUUT.StartDateTime.ToUniversalTime();
                currentUUT.PartNumber = converterArguments["partNumber"];
            }
            if (match.matchField.fieldName == "Footer")
            {
                DateTime endDate = (DateTime)match.GetSubField("Date") + (TimeSpan)match.GetSubField("Time");
                currentUUT.ExecutionTime = (endDate - currentUUT.StartDateTime).TotalSeconds;
                currentUUT.Status = (UUTStatusType)match.GetSubField("Result");
                SubmitUUT();
                CreateDefaultUUT();
                readState = ReportReadState.InHeader;
            }
            else if (match.matchField.fieldName == "Measure")
            {
                string compRef = (string)match.GetSubField("StepRef");
                string stepRef = GetNameFromRef(compRef);
                if (stepRef != prevStepRef)
                {
                    currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(stepRef);
                    prevStepRef = stepRef;
                }
                double measure = (double)match.GetSubField("Meas");
                double loLim = (double)match.GetSubField("LoLim");
                double hiLim = (double)match.GetSubField("HiLim");
                string unit = (string)match.GetSubField("Unit");
                string stepName = (string)match.GetSubField("StepName");
                StepStatusType stepStatus = ((string)match.GetSubField("Result")).StartsWith("PASS") ? StepStatusType.Passed : StepStatusType.Failed;
                Step step;

                if (measure == 0 && loLim == 0 && hiLim == 0)
                {
                    step = currentSequence.AddPassFailStep(stepName);
                    ((PassFailStep)step).AddTest(stepStatus == StepStatusType.Passed);
                }
                else
                {
                    NumericLimitStep s = currentSequence.AddNumericLimitStep(stepName);
                    step = s;
                    if (hiLim == 0)
                        s.AddTest(measure, Virinco.WATS.Interface.CompOperatorType.GE, loLim, unit);
                    else if (loLim == 0)
                        s.AddTest(measure, Virinco.WATS.Interface.CompOperatorType.LE, hiLim, unit);
                    else
                        s.AddTest(measure, Virinco.WATS.Interface.CompOperatorType.GELE, loLim, hiLim, unit);
                }
                if (stepStatus == StepStatusType.Failed)
                {
                    step.Status = stepStatus;
                    step.Parent.Status = step.Status;
                }
            }
            return true;
        }

        Dictionary<string, string> componentTypes = new Dictionary<string, string>()
        {
            {"R","Resistors" },
            {"RP","Resistors" },
            {"RS","Resistors" },
            {"C","Capacitors"},
            {"CS","Capacitors"},
            {"CP","Capacitors"},
            {"DSC","Capacitors"},
            {"L","Inductors" },
            {"D","Diodes" },
            {"Q","Transistors" },
            {"CON","Connectors" },
            {"SHO","Short" },
            {"TC","Thermocouple" },
            {"PTC","PTC" },
            {"DZ","ZenerDiodes" },
            {"F","Fuses" },
            {"J","Jumpers" },
            {"T","Transformers" },
            {"ISO","Isolators" },
            {"U","ICs" }
        };

        string GetNameFromRef(string refName)
        {
            Match match = new Regex("(?<Ref>[A-Za-z]+)").Match(refName);
            if (match.Success)
            {
                string type = match.Groups["Ref"].Value;
                if (componentTypes.ContainsKey(type))
                    return componentTypes[type];
                else
                    return type;
            }
            else
                return refName;
        }
    }
}
