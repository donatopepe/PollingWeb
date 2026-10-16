IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdA_Frequenza_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdA] DROP CONSTRAINT [DF_RdA_Frequenza_1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdA_ParteCorrelata' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdA] DROP CONSTRAINT [DF_RdA_ParteCorrelata];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdA_IsDelega' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdA] DROP CONSTRAINT [DF_RdA_IsDelega];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdA_FlagDone' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdA] DROP CONSTRAINT [DF_RdA_FlagDone];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdA_Multipla' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdA] DROP CONSTRAINT [DF_RdA_Multipla];
END
GO
/****** Object:  Table [dbo].[RdA]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RdA]') AND type in (N'U'))
DROP TABLE [dbo].[RdA]
GO
/****** Object:  Table [dbo].[RdA]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RdA](
	[GUID] [uniqueidentifier] NOT NULL,
	[Progressivo] [int] NOT NULL,
	[PageID] [int] NOT NULL,
	[Istituto] [int] NULL,
	[Oggetto] [nvarchar](250) NULL,
	[TacID] [int] NULL,
	[TacIDBuyer] [int] NULL,
	[CdRUOCode] [nvarchar](6) NULL,
	[CdRIstID] [int] NULL,
	[CatID] [int] NULL,
	[Matricola] [nvarchar](7) NOT NULL,
	[Status] [int] NULL,
	[UOCode] [nvarchar](6) NULL,
	[IstID] [int] NOT NULL,
	[UOCode1] [nvarchar](6) NULL,
	[IstID1] [int] NULL,
	[Destinazione] [nvarchar](6) NULL,
	[IstIDDestinazione] [int] NULL,
	[CodMta] [nchar](2) NULL,
	[NOrdSAP] [float] NULL,
	[NOrdSAPInt] [float] NULL,
	[Motivazione] [char](500) NULL,
	[Tipomod] [numeric](18, 0) NULL,
	[MotivazioneAnnulla] [char](500) NULL,
	[DatiMancanti] [char](500) NULL,
	[Multipla] [bit] NULL,
	[Manuale] [bit] NULL,
	[CostoTotale] [float] NULL,
	[CostoTotConcordato] [float] NULL,
	[PDAut] [nvarchar](6) NULL,
	[ImpOrd] [float] NULL,
	[ImpInt] [float] NULL,
	[DataCreazione] [datetime] NULL,
	[DataModifica] [datetime] NULL,
	[FlagDone] [int] NULL,
	[NoteDelibera] [nvarchar](200) NULL,
	[IsDelega] [int] NULL,
	[Verificato] [int] NULL,
	[Canale] [nvarchar](2) NULL,
	[IDRichiesta] [uniqueidentifier] NULL,
	[ParteCorrelata] [int] NULL,
	[TipoOrdine] [nvarchar](2) NULL,
	[ForID] [int] NULL,
	[CFor] [int] NULL,
	[ForSceltoID] [int] NULL,
	[ForNuovo] [nvarchar](300) NULL,
	[GruppoAcquisti] [nvarchar](3) NULL,
	[PotereDelegato] [nvarchar](2) NULL,
	[Frequenza] [int] NULL,
	[FqID] [int] NULL,
	[DataInizioCompetenza] [datetime] NULL,
	[DataFineCompetenza] [datetime] NULL,
	[NoteAggTestata] [nvarchar](2000) NULL,
	[NoteAggPiede] [nvarchar](2000) NULL,
	[IndiConsNoSTD] [nvarchar](1500) NULL,
	[GUIDCtr] [uniqueidentifier] NULL,
	[CodPrg] [nvarchar](6) NULL,
	[DispBudget] [int] NULL,
	[IsDerogata] [int] NULL,
	[TipoRdAID] [int] NULL,
	[TpsRdAID] [int] NULL,
	[CompetenzeIAnno] [float] NULL,
	[CompetenzeIIAnno] [float] NULL,
	[CompetenzeIIIAnno] [float] NULL,
	[IsExArt81] [int] NULL,
	[CentroDiCosto] [nvarchar](10) NULL,
 CONSTRAINT [PK_RdA] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
