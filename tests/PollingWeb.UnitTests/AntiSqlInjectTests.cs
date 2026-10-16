using Database;
using Xunit;

namespace PollingWeb.UnitTests
{
    /// <summary>
    /// Test del controllo antinjection.
    /// <para>
    /// Non sono test decorativi: fissano il comportamento di un <b>controllo di sicurezza</b>
    /// che è stato modificato in <c>X1.2</c>. Lì <c>HttpUtility.UrlDecode</c> (disponibile solo
    /// su .NET Framework) è stato sostituito da <c>Uri.UnescapeDataString</c> più la conversione
    /// di <c>+</c> in spazio, per eliminare l'ultima dipendenza da <c>System.Web</c> nella DAL.
    /// In quella modifica ho asserito l'equivalenza su 8 casi: questi test la <b>fissano</b>,
    /// così una futura modifica alla decodifica non può passare inosservata.
    /// </para>
    /// </summary>
    public class AntiSqlInjectTests
    {
        private static bool Safe(string input) => AntiSqlInject.Instance.IsSafetySql(input);

        [Theory]
        [InlineData("' OR 1=1 --")]
        [InlineData("1' or '1'='1")]
        [InlineData("SELECT * FROM utenti")]
        [InlineData("select nome from clienti")]
        [InlineData("x; drop table tool")]
        [InlineData("UNION SELECT password")]
        public void Payload_noti_sono_considerati_non_sicuri(string payload)
        {
            Assert.False(Safe(payload), $"payload non riconosciuto come non sicuro: {payload}");
        }

        [Theory]
        [InlineData("Fornitore Sintetico SRL")]
        [InlineData("2.500,50")]
        [InlineData("città con accenti")]
        [InlineData("a-b_c.d")]
        public void Testo_ordinario_e_considerato_sicuro(string input)
        {
            Assert.True(Safe(input), $"testo ordinario rifiutato: {input}");
        }

        [Fact]
        public void Input_vuoto_o_nullo_e_considerato_sicuro()
        {
            // Il controllo non deve bloccare l'assenza di valore: non c'e' nulla da iniettare.
            Assert.True(Safe(null));
            Assert.True(Safe(""));
            Assert.True(Safe("   "));
        }

        [Theory]
        // Il payload è percent-encoded: senza la decodifica non verrebbe riconosciuto.
        // Questi due casi sono la prova che la decodifica avviene PRIMA del controllo.
        [InlineData("1%27%20or%20%271%27%3D%271")]     // 1' or '1'='1
        [InlineData("%3Bdrop%20table%20tool")]          // ;drop table tool
        public void Payload_percent_encoded_viene_decodificato_prima_del_controllo(string encoded)
        {
            Assert.False(Safe(encoded), $"payload percent-encoded non riconosciuto: {encoded}");
        }

        [Theory]
        // UrlDecode converte '+' in spazio, Uri.UnescapeDataString no: senza la conversione
        // esplicita questi payload passerebbero. È la differenza che X1.2 doveva replicare.
        [InlineData("SELECT+*+FROM+utenti")]
        [InlineData("x+UNION+SELECT+y")]
        public void Il_piu_viene_convertito_in_spazio(string encoded)
        {
            Assert.False(Safe(encoded), $"il '+' non è stato convertito in spazio: {encoded}");
        }

        [Theory]
        [InlineData("' OR 1=1 --")]
        [InlineData("1' or '1'='1")]
        [InlineData("x; drop table tool")]
        [InlineData("SELECT * FROM utenti")]
        [InlineData("50%")]
        public void GetSafetySql_rende_sicuro_il_payload(string payload)
        {
            // Proprietà forte e verificabile: qualunque cosa restituisca il sanificatore,
            // deve superare il controllo di sicurezza. È il contratto del metodo.
            //
            // Verificato empiricamente: GetSafetySql NON restituisce stringa vuota —
            // rimuove le parole chiave e i separatori e restituisce ciò che resta
            // (es. "' OR 1=1 --" -> " 1=1 "). La prima stesura di questo test asseriva
            // il contrario ed è fallita: l'osservazione ha corretto l'assunzione.
            var risultato = AntiSqlInject.Instance.GetSafetySql(payload);
            Assert.True(AntiSqlInject.Instance.IsSafetySql(risultato),
                $"il risultato del sanificatore non è sicuro: '{risultato}' da '{payload}'");
            Assert.NotEqual(payload, risultato);
        }

        [Theory]
        // Valori attuali, osservati: fissano il comportamento come regressione.
        [InlineData("' OR 1=1 --", " 1=1 ")]
        [InlineData("SELECT * FROM utenti", "utenti")]
        public void GetSafetySql_rimuove_parole_chiave_e_separatori(string payload, string atteso)
        {
            Assert.Equal(atteso, AntiSqlInject.Instance.GetSafetySql(payload));
        }

        [Fact]
        public void GetSafetySql_lascia_passare_il_testo_ordinario()
        {
            const string input = "Fornitore Sintetico SRL";
            Assert.Equal(input, AntiSqlInject.Instance.GetSafetySql(input));
        }

        /// <summary>
        /// <b>Documenta un difetto, non lo approva.</b>
        ///
        /// Il controllo tratta <c>%</c> e <c>#</c> come separatori SQL (lo sono nei commenti
        /// e nei caratteri jolly), quindi li rimuove anche da testo legittimo: una
        /// descrizione contenente «sconto 50%» diventa «sconto 50». Sui tracciati SAP, dove
        /// percentuali e codici con <c>#</c> sono comuni, è un rischio funzionale.
        ///
        /// È materia dei task <c>C1</c>–<c>C3</c> (parametrizzazione al posto del filtraggio).
        /// Il test fissa il comportamento attuale: quando il filtraggio sarà sostituito,
        /// dovrà essere aggiornato — ed è il segnale che si vuole ottenere.
        /// </summary>
        [Theory]
        // Valori attuali, osservati con un probe sul comportamento reale.
        // Nota: quando il payload è considerato non sicuro, il sanificatore lo abbassa
        // di caso prima di rimuovere: "codice A#1" diventa "codice a1".
        [InlineData("sconto 50%", "sconto 50")]
        [InlineData("codice A#1", "codice a1")]
        public void Il_filtraggio_rimuove_caratteri_leggittimi_difetto_C1(string input, string atteso)
        {
            Assert.Equal(atteso, AntiSqlInject.Instance.GetSafetySql(input));
        }
    }
}
