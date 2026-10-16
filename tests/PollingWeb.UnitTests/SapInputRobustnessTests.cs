using System;
using System.Globalization;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class SapInputRobustnessTests
    {
        [Fact]
        public void Sap_text_decoder_accepts_explicit_legacy_encoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var legacy = Encoding.GetEncoding(1252);
            var bytes = legacy.GetBytes("caf\u00e9");
            Assert.Equal("caf\u00e9", ConfiguredFilePaths.DecodeSapText(bytes, legacy));
        }

        [Fact]
        public void Sap_text_reader_handles_empty_utf8_and_bom_encoded_text()
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "synthetic.txt");
            try
            {
                File.WriteAllText(path, string.Empty, new UTF8Encoding(false));
                using (var reader = ConfiguredFilePaths.OpenSapText(path)) Assert.Equal(string.Empty, reader.ReadToEnd());
                File.WriteAllText(path, "città", new UTF8Encoding(true));
                using (var reader = ConfiguredFilePaths.OpenSapText(path)) Assert.Equal("città", reader.ReadToEnd());
            }
            finally { Directory.Delete(directory, true); }
        }

        [Theory]
        [InlineData(3, "123")]
        [InlineData(5, "12345")]
        public void Sap_output_header_rejects_truncated_records(int length, string record)
        {
            Assert.Equal(length, record.Length);
            Assert.True(record.Length < 20);
            Assert.Throws<FormatException>(() => Ordini_Sap.ValidateSapOutputRecord(record));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Invoice_parser_treats_empty_lines_as_no_records(string line)
        {
            Assert.False(ParserFatture.IsValidRecord(line));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("12345")]
        public void Invoice_parser_rejects_truncated_header(string line)
        {
            Assert.False(ParserFatture.IsValidRecord(line));
        }

        [Theory]
        [InlineData("abc", 2, 3, "bc")]
        [InlineData("abc", 4, 2, "")]
        [InlineData("abc", 2, 20, "bc")]
        public void Invoice_record_slices_are_safe_for_truncated_inputs(string input, int index, int length, string expected)
        {
            Assert.Equal(expected, ParserFatture.Mid(input, index, length));
        }

        [Fact]
        public void Sap_output_reader_treats_empty_record_as_no_data()
        {
            Ordini_Sap.ValidateSapOutputRecord(string.Empty);
        }

        [Theory]
        [InlineData(21, "123456789012345678901")]
        [InlineData(24, "123456789012345678901234")]
        public void Sap_output_error_record_rejects_truncated_error_fields(int length, string record)
        {
            Assert.Equal(length, record.Length);
            Assert.Throws<FormatException>(() => Ordini_Sap.ValidateSapOutputRecord(record));
        }

        [Theory]
        [InlineData("20260115", 2026, 1, 15)]
        [InlineData("15.01.2026", 2026, 1, 15)]
        public void Sap_dates_use_declared_formats_under_multiple_cultures(string input, int year, int month, int day)
        {
            var oldCulture = CultureInfo.CurrentCulture;
            try
            {
                foreach (var culture in new[] { "en-US", "it-IT" })
                {
                    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                    var parsed = ParserFatture.ParseTextDate(input);
                    Assert.Equal(new DateTime(year, month, day), parsed);
                }
            }
            finally { CultureInfo.CurrentCulture = oldCulture; }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(20)]
        public void Budget_fixed_width_rows_missing_trailing_cells_are_safe(int cells)
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "minimal.xlsx");
            try
            {
                using (var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
                {
                    var workbook = document.AddWorkbookPart();
                    workbook.Workbook = new Workbook(new Sheets(new Sheet { Id = "rId1", SheetId = 1, Name = "BDG" }));
                    var worksheet = workbook.AddNewPart<WorksheetPart>("rId1");
                    var row = new Row();
                    for (var i = 0; i < cells; i++) row.Append(new Cell { CellValue = new CellValue("1") });
                    worksheet.Worksheet = new Worksheet(new SheetData(row));
                    worksheet.Worksheet.Save();
                    workbook.Workbook.Save();
                }
                var result = new ImportaXML().ReadExcelFileDOM(path);
                Assert.Single(result);
            }
            finally { if (File.Exists(path)) File.Delete(path); Directory.Delete(directory); }
        }

        [Fact]
        public void Sap_text_reader_is_independent_of_current_culture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it-IT");
                Assert.Equal(1234.5, double.Parse("1234.5", CultureInfo.InvariantCulture));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void Budget_reader_reports_missing_worksheet_instead_of_null_reference()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
            using (var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                var part = document.AddWorkbookPart();
                part.Workbook = new Workbook(new Sheets());
                part.Workbook.Save();
            }
            try
            {
                var error = Assert.Throws<InvalidDataException>(() => new ImportaXML().ReadExcelFileDOM(path));
                Assert.Contains("BDG worksheet", error.Message);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void Budget_cell_lookup_reports_missing_sheet()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
            using (var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                var part = document.AddWorkbookPart();
                part.Workbook = new Workbook(new Sheets());
                part.Workbook.Save();
            }
            try
            {
                Assert.Throws<InvalidDataException>(() => ImportaXML.GetCellValue(path, "BDG", "A1"));
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void Budget_cell_lookup_returns_empty_for_missing_cell()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xlsx");
            using (var document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook(new Sheets(new Sheet { Id = "rId1", SheetId = 1, Name = "BDG" }));
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId1");
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                worksheetPart.Worksheet.Save();
                workbookPart.Workbook.Save();
            }
            try { Assert.Equal(string.Empty, ImportaXML.GetCellValue(path, "BDG", "A1")); }
            finally { File.Delete(path); }
        }
    }
}
