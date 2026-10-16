CREATE TABLE [dbo].[Tbl_Export_SAP_OnLine] (
    [ID]          INT             IDENTITY (1, 1) NOT NULL,
    [Riga]        NVARCHAR (MAX)  NULL,
    [Spid]        INT             NULL,
    [Codist]      SMALLINT        NULL,
    [Elaborato]   BIT             NULL,
    [Tiporecord]  NVARCHAR (3)    NULL,
    CONSTRAINT [PK_Tbl_Export_SAP_OnLine] PRIMARY KEY CLUSTERED ([ID] ASC)
);
