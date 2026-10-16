using System;

namespace PollingClass
{
    /// <summary>
    /// Riga di un'estrazione di budget, così com'è letta da un foglio "BDG".
    /// <para>
    /// Era definita in <c>ImportaExcel.cs</c> insieme a <c>ConnexionExcel</c>, la classe che
    /// dipendeva da <c>LinqToExcel</c> (licenza LGPL, Windows-only). L'estrazione in un file
    /// proprio permette di eliminare quella dipendenza — mai usata se non in un commento —
    /// senza perdere il tipo <c>Budget</c>, che invece è usato da <c>ImportaXML</c>.
    /// </para>
    /// <para>
    /// Classe di soli dati: nessuna dipendenza di piattaforma.
    /// I membri si chiamano come le colonne attese dal parser
    /// (<see cref="ImportaXML.ReadExcelFileDOM"/>), non come vorrebbe una convenzione di
    /// naming: rinominarli richiede di aggiornare il parser e i chiamanti in un solo passo.
    /// </para>
    /// </summary>
    public class Budget
    {
        /// <summary>Codice istituto.</summary>
        public string IST { get; set; }

        /// <summary>Centro di responsabilità.</summary>
        public string CDR { get; set; }

        /// <summary>Voce di budget.</summary>
        public string VOCE { get; set; }

        /// <summary>Budget rilasciato.</summary>
        public string BDGRILASCI { get; set; }

        /// <summary>Budget residuo.</summary>
        public string BDGRESIDUO { get; set; }

        /// <summary>Codice fondo.</summary>
        public string CODFONDO { get; set; }

        /// <summary>Data di riferimento.</summary>
        public string DATARIF { get; set; }
    }
}
