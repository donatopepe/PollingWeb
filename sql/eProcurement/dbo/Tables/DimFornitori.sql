IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimFornitori_IsActive' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_DimFornitori_IsActive];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimFornitori_ParteCorrelata_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_DimFornitori_ParteCorrelata_1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_Fornitori_Strategico' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_Fornitori_Strategico];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimFornitori_Istituzionale' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_DimFornitori_Istituzionale];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimFornitori_InAttesa' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_DimFornitori_InAttesa];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimFornitori_IscrittoAF' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimFornitori] DROP CONSTRAINT [DF_DimFornitori_IscrittoAF];
END
GO
/****** Object:  Table [dbo].[DimFornitori]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimFornitori]') AND type in (N'U'))
DROP TABLE [dbo].[DimFornitori]
GO
/****** Object:  Table [dbo].[DimFornitori]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DimFornitori](
	[IDFor] [int] IDENTITY(1,1) NOT NULL,
	[IstID] [smallint] NULL,
	[CFor] [int] NULL,
	[RagSoc] [nvarchar](200) NULL,
	[IscrittoAF] [int] NULL,
	[InAttesa] [int] NULL,
	[Istituzionale] [int] NULL,
	[Strategico] [int] NULL,
	[Monopolio] [int] NULL,
	[Convenzione] [int] NULL,
	[GruppiAcquisto] [int] NULL,
	[CodFisc] [nvarchar](50) NULL,
	[PIva] [nvarchar](50) NULL,
	[Indirizzo] [nvarchar](50) NULL,
	[Cap] [nvarchar](50) NULL,
	[Citta] [nvarchar](50) NULL,
	[Prov] [text] NULL,
	[Nazione] [nvarchar](50) NULL,
	[Tel] [nvarchar](50) NULL,
	[Fax] [nvarchar](50) NULL,
	[Email] [nvarchar](100) NULL,
	[Web] [nvarchar](100) NULL,
	[Contatto] [nvarchar](300) NULL,
	[Professione] [nvarchar](50) NULL,
	[Cellulare] [nvarchar](50) NULL,
	[CapSoc] [numeric](18, 0) NULL,
	[Referenze] [nvarchar](1000) NULL,
	[Cdc1ID] [int] NULL,
	[ParereCdc1] [nvarchar](30) NULL,
	[Cdc2ID] [int] NULL,
	[ParereCdc2] [nvarchar](30) NULL,
	[Cdc3ID] [int] NULL,
	[ParereCdc3] [nvarchar](30) NULL,
	[Cdc4ID] [int] NULL,
	[ParereCdc4] [nvarchar](30) NULL,
	[ScoreID] [int] NULL,
	[Note] [varchar](500) NULL,
	[DataCreazione] [datetime] NULL,
	[DataModifica] [datetime] NULL,
	[Creatore] [nvarchar](7) NULL,
	[ParteCorrelata] [int] NULL,
	[IsActive] [int] NULL,
 CONSTRAINT [PK_for] PRIMARY KEY CLUSTERED 
(
	[IDFor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
