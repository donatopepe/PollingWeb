CREATE TABLE [dbo].[DimStato] (
    [Stato]             SMALLINT       NOT NULL,
    [Descrizione_Stato] NVARCHAR (100) NULL,
    CONSTRAINT [PK_DimStato] PRIMARY KEY CLUSTERED ([Stato] ASC)
);

