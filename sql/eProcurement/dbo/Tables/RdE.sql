IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'FK_RdE_RdA' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdE] DROP CONSTRAINT [FK_RdE_RdA];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_RdE_Consulenza_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[RdE] DROP CONSTRAINT [DF_RdE_Consulenza_1];
END
GO
/****** Object:  Table [dbo].[RdE]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RdE]') AND type in (N'U'))
DROP TABLE [dbo].[RdE]
GO
/****** Object:  Table [dbo].[RdE]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RdE](
	[GUID] [uniqueidentifier] NOT NULL,
	[Progressivo] [int] NOT NULL,
	[Matricola] [nvarchar](7) NULL,
	[PageID] [int] NOT NULL,
	[CatID] [int] NULL,
	[Status] [int] NULL,
	[RdAID] [uniqueidentifier] NULL,
	[Motivazione] [nvarchar](1000) NULL,
	[MotivazioneAnnulla] [nvarchar](500) NULL,
	[Tipomod] [numeric](18, 0) NULL,
	[UOCode] [nvarchar](6) NULL,
	[IstID] [int] NOT NULL,
	[UOCode1] [nvarchar](6) NULL,
	[IstID1] [int] NULL,
	[Destinazione] [nvarchar](6) NULL,
	[IstIDDestinazione] [int] NULL,
	[DataRichCons] [datetime] NULL,
	[DataCreazione] [datetime] NULL,
	[DataModifica] [datetime] NULL,
	[Canale] [nvarchar](2) NULL,
	[IDRichiesta] [uniqueidentifier] NULL,
	[TacID] [int] NULL,
	[CodPrg] [nvarchar](6) NULL,
	[CdRUOCode] [nvarchar](6) NULL,
	[CdRIstID] [int] NULL,
	[Consulenza] [int] NULL,
 CONSTRAINT [PK_RdE_1] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
