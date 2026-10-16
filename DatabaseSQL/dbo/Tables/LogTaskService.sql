CREATE TABLE [dbo].[LogTaskService] (
    [Istituto]         SMALLINT       NOT NULL,
    [Data]             DATETIME       NOT NULL,
    [IdProcesso]       NVARCHAR (20)  NOT NULL,
    [Ora_Inizio]       DATETIME       NOT NULL,
    [Ora_fine]         DATETIME       NULL,
    [Codice_stato]     SMALLINT       NULL,
    [Messaggio_Return] NVARCHAR (100) NULL,
    CONSTRAINT [PK_Letture] PRIMARY KEY CLUSTERED ([Istituto] ASC, [Data] ASC, [IdProcesso] ASC, [Ora_Inizio] ASC)
);

