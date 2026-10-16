#!/usr/bin/env python3
"""Genera un file XLSX sintetico per il flusso di import budget.

Perché uno script e non un file binario "a caso": il campione deve essere
riproducibile e verificabile. Chiunque può rigenerarlo e confrontare i bytes.

Formato atteso da `PollingClass/ImportaXML.ReadExcelFileDOM`:
  * nome del foglio: "BDG"
  * 7 colonne, tutte come stringhe condivise (shared strings):
        1 IST          istituto (codice istituto)
        2 CDR          centro di responsabilità
        3 VOCE         voce di budget
        4 BDGRILASCI   budget rilasciato
        5 BDGRESIDUO   budget residuo
        6 CODFONDO     codice fondo
        7 DATARIF      data di riferimento

Uso:
    python3 samples/tools/make-budget-sample.py [output.xlsx]
Default output: samples/budget/BDG_sample.xlsx
"""

from __future__ import annotations

import sys
import zipfile
from pathlib import Path
from xml.sax.saxutils import escape

SHEET_NAME = "BDG"

# Righe completamente inventate: istituto 1, centri di responsabilità fittizi.
ROWS: list[list[str]] = [
    ["IST", "CDR", "VOCE", "BDGRILASCI", "BDGRESIDUO", "CODFONDO", "DATARIF"],
    ["1", "DEMO01", "V0001", "1000.00", "1000.00", "FONDO-A", "2024-01-01"],
    ["1", "DEMO01", "V0002", "2500.50", "1250.25", "FONDO-A", "2024-01-01"],
    ["1", "DEMO02", "V0001", "750.00", "0.00", "FONDO-B", "2024-01-01"],
    ["2", "DEMO03", "V0003", "10000.00", "9500.00", "FONDO-B", "2024-01-01"],
]

CONTENT_TYPES = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
  <Default Extension="xml" ContentType="application/xml"/>
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
  <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
</Types>
"""

ROOT_RELS = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
</Relationships>
"""

WORKBOOK = f"""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="{SHEET_NAME}" sheetId="1" r:id="rId1"/>
  </sheets>
</workbook>
"""

WORKBOOK_RELS = """<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
</Relationships>
"""


def column_name(index: int) -> str:
    """1 -> A, 26 -> Z, 27 -> AA."""
    name = ""
    while index > 0:
        index, rem = divmod(index - 1, 26)
        name = chr(ord("A") + rem) + name
    return name


def build() -> tuple[bytes, bytes, bytes]:
    """Restituisce (sharedStrings.xml, sheet1.xml, indice delle stringhe)."""
    index: dict[str, int] = {}

    def shared(value: str) -> int:
        if value not in index:
            index[value] = len(index)
        return index[value]

    rows_xml = []
    for r, row in enumerate(ROWS, start=1):
        cells = []
        for c, value in enumerate(row, start=1):
            ref = f"{column_name(c)}{r}"
            cells.append(f'<c r="{ref}" t="s"><v>{shared(value)}</v></c>')
        rows_xml.append(f'<row r="{r}">{"".join(cells)}</row>')

    sheet = (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'
        f'<sheetData>{"".join(rows_xml)}</sheetData>'
        "</worksheet>"
    )

    items = sorted(index.items(), key=lambda kv: kv[1])
    sst = (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        f'<sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" '
        f'count="{len(items)}" uniqueCount="{len(items)}">'
        + "".join(f"<si><t>{escape(v)}</t></si>" for v, _ in items)
        + "</sst>"
    )

    return sst.encode("utf-8"), sheet.encode("utf-8"), b""


def main() -> int:
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("samples/budget/BDG_sample.xlsx")
    out.parent.mkdir(parents=True, exist_ok=True)
    sst, sheet, _ = build()
    with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
        z.writestr("[Content_Types].xml", CONTENT_TYPES)
        z.writestr("_rels/.rels", ROOT_RELS)
        z.writestr("xl/workbook.xml", WORKBOOK)
        z.writestr("xl/_rels/workbook.xml.rels", WORKBOOK_RELS)
        z.writestr("xl/sharedStrings.xml", sst)
        z.writestr("xl/worksheets/sheet1.xml", sheet)
    print(f"scritto {out} ({out.stat().st_size} byte), foglio '{SHEET_NAME}', {len(ROWS) - 1} righe dati")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
