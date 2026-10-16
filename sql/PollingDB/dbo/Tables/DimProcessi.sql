CREATE TABLE [dbo].[DimProcessi] (
    [IdProcesso]  NVARCHAR (20) NOT NULL,
    [Descrizione] NVARCHAR (50) NULL,
    [Tipo]        SMALLINT      NULL,
    CONSTRAINT [PK_DimProcessi] PRIMARY KEY CLUSTERED ([IdProcesso] ASC)
);

