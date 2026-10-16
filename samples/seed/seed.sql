-- =============================================================================
-- seed.sql — dati sintetici minimi per lo sviluppo e i test
-- =============================================================================
-- ⚠️  DATI INVENTATI. Nessun dato reale di clienti, fornitori o dipendenti.
--     I codici fiscali/P.IVA sono segnaposto evidenti.
--
-- Scopo: permettere l'avvio dell'applicazione e l'esecuzione dei test di integrazione
-- su un database vuoto, senza dipendere da dati di produzione.
--
-- Idempotente: puo' essere rieseguito senza errori.
--
-- Uso:
--   sqlcmd -S <server> -d Polling -i samples/seed/seed.sql
--   (oppure tramite scripts/db-deploy.sh, che lo richiama in modalita' development)
-- =============================================================================
SET NOCOUNT ON;
GO

-- ------------------------------------------------------------------ DimStato
-- Codici di esito usati da LogTaskService.Codice_stato.
MERGE dbo.DimStato WITH (HOLDLOCK) AS t
USING (VALUES
    (0,  N'Non eseguito'),
    (1,  N'In corso'),
    (2,  N'Completato'),
    (3,  N'Completato con errori'),
    (9,  N'Errore'),
    (99, N'Annullato')
) AS s (Stato, Descrizione_Stato)
    ON t.Stato = s.Stato
WHEN NOT MATCHED THEN
    INSERT (Stato, Descrizione_Stato) VALUES (s.Stato, s.Descrizione_Stato);
GO

-- --------------------------------------------------------------- DimProcessi
-- Codici di processo di esempio. Nell'installazione reale questi valori dipendono
-- dal contratto con SAP e vanno concordati: NON sono un elenco definitivo.
MERGE dbo.DimProcessi WITH (HOLDLOCK) AS t
USING (VALUES
    (N'RDA', N'Richiesta di acquisto',       1),
    (N'ODA', N'Ordine di acquisto',          1),
    (N'BDG', N'Import budget',               2),
    (N'FAT', N'Import fatture',              2),
    (N'FOR', N'Anagrafica fornitori',        3)
) AS s (IdProcesso, Descrizione, Tipo)
    ON t.IdProcesso = s.IdProcesso
WHEN NOT MATCHED THEN
    INSERT (IdProcesso, Descrizione, Tipo) VALUES (s.IdProcesso, s.Descrizione, s.Tipo);
GO

-- -------------------------------------------------------------- DimDayquartz
-- Espressioni del campo day-of-week di Quartz (usate in Day_Check_File).
MERGE dbo.DimDayquartz WITH (HOLDLOCK) AS t
USING (VALUES
    (N'MON-FRI', N'Lunedi''-Venerdi'''),
    (N'MON-SAT', N'Lunedi''-Sabato'),
    (N'MON',     N'Lunedi'''),
    (N'TUE',     N'Martedi'''),
    (N'WED',     N'Mercoledi'''),
    (N'THU',     N'Giovedi'''),
    (N'FRI',     N'Venerdi'''),
    (N'SAT',     N'Sabato'),
    (N'SUN',     N'Domenica'),
    (N'*',       N'Tutti i giorni')
) AS s (DayQuartz, Descrizione)
    ON t.DayQuartz = s.DayQuartz
WHEN NOT MATCHED THEN
    INSERT (DayQuartz, Descrizione) VALUES (s.DayQuartz, s.Descrizione);
GO

-- ---------------------------------------------------------- TargetSC00G100
-- Anagrafica fornitori: corrisponde al campione samples/sap/SC00G100.sample.txt.
MERGE dbo.TargetSC00G100 WITH (HOLDLOCK) AS t
USING (VALUES
    (CAST(1 AS SMALLINT), 1001, N'FORNITORE SINTETICO UNO SRL',  N'VIA DELLE PROVE 1',  N'RM', N'00100', N'ROMA',    N'00000000001', N'00000000001'),
    (CAST(1 AS SMALLINT), 1002, N'FORNITORE SINTETICO DUE SPA',  N'CORSO ESEMPIO 22',   N'MI', N'20100', N'MILANO',  N'00000000002', N'00000000002'),
    (CAST(1 AS SMALLINT), 1003, N'FORNITORE SINTETICO TRE SNC',  N'PIAZZA MODELLO 3',   N'TO', N'10100', N'TORINO',  N'00000000003', N'00000000003'),
    (CAST(1 AS SMALLINT), 1004, N'FORNITORE SINTETICO QUATTRO',  N'VIA FITTIZIA 4',     N'NA', N'80100', N'NAPOLI',  N'00000000000000000', N'00000000004'),
    (CAST(1 AS SMALLINT), 1005, N'FORNITORE SINTETICO CINQUE',   N'VIA INVENTATA 5',    N'FI', N'50100', N'FIRENZE', N'00000000005', N'00000000005')
) AS s (Istituto, Cfor, Ragione_Sociale, Indirizzo, Prov, Cap, Citta, Codfis, Piva)
    ON t.Istituto = s.Istituto AND t.Cfor = s.Cfor
WHEN MATCHED THEN
    UPDATE SET t.Ragione_Sociale = s.Ragione_Sociale, t.Indirizzo = s.Indirizzo,
               t.Prov = s.Prov, t.Cap = s.Cap, t.Citta = s.Citta,
               t.Codfis = s.Codfis, t.Piva = s.Piva
WHEN NOT MATCHED THEN
    INSERT (Istituto, Cfor, Ragione_Sociale, Indirizzo, Prov, Cap, Citta, Codfis, Piva)
    VALUES (s.Istituto, s.Cfor, s.Ragione_Sociale, s.Indirizzo, s.Prov, s.Cap, s.Citta, s.Codfis, s.Piva);
GO

-- ------------------------------------------------------------------- Tool
MERGE dbo.Tool WITH (HOLDLOCK) AS t
USING (VALUES (N'seed development')) AS s (Name)
    ON t.Name = s.Name
WHEN NOT MATCHED THEN
    INSERT (Name) VALUES (s.Name);
GO

-- -------------------------------------------------------------- TaskService
-- Percorsi relativi alla radice di lavoro: NON usare percorsi assoluti di macchina
-- (vincolo C8). In sviluppo la radice e' ./samples.
MERGE dbo.TaskService WITH (HOLDLOCK) AS t
USING (VALUES
    (CAST(1 AS SMALLINT), N'RDA', N'Lettura RDA',
     N'samples/sap',      N'rda_input.sample',      CAST('08:30:00' AS DATETIME), N'MON-FRI',
     CAST(10 AS SMALLINT), CAST(0 AS SMALLINT),
     N'polling@example.invalid', N'errori@example.invalid',
     N'samples/archive',  N'esiti@example.invalid', CAST(0 AS BIT)),
    (CAST(1 AS SMALLINT), N'ODA', N'Invio ordini SAP',
     N'samples/sap',      N'oda_esiti_input.sample', CAST('08:35:00' AS DATETIME), N'MON-FRI',
     CAST(10 AS SMALLINT), CAST(0 AS SMALLINT),
     N'polling@example.invalid', N'errori@example.invalid',
     N'samples/archive',  N'esiti@example.invalid', CAST(0 AS BIT)),
    (CAST(1 AS SMALLINT), N'BDG', N'Import budget',
     N'samples/budget',   N'BDG_sample.xlsx',        CAST('09:00:00' AS DATETIME), N'MON-FRI',
     CAST(60 AS SMALLINT), CAST(0 AS SMALLINT),
     N'polling@example.invalid', N'errori@example.invalid',
     N'samples/archive',  N'esiti@example.invalid', CAST(0 AS BIT))
) AS s (Istituto, IdProcesso, Descrizione, Path_Source, File_Source, Orario_Check_File,
        Day_Check_File, Polling_Intervall, Cutoff, Mail_Mittente, Mail_Failure,
        Path_Archiviazione, Mail_success, LasciaInSource)
    ON t.Istituto = s.Istituto AND t.IdProcesso = s.IdProcesso
WHEN MATCHED THEN
    UPDATE SET t.Descrizione = s.Descrizione, t.Path_Source = s.Path_Source,
               t.File_Source = s.File_Source, t.Orario_Check_File = s.Orario_Check_File,
               t.Day_Check_File = s.Day_Check_File,
               t.Polling_Intervall = s.Polling_Intervall, t.Cutoff = s.Cutoff,
               t.Mail_Mittente = s.Mail_Mittente, t.Mail_Failure = s.Mail_Failure,
               t.Path_Archiviazione = s.Path_Archiviazione, t.Mail_success = s.Mail_success,
               t.LasciaInSource = s.LasciaInSource
WHEN NOT MATCHED THEN
    INSERT (Istituto, IdProcesso, Descrizione, Path_Source, File_Source, Orario_Check_File,
            Day_Check_File, Polling_Intervall, Cutoff, Mail_Mittente, Mail_Failure,
            Path_Archiviazione, Mail_success, LasciaInSource)
    VALUES (s.Istituto, s.IdProcesso, s.Descrizione, s.Path_Source, s.File_Source,
            s.Orario_Check_File, s.Day_Check_File, s.Polling_Intervall, s.Cutoff,
            s.Mail_Mittente, s.Mail_Failure, s.Path_Archiviazione, s.Mail_success,
            s.LasciaInSource);
GO

PRINT N'seed.sql completato: dati sintetici inseriti.';
GO
