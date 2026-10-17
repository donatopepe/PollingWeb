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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-an-address")]
        public void Aggset_query_wrapper_preserves_inner_failure_without_database(string sql)
        {
            var error = Assert.Throws<InvalidOperationException>(() =>
                Database.TableInterface.Aggset.EseguiQuery("synthetic", sql, null));
            Assert.Equal("EseguiQuery failed.", error.Message);
            Assert.NotNull(error.InnerException);
        }

        [Fact]
        public void Aggset_command_wrapper_preserves_inner_failure_without_database()
        {
            var error = Assert.Throws<InvalidOperationException>(() =>
                Database.TableInterface.Aggset.Eseguicomando("synthetic command", null));
            Assert.Equal("Eseguicomando failed.", error.Message);
            Assert.NotNull(error.InnerException);
        }

        [Theory]
        [InlineData("31/12/2024", true)]
        [InlineData("2024-12-31", true)]
        [InlineData("not-a-date", false)]
        [InlineData("2024-13-45", false)]
        public void ValidaData_copre_formati_iso_e_input_non_validi(string value, bool expected)
        {
            Assert.Equal(expected, ValidaData.Esegui(value));
        }

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

        [Theory]
        [InlineData("2024-01-31")]
        [InlineData("2024-01-31T12:30:00")]
        public void ValidaData_iso_formats_are_parsed_invariantly(string input)
        {
            ConCultura("en-US", () =>
            {
                Assert.True(ValidaData.Esegui(input));
                Assert.Equal("31/01/2024", ValidaData.Valore());
            });
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

        [Theory]
        [InlineData("en-US")]
        [InlineData("it-IT")]
        public void ValidaData_dd_mm_yyyy_ha_esito_indipendente_dalla_cultura(string culture)
        {
            ConCultura(culture, () =>
            {
                Assert.True(ValidaData.Esegui("31/12/2024"));
                Assert.Equal("31/12/2024", ValidaData.Valore());
                Assert.Equal("2024-12-31", ValidaData.FEvalore());
            });
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
        [InlineData("1.5", 1, true)]
        [InlineData("1234.567", 2, true)]
        [InlineData("not-a-number", 2, false)]
        [InlineData("1", 0, true)]
        public void IsNumeric_gestisce_precisione_e_input_invalidi(string valore, int decimali, bool expected)
        {
            Assert.Equal(expected, IsNumeric.Esegui(valore, decimali));
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

        [Theory]
        [InlineData("en-US")]
        [InlineData("it-IT")]
        public void IsNumeric_usa_formato_invariant_sotto_culture_diverse(string culture)
        {
            ConCultura(culture, () =>
            {
                Assert.True(IsNumeric.Esegui("1234.56"));
                Assert.Equal(1234.56, IsNumeric.Valore(), 2);
                Assert.Equal("1234.56", IsNumeric.FEFormattato());
            });
        }
    }
}
