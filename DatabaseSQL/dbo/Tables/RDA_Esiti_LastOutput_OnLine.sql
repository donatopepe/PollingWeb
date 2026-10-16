CREATE TABLE [dbo].[RDA_Esiti_LastOutput_OnLine] (
    [Num_Ordine]      NCHAR (10)    NOT NULL,
    [Mandante]        NCHAR (3)     NULL,
    [Societa]         NCHAR (4)     NULL,
    [Posizione]       NCHAR (3)     NULL,
    [Cod_Errore]      NVARCHAR (5)  NULL,
    [Desc_Errore]     NVARCHAR (55) NULL,
    [Data_FileOutput] DATETIME      NULL,
    CONSTRAINT [PK_RDA_Esiti_LastOutput_OnLine] PRIMARY KEY CLUSTERED ([Num_Ordine] ASC)
);
