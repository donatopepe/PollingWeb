IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimCentridiResp_FirmaCdRAutomatica_1' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimCentridiResp] DROP CONSTRAINT [DF_DimCentridiResp_FirmaCdRAutomatica_1];
END
GO
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = N'DF_DimCentridiResp_RichiestaFirmaCM' AND type IN (N'F', N'D', N'PK', N'UQ', N'C'))
BEGIN
    ALTER TABLE [dbo].[DimCentridiResp] DROP CONSTRAINT [DF_DimCentridiResp_RichiestaFirmaCM];
END
GO
/****** Object:  Table [dbo].[DimCentridiResp]    Script Date: 18/07/2022 10:15:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DimCentridiResp]') AND type in (N'U'))
DROP TABLE [dbo].[DimCentridiResp]
GO
/****** Object:  Table [dbo].[DimCentridiResp]    Script Date: 18/07/2022 10:15:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DimCentridiResp](
	[UoCode] [nvarchar](6) NOT NULL,
	[IstID] [int] NOT NULL,
	[Codice] [nvarchar](5) NULL,
	[CodiceColl] [nvarchar](5) NULL,
	[Codice_OLD] [nvarchar](5) NULL,
	[CodiceColl_OLD] [nvarchar](5) NULL,
	[NAut] [int] NULL,
	[ImportoMaxOper] [float] NULL,
	[Plafond] [float] NULL,
	[RichiestaFirmaCM] [int] NULL,
	[FirmaCdRAutomatica] [int] NULL,
	[NomeFileFirmaCdR] [nvarchar](150) NULL,
 CONSTRAINT [PK_DimCentridiResp] PRIMARY KEY CLUSTERED 
(
	[UoCode] ASC,
	[IstID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
