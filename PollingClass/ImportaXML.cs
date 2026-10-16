using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                //WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                //WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                //SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                Sheet theSheet = workbookPart.Workbook.Descendants<Sheet>().
                 Where(s => s.Name == "BDG").FirstOrDefault();
                WorksheetPart wsPart =
                    (WorksheetPart)(workbookPart.GetPartById(theSheet.Id));
                SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().First();

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
                        if (c.InnerText.Length > 0)
                        {
                            value = c.InnerText;                            
                            if (c.DataType != null)
                            {
                                switch (c.DataType.Value)
                                {
                                    case CellValues.SharedString:

                                    var stringTable =
                                        workbookPart.GetPartsOfType<SharedStringTablePart>()
                                        .FirstOrDefault();

                                   
                                    if (stringTable != null)
                                    {
                                        value =
                                            stringTable.SharedStringTable
                                            .ElementAt(int.Parse(value)).InnerText;
                                    }
                                    break;                                   
                                }
                            }
                            else
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
                DateTime.FromOADate(double.Parse(value)).ToShortDateString());
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

                // Find the sheet with the supplied name, and then use that 
                // Sheet object to retrieve a reference to the first worksheet.
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                // Throw an exception if there is no sheet.
                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                // Retrieve a reference to the worksheet part.
                WorksheetPart wsPart =
                    (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet.Descendants<Cell>().
                  Where(c => c.CellReference == addressName).FirstOrDefault();

                // If the cell does not exist, return an empty string.
                if (theCell.InnerText.Length > 0)
                {
                    value = theCell.InnerText;

                    // If the cell represents an integer number, you are done. 
                    // For dates, this code returns the serialized value that 
                    // represents the date. The code handles strings and 
                    // Booleans individually. For shared strings, the code 
                    // looks up the corresponding value in the shared string 
                    // table. For Booleans, the code converts the value into 
                    // the words TRUE or FALSE.
                    if (theCell.DataType != null)
                    {
                        switch (theCell.DataType.Value)
                        {
                            case CellValues.SharedString:

                            // For shared strings, look up the value in the
                            // shared strings table.
                            var stringTable =
                                wbPart.GetPartsOfType<SharedStringTablePart>()
                                .FirstOrDefault();

                            // If the shared string table is missing, something 
                            // is wrong. Return the index that is in
                            // the cell. Otherwise, look up the correct text in 
                            // the table.
                            if (stringTable != null)
                            {
                                value =
                                    stringTable.SharedStringTable
                                    .ElementAt(int.Parse(value)).InnerText;
                            }
                            break;

                            //case CellValues.Boolean:
                            //switch (value)
                            //{
                            //    case "0":
                            //    value = "FALSE";
                            //    break;
                            //    default:
                            //    value = "TRUE";
                            //    break;
                            //}
                            //break;
                        }
                    }
                }
            }
            return value;
        }
    }


    
}

