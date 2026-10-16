using System;
using System.Globalization;
using Database;
using Xunit;

namespace PollingWeb.UnitTests
{
    /// <summary>
    /// Test di validazione di date, email e numeri.
    ///
    /// <para>
    /// La classe è in una collection dedicata perché i tipi sotto test — <c>ValidaData</c>,
    /// <c>IsNumeric</c> — conservano il risultato in <b>campi statici mutabili</b>
    /// (<c>valoreRet</c>, <c>valoreRetformat</c>). È il difetto tracciato dal task <c>C5</c>:
    /// finché non è corretto, i test che leggono <c>Valore()</c> dopo <c>Esegui()</c> non
    /// possono girare in parallelo con altri che chiamano gli stessi metodi. La collection
    /// lo rende esplicito invece di lasciarlo al caso.
    /// </para>
    /// </summary>
    [Collection("stato statico condiviso")]
    public class ValidationTests
    {
        /// <summary>
        /// Esegue il corpo con una cultura impostata e la ripristina. Necessario perché il
        /// codice sotto test converte stringhe in date e numeri con la cultura corrente:
        /// senza fissarla, lo stesso test passerebbe o fallirebbe a seconda della macchina.
        /// </summary>
        private static void ConCultura(string nomeCultura, Action corpo)
        {
            var precedente = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(nomeCultura);
                corpo();
            }
            finally
            {
                CultureInfo.CurrentCulture = precedente;
            }
        }

        // ------------------------------------------------------------------ ValidaData

        [Fact]
        public void ValidaData_accetta_il_valore_assente()
        {
            // Non c'e' nulla da validare: il codice considera validi null, stringa vuota e
            // il segnaposto della maschera di input.
            Assert.True(ValidaData.Esegui(null));
            Assert.True(ValidaData.Esegui(""));
            Assert.True(ValidaData.Esegui("  /  /    "));
        }

        [Fact]
        public void ValidaData_rifiuta_una_stringa_troppo_corta()
        {
            // Scorciatoia esplicita del codice: meno di 10 caratteri non può essere una data.
            Assert.False(ValidaData.Esegui("01/01"));
            Assert.False(ValidaData.Esegui("x"));
        }

        [Fact]
        public void ValidaData_rifiuta_una_data_impossibile()
        {
            ConCultura("it-IT", () => Assert.False(ValidaData.Esegui("2024-13-45")));
        }

        [Fact]
        public void ValidaData_con_cultura_italiana_legge_gg_mm_aaaa()
        {
            ConCultura("it-IT", () =>
            {
                Assert.True(ValidaData.Esegui("31/12/2024"));
                Assert.Equal("31/12/2024", ValidaData.Valore());
                Assert.Equal("2024-12-31", ValidaData.FEvalore());
            });
        }

        /// <summary>
        /// <b>Documenta un difetto, non lo approva.</b>
        ///
        /// L'interpretazione della data dipende dalla cultura del thread: lo stesso valore
        /// che con cultura italiana è il 31 dicembre, con cultura statunitense viene letto
        /// come mese 31 e quindi rifiutato. È il problema tracciato dal task <c>C4</c>
        /// («eliminazione di <c>Convert.ToDateTime(string)</c> culture-dependent»).
        ///
        /// Il test asserisce il comportamento <b>attuale</b> per fissarlo: quando <c>C4</c>
        /// renderà il parsing indipendente dalla cultura, questo test dovrà essere aggiornato
        /// — ed è proprio il segnale che si vuole ottenere.
        /// </summary>
        [Fact]
        public void ValidaData_dipende_dalla_cultura_difetto_C4()
        {
            ConCultura("en-US", () => Assert.False(ValidaData.Esegui("31/12/2024")));
            ConCultura("it-IT", () => Assert.True(ValidaData.Esegui("31/12/2024")));
        }

        // ------------------------------------------------------------------ ValidaMail

        [Theory]
        [InlineData("utente@example.invalid")]
        [InlineData("nome.cognome@example.invalid")]
        [InlineData("a+b@example.invalid")]
        public void ValidaMail_accetta_un_indirizzo_valido(string email)
        {
            Assert.True(ValidaMail.IsValidEmail(email));
        }

        [Theory]
        [InlineData("utente@example.invalid;altro@example.invalid")]
        [InlineData("utente@example.invalid,altro@example.invalid")]
        [InlineData("utente@example.invalid ; altro@example.invalid")]
        public void ValidaMail_accetta_piu_indirizzi_separati_da_punto_e_virgola_o_virgola(string email)
        {
            // Il codice normalizza ',' in ';' e rimuove gli spazi prima di validare ogni parte.
            Assert.True(ValidaMail.IsValidEmail(email));
        }

        [Theory]
        [InlineData("non-una-email")]
        [InlineData("@example.invalid")]
        [InlineData("utente@")]
        [InlineData("utente@example.invalid;rott@")]
        [InlineData("utente@example.invalid;non-valida")]
        public void ValidaMail_rifiuta_un_indirizzo_non_valido(string email)
        {
            Assert.False(ValidaMail.IsValidEmail(email));
        }

        [Fact(Skip = "Difetto noto: IsValidEmail(null) genera NullReferenceException perché " +
                     "email.Replace(...) è fuori dal blocco try. Da correggere in C6 " +
                     "(gestione degli errori); quando sarà corretto, rimuovere lo Skip: " +
                     "l'asserzione qui sotto descrive il comportamento voluto.")]
        public void ValidaMail_non_deve_generare_eccezioni_su_input_nullo()
        {
            Assert.False(ValidaMail.IsValidEmail(null));
        }

        // ------------------------------------------------------------------ IsNumeric

        [Theory]
        [InlineData("0")]
        [InlineData("123")]
        [InlineData("-45")]
        [InlineData("1.234")]
        public void IsNumeric_accetta_un_numero_valido(string valore)
        {
            ConCultura("it-IT", () => Assert.True(IsNumeric.Esegui(valore)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("12,34,56")]
        public void IsNumeric_rifiuta_un_valore_non_numerico(string valore)
        {
            ConCultura("it-IT", () => Assert.False(IsNumeric.Esegui(valore)));
        }

        [Fact]
        public void IsNumeric_converte_il_punto_in_separatore_decimale()
        {
            // Il codice fa valore.Replace('.', ',') prima di convertire: è la sua difesa
            // contro il separatore decimale dei tracciati SAP. Fissa il comportamento.
            ConCultura("it-IT", () =>
            {
                Assert.True(IsNumeric.Esegui("1234.56"));
                Assert.Equal(1234.56, IsNumeric.Valore(), 2);
            });
        }
    }
}
