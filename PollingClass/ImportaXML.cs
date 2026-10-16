using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Database;

namespace PollingClass
{
   
    public class ImportaXML
    {
       

        public List<Budget> ReadExcelFileDOM(string fileName)
        {
            

            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                Sheet theSheet = workbookPart?.Workbook?.Descendants<Sheet>()
                    .FirstOrDefault(s => s.Name == "BDG");
                if (theSheet == null) throw new InvalidDataException("Workbook is missing the required BDG worksheet.");
                WorksheetPart wsPart = (WorksheetPart)workbookPart.GetPartById(theSheet.Id);
                SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                if (sheetData == null) throw new InvalidDataException("The BDG worksheet has no sheet data.");

                string value = null;
                Budget bdg;
                List<Budget> budgets = new List<Budget>();
                int i = 0;
                foreach (Row r in sheetData.Elements<Row>())
                {
                    i = 0;
                    bdg = new Budget();
                    foreach (Cell c in r.Elements<Cell>())
                    {
                        i++;
                        //text = c.CellValue.Text;
                        //Console.Write(text + " ");
                        if (!string.IsNullOrEmpty(c.InnerText))
                        {
                            value = c.InnerText;                            
                            if (c.DataType != null)
                            {
                                switch (c.DataType.Value)
                                {
                                    case CellValues.SharedString:

                                    var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                                   
                                    if (stringTable == null || !int.TryParse(value, out var sharedStringIndex)
                                        || sharedStringIndex < 0 || sharedStringIndex >= stringTable.SharedStringTable.Count())
                                        throw new InvalidDataException("Workbook contains an invalid shared-string reference.");
                                    value = stringTable.SharedStringTable.ElementAt(sharedStringIndex).InnerText;
                                    break;                                   
                                }
                            }
                            else if (c.CellValue != null)
                            {
                                value = c.CellValue.Text;
                            }


                            switch (i)
                            {
                                case 1://IST
                                    bdg.IST = value;
                                    break;

                                case 2://CDR
                                    bdg.CDR = value;
                                    break;

                                case 3://VOCE
                                    bdg.VOCE = value;
                                    break;

                                case 4://BDGRILASCI
                                    bdg.BDGRILASCI = value;
                                    break;

                                case 5://BDGRESIDUO
                                    bdg.BDGRESIDUO = value;
                                    break;

                                case 6://CODFONDO
                                    bdg.CODFONDO = value;
                                    break;

                                case 7://DATARIF
                                    bdg.DATARIF = value;
                                    break;
                            }
                        }
                        Console.WriteLine(value);
                    }
                    budgets.Add(bdg);
                }

                return budgets;
            }
            
        }
        public void Esegui(string fileName)
        {
            try
            {
                const string sheet = "BDG";
                // Il percorso del file arriva dal chiamante: nessun percorso assoluto
                // o cartella personale nel codice (vincolo C8).

                // Retrieve the value in cell A1.
                string value = GetCellValue(fileName, sheet, "A1");
                Console.WriteLine(value);
                // Retrieve the date value in cell A2.
                value = GetCellValue(fileName, sheet, "A2");
                Console.WriteLine(
                DateTime.FromOADate(double.Parse(value, CultureInfo.InvariantCulture)).ToShortDateString());
            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }

            
        }

        public static string GetCellValue(string fileName,
        string sheetName,
        string addressName)
        {
            string value = null;

            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document =
                SpreadsheetDocument.Open(fileName, false))
            {
                // Retrieve a reference to the workbook part.
                WorkbookPart wbPart = document.WorkbookPart;

                Sheet theSheet = wbPart?.Workbook?.Descendants<Sheet>()
                    .FirstOrDefault(s => s.Name == sheetName);

                // Throw an exception if there is no sheet.
                if (theSheet == null)
                    throw new InvalidDataException("Workbook is missing the requested worksheet.");

                // Retrieve a reference to the worksheet part.
                WorksheetPart wsPart =
                    (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet?.Descendants<Cell>()
                    .FirstOrDefault(c => c.CellReference == addressName);

                // An absent or empty cell has no value.
                if (theCell == null || string.IsNullOrEmpty(theCell.InnerText))
                    return string.Empty;
                value = theCell.InnerText;
                if (theCell.DataType == CellValues.SharedString)
                {
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    if (stringTable == null || !int.TryParse(value, out var sharedStringIndex)
                        || sharedStringIndex < 0 || sharedStringIndex >= stringTable.SharedStringTable.Count())
                        throw new InvalidDataException("Workbook contains an invalid shared-string reference.");
                    value = stringTable.SharedStringTable.ElementAt(sharedStringIndex).InnerText;
                }
            }
            return value;
        }
    }


    
}

