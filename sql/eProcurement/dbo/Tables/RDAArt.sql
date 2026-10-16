IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_RDAArt_DimFornitori2' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [FK_RDAArt_DimFornitori2];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_RDAArt_DimFornitori1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [FK_RDAArt_DimFornitori1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_RDAArt_DimFornitori' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [FK_RDAArt_DimFornitori];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_Per_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_Per_1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_TipoContabilizzazione_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_TipoContabilizzazione_1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_CostoConcordato' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_CostoConcordato];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_CostoConcordatoNoIva' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_CostoConcordatoNoIva];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_CostoTotale1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_CostoTotale1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RDAArt_CostoTotale' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RDAArt] DROP CONSTRAINT [DF_RDAArt_CostoTotale];
END
GO
/****** Object:  Table [dbo].[RDAArt]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RDAArt]') AND type in (N'U'))
DROP TABLE [dbo].[RDAArt]
GO
/****** Object:  Table [dbo].[RDAArt]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RDAArt](
	[IDArt] [int] NOT NULL,
	[RdAID] [uniqueidentifier] NOT NULL,
	[CatID] [int] NULL,
	[SCID] [int] NULL,
	[SapID] [int] NULL,
	[ForID] [int] NULL,
	[ForSceltoID] [int] NULL,
	[ForAlternativoID1] [int] NULL,
	[ForAlternativoID2] [int] NULL,
	[ForAlternativoID3] [int] NULL,
	[ForNuovo] [nvarchar](300) NULL,
	[DescrDett] [nvarchar](2500) NULL,
	[Qta] [int] NULL,
	[UmsID] [int] NULL,
	[CostoStimatoNoIva] [float] NULL,
	[CostoStimatoPerc] [int] NULL,
	[CostoStimato] [float] NULL,
	[CostoEffettivo] [float] NULL,
	[CostoTotale] [float] NULL,
	[CostoUnitarioConsumo] [float] NULL,
	[DescrDett_Ord] [nvarchar](2000) NULL,
	[QtaC] [int] NULL,
	[CostoConcordatoNoIva] [float] NULL,
	[CodiceIVAConcordato] [nvarchar](2) NULL,
	[CostoConcordatoPerc] [int] NULL,
	[CostoConcordato] [float] NULL,
	[Note] [nvarchar](1500) NULL,
	[DataRichCons] [datetime] NULL,
	[DataPrevCons] [datetime] NULL,
	[DataCreazione] [datetime] NULL,
	[DataModifica] [datetime] NULL,
	[Intervento] [int] NULL,
	[InterventoID] [int] NULL,
	[SedeID] [uniqueidentifier] NULL,
	[PrezzoConvenzione] [float] NULL,
	[PrezzoAggiuntivo] [float] NULL,
	[TipoContabilizzazione] [nvarchar](1) NULL,
	[DataInizioCompetenza] [datetime] NULL,
	[DataFineCompetenza] [datetime] NULL,
	[ScadenzaVigente] [datetime] NULL,
	[GruppoMerci] [nvarchar](9) NULL,
	[Categ1] [nvarchar](10) NULL,
	[CodiceIVA] [nvarchar](2) NULL,
	[Cespite] [nvarchar](12) NULL,
	[Per] [int] NULL,
	[Fondo] [nvarchar](10) NULL,
	[GUIDArt] [uniqueidentifier] NULL,
 CONSTRAINT [PK_RDAArt] PRIMARY KEY CLUSTERED 
(
	[IDArt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
